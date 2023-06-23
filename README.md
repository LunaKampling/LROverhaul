![logo](./LRO-Logo.svg)

# About
[Line Rider: Advanced](https://github.com/jealouscloud/linerider-advanced) (or LRA) is an open source spiritual successor to the flash game Line Rider 6.2 with lots of new features. Not updated since 2018.

[Line Rider Advanced: Community Edition](https://github.com/RatherBeLunar/LRA-Community-Edition) (or LRA:CE) is a fork of LRA. The project's goal was to unify the best features from several existing forks (such as [LRTran](https://github.com/Tran-Foxxo/LRTran)) into one version. Not updated since 2020.

**Line Rider Overhaul** (or LRO) is a restoration project of LRA:CE. It has updated graphics, lots of bug fixes and new features to make Line Rider more user friendly.

# Downloads
You can download the latest version from [here](https://github.com/LunaKampling/LROverhaul/releases/tag/Initial).
* **Windows**: Windows older than Win10 19H1 (May 2019 update) may require [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) to be installed manually.
* **Mac/Linux**: You will need the [mono framework](http://www.mono-project.com/download/stable/) installed in order to run the executable file.

# Features
* Updated UI with scalable graphics
* New tool: smooth pencil
* Iteration-based contact point coordinate viewer
* Copy paste preservation of line data to transfer between tracks
* Ability to change line type of a selection
* Customizable UI colors, better night mode support
* New Bosh sprite and [user skin and scarf handlers](/Examples)

# Build
Clone the repository and build `src/linerider.sln` with msbuild or Visual Studio.

This project requires .NET Framework 4.8 and C# 7 support.

# Issues
We are tracking the development, bug reports and feature requests on [Trello](https://trello.com/invite/b/qu4SvIr6/ATTI0ac1327b122a1cf4d1084b9d7b8acb0dB9177B71/lrl-cleanup-update). If whatever you wish to report isn't present there, just add it :>

# Libraries
This project uses binaries, sources, or modified sources from the following libraries:

* ffmpeg https://ffmpeg.org/
* NVorbis https://github.com/ioctlLR/NVorbis
* gwen-dotnet https://code.google.com/archive/p/gwen-dotnet/
* OpenTK https://github.com/opentk/opentk
* Newtonsoft Json.NET https://github.com/JamesNK/Newtonsoft.Json
* SVG.NET https://github.com/svg-net/SVG

You can find their license info in [LICENSES.txt](/LICENSES.txt)

The UI is a modified modified version of gwen-dotnet by jealouscloud

# License
LRO is licensed under GPL3.

Line Rider is a registered trademark of Boštjan Čadež.