# About Line Rider Advanced: Community Edition
Line Rider Advanced: Community Edition, abbreviated as LRA:CE, is a fork of https://github.com/jealouscloud/linerider-advanced; "An open source spiritual successor to the flash game Line Rider 6.2 with lots of new features."

This project's goal is to unify the best features from several existing forks of the original Line Rider Advanced all into one version. Once this version is caught up with all others, we, the contributors, will aim to continue development of new features in this repo as opposed to branching off into new forks again.

# Instructions
You can download the latest version from https://github.com/RatherBeLunar/LRA-Community-Edition/releases

## Windows
If you can't run the application, you probably need to install [.net 4.6](https://www.microsoft.com/en-us/download/details.aspx?id=48130) which is a requirement for running LRA.
## Mac/Linux
You will need the [mono framework](http://www.mono-project.com/download/stable/) installed in order to run LRA:CE.

# LRA:CE Features
* Discord activity support (Aka little stats when you click your user)
* Custom scarves from a .txt file in /LRA/Scarves -> [/Examples/Scarves/README.md](https://github.com/RatherBeLunar/LRA-Community-Edition/tree/master/Examples/Scarves/README.md)
* Custom amount of scarf segments
* Custom riders in /LRA/Riders -> [/Examples/Riders/README.md](https://github.com/RatherBeLunar/LRA-Community-Edition/tree/master/Examples/Riders/README.md)
* Custom scarves on a rider png -> [/Examples/Riders/Bosh-Custom-Scarf-On-Png-Example/README.md](https://github.com/RatherBeLunar/LRA-Community-Edition/tree/master/Examples/Riders/Bosh-Custom-Scarf-On-Png-Example/README.md)

# Issues
We are tracking issues on https://trello.com/b/0RGXoFZQ/lra-community-edition. If the issue you wish to report isn't present there, please submit an issue here on Github, and we will add it to the Trello board.

# Build
First extract the source code and download [gwen-lra](https://github.com/jealouscloud/gwen-lra/tree/dbe3e84568b163f3e20cd876672fc1b3b0e40873)'s source code and extract it to the /lib/gwen-lra/ folder
Run nuget restore in src (Visual Studio (not VS Code) will do this for you)
Build src/linerider.sln with msbuild or Visual Studio
Download Discord's Game SDK at [https://dl-game-sdk.discordapp.net/latest/discord_game_sdk.zip](https://dl-game-sdk.discordapp.net/latest/discord_game_sdk.zip) and copy /lib/x86/discord_game_sdk.dll to the build directory. (If that doesn't work for some reason try x86_64)

This project requires .net 4.6 and C# 7 support.

# Libraries
This project uses binaries, sources, or modified sources from the following libraries:

* ffmpeg https://ffmpeg.org/
* NVorbis https://github.com/ioctlLR/NVorbis
* gwen-dotnet https://code.google.com/archive/p/gwen-dotnet/
* OpenTK https://github.com/opentk/opentk
* Discord Game SDK https://discord.com/

You can find their license info in LICENSES.txt

The UI is a modified modified version of gwen-dotnet by jealouscloud

# License
LRA:CE is licensed under GPL3.
