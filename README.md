# WindowsGSM.ProjectZomboid
🧩 WindowsGSM plugin for supporting Project Zomboid Dedicated Server 

## Requirements
[WindowsGSM](https://github.com/WindowsGSM/WindowsGSM) >= 1.21.0

## Installation
1. Download the [latest](https://github.com/DoctorBeardz/WindowsGSM.ProjectZomboid/releases/latest) release
1. Move **ProjectZomboid.cs** folder to **plugins** folder
1. Click **[RELOAD PLUGINS]** button or restart WindowsGSM

## Info
This is a quickly hacked together Project Zomboid Dedicated Server for WindowsGSM. It will be necessary to edit the proper .ini files for all settings to be used properly. This has been mostly done for myself to use the autostart and Discord features of WindowsGSM and is definitely not polished.

Most code and inspiration taken from:

[BattlefieldDuck](https://github.com/BattlefieldDuck/WindowsGSM.ARMA3)

[1stian](https://github.com/1stian/WindowsGSM.Spigot)

## Features

What it will do:

- Create a "Zomboid" folder in the main server file folder for centralized saves, no longer using the default Zomboid folder in %User%
- Use the server name instead of the servertest.ini
- Use the port set up in WindowsGSM

What it will NOT do:

- Set the server name
- Set the max player number
- Set any maps, mods or tweaks

All of that needs to be done by editing the .ini files in the "serverfiles\Zomboid\Server" folder


### License
This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/DoctorBeardz/WindowsGSM.ProjectZomboid/blob/main/LICENSE) file for details
