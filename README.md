![logo](./LRO-Logo.svg)

# 🛷 About
[Line Rider: Advanced](https://github.com/jealouscloud/linerider-advanced) (or LRA) is an open source spiritual successor to the flash game Line Rider 6.2 with lots of new features. Not updated since 2018.

[Line Rider Advanced: Community Edition](https://github.com/RatherBeLunar/LRA-Community-Edition) (or LRA:CE) is a fork of LRA. The project's goal was to unify the best features from several existing forks (such as [LRTran](https://github.com/Tran-Foxxo/LRTran)) into one version. Not updated since 2020.

**Line Rider Overhaul** (or LRO) is a restoration project of LRA:CE. It has updated graphics, lots of bug fixes and new features to make Line Rider more user friendly.

# 💾 Downloads
You can download the latest version from [here](https://github.com/LunaKampling/LROverhaul/releases) (note this is not the same as the main branch).

LRO requires [.NET Desktop Runtime 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0). You will be suggested to download and install it if it is not yet.

# ✨ Features
* Native cross platform builds (no Mono framework required).
* Updated UI with scalable graphics.
* New tool: smooth pencil.
* Iteration-based contact point coordinate viewer.
* Copy paste preservation of line data to transfer between tracks.
* Ability to change line type of a selection.
* Customizable UI colors, better night mode support.
* New Bosh sprite and [user skin and scarf handlers](/Examples).

# 🛠️ Build
Clone the repository and either build `src/linerider.sln` with Visual Studio or run `dotnet build` from the `src` folder.

This project requires .NET 8.

# 🐛 Issues
We are tracking the development, bug reports and feature requests on [GitHub](https://github.com/LunaKampling/LROverhaul/issues). If whatever you wish to report isn't present there, just add it :>

# 📚 Libraries
This project uses resources, binaries, sources, or modified sources from the following libraries:

* [ffmpeg](https://ffmpeg.org/)
* [NVorbis](https://github.com/ioctlLR/NVorbis)
* [gwen-dotnet](https://code.google.com/archive/p/gwen-dotnet/)
* [OpenTK](https://github.com/opentk/opentk)
* [Newtonsoft Json.NET](https://github.com/JamesNK/Newtonsoft.Json)
* [SVG.NET](https://github.com/svg-net/SVG)
* [Material Design Icons](https://pictogrammers.com/library/mdi/)
* The UI is a modified version of [gwen-dotnet](https://github.com/jealouscloud/gwen-lra) by [jealouscloud](https://github.com/jealouscloud)
* The in-game cursors are modified version of [Posy cursors](http://www.michieldb.nl/other/cursors) by [Michiel de Boer](https://www.youtube.com/user/PosyMusic)

# 📝 Licenses
Library licenses can be found in [LICENSES.txt](/LICENSES.txt)

LRO is licensed under GPL3.

Line Rider is a registered trademark of Boštjan Čadež.
