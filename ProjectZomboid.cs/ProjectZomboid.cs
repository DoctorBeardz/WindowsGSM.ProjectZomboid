using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Engine;
using WindowsGSM.GameServer.Query;

namespace WindowsGSM.Plugins
{
	public class ProjectZomboid : SteamCMDAgent // SteamCMDAgent is used because Project Zomboid relies on SteamCMD for installation and update process
	{
		// - Plugin Details
		public Plugin Plugin = new Plugin
		{
			name = "WindowsGSM.ProjectZomboid", // WindowsGSM.XXXX
			author = "Beard",
			description = "🧩 WindowsGSM plugin for supporting Project Zomboid Dedicated Server",
			version = "1.5",
			url = "https://github.com/DoctorBeardz/WindowsGSM.ProjectZomboid", // Github repository link (Best practice)
			color = "#38CDD4" // Color Hex
		};

		// - Standard Constructor and properties
		public ProjectZomboid(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
		private readonly ServerConfig _serverData; // Store server start metadata, such as start ip, port, start param, etc

		// - Settings properties for SteamCMD installer
		public override bool loginAnonymous => true; // Project Zomboid does not require a steam account to install the server, so loginAnonymous = true
		public override string AppId => "380870"; // Game server appId, Project Zomboid is 380870

		// - Game server Fixed variables
		public override string StartPath => "jre64\\bin\\java.exe"; // Game server start path, for Project Zomboid, it is StartServer64.bat
		public string FullName = "Project Zomboid Server"; // Game server FullName
		public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
		public int PortIncrements = 1; // This tells WindowsGSM how many ports should skip after installation
		public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()

		// - Game server default values
		public string Port = "16261"; // Default port
		public string QueryPort = "16261"; // Default query port
		public string Defaultmap = "Muldraugh, KY"; // Default map name
		public string Maxplayers = "64"; // Default maxplayers
		public string Additional = ""; // Additional server start parameter

		// - Create a default cfg for the game server after installation
		public async void CreateServerCFG()
		{
			var startScript = File.ReadAllText(ServerPath.GetServersServerFiles(_serverData.ServerID, "StartServer64.bat")); // Fetches content of old startup script.
			var sb = new StringBuilder(startScript);
			sb.Replace("-Dzomboid.znetlog=1", "-Dzomboid.znetlog=1 -Duser.home=.."); // Adds a Java parameter to change home location.
			File.WriteAllText(ServerPath.GetServersServerFiles(_serverData.ServerID, "StartServer64.bat"), sb.ToString()); // Saves new startup script, replacing the old.
		}

		// - Start server function, return its Process to WindowsGSM
		public async Task<Process> Start()
		{
			var param = new StringBuilder();
			param.Append("\"-Djava.awt.headless=true\" \"-Dzomboid.steam=1\" \"-Dzomboid.znetlog=1\" \"-Duser.home=..\" \"-XX:+UseZGC\" \"-XX:-CreateCoredumpOnCrash\" \"-XX:-OmitStackTraceInFastThrow\"");
            
			//if you have Memory issues you can try to edit -Xms16g -Xmx16g to better suite your system (if you only have 16g you maybe should adjust it to 8g or even 4g if you have other servers running)
            param.Append(" -Xms16g -Xmx16g \"-Djava.library.path=natives/;natives/win64/;.\"");
            //java classpath copied from startScript
			param.Append(" -cp \"java/istack-commons-runtime.jar;java/jassimp.jar;java/javacord-2.0.17-shaded.jar;java/javax.activation-api.jar;java/jaxb-api.jar;java/jaxb-runtime.jar;java/lwjgl.jar;java/lwjgl-natives-windows.jar;java/lwjgl-glfw.jar;java/lwjgl-glfw-natives-windows.jar;java/lwjgl-jemalloc.jar;java/lwjgl-jemalloc-natives-windows.jar;java/lwjgl-opengl.jar;java/lwjgl-opengl-natives-windows.jar;java/lwjgl_util.jar;java/sqlite-jdbc-3.27.2.1.jar;java/trove-3.0.3.jar;java/uncommons-maths-1.2.3.jar;java/commons-compress-1.18.jar;java/\"");
            param.Append(" zombie.network.GameServer \"-statistic 0\"");
			//add custom parameters and ports
            param.Append($" -port {_serverData.ServerPort} {_serverData.ServerParam} ");

			// Prepare Process
			var p = new Process
			{
				StartInfo =
				{
					WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
					FileName = ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath),
					Arguments = param.ToString(),
					WindowStyle = ProcessWindowStyle.Minimized,
					UseShellExecute = false,
				},
				EnableRaisingEvents = true,
			};

			// Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
			if (AllowsEmbedConsole)
			{
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.RedirectStandardInput = true;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;
				var serverConsole = new ServerConsole(_serverData.ServerID);
				p.OutputDataReceived += serverConsole.AddOutput;
				p.ErrorDataReceived += serverConsole.AddOutput;

				// Start Process
				try
				{
					p.Start();
				}
				catch (Exception e)
				{
					Error = e.Message;
					return null; // return null if fail to start
				}

				p.BeginOutputReadLine();
				p.BeginErrorReadLine();
				return p;
			}

			// Start Process
			try
			{
				p.Start();
				return p;
			}
			catch (Exception e)
			{
				Error = e.Message;
				return null; // return null if fail to start
			}
		}

		// - Stop server function
		public async Task Stop(Process p)
		{
			await Task.Run(() =>
			{
				if (p.StartInfo.RedirectStandardInput)
				{
					// Send "quit" command to StandardInput stream if EmbedConsole is on
					p.StandardInput.WriteLine("quit");
				}
				else
				{
					// Send "quit" command to game server process MainWindow
					ServerConsole.SendMessageToMainWindow(p.MainWindowHandle, "quit");
				}
			});
		}
	}
}
