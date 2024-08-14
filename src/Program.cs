//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using linerider.Utils;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace linerider
{
    public static class Program
    {
        public static string BinariesFolder = "bin";
        public static readonly CultureInfo Culture = new CultureInfo("en-US");
        public static string NewVersion = null;
        public static readonly string WindowTitle = AssemblyInfo.Title + " \u22C5 " + AssemblyInfo.FullVersion;
        public static Random Random;
        private static bool _crashed;
        private static MainWindow glGame;
        private static string _currdir;
        private static string _userdir;
        public static string[] args;

        /// <summary>
        /// Gets the default user data directory. Ends in Path.DirectorySeperator
        /// </summary>
        public static string UserDirectory
        {
            get
            {
                if (_userdir == null)
                {
                    _userdir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    // Mono doesn't do well with non windows ~/Documents.
                    if (_userdir == Environment.GetFolderPath(Environment.SpecialFolder.Personal))
                    {
                        string documents = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Documents");
                        // So, if we can find a Documents folder, we use that.
                        // Otherwise we're just gonna use ~/LRA, unfortunately.
                        if (Directory.Exists(documents))
                        {
                            _userdir = documents;
                        }
                    }
                    // Linux support: some users use XDG_DOCUMENTS_DIR to change
                    // their Documents directory location.
                    string xdg_dir = Environment.GetEnvironmentVariable("XDG_DOCUMENTS_DIR");
                    if (!string.IsNullOrEmpty(xdg_dir) && Directory.Exists(xdg_dir))
                    {
                        _userdir = xdg_dir;
                    }
                    _userdir += Path.DirectorySeparatorChar + Constants.UserDirFolderName + Path.DirectorySeparatorChar;
                }
                return _userdir;
            }
        }
        /// <summary>
        /// Gets the portable user data directory. Ends in Path.DirectorySeperator
        /// </summary>
        public static string UserPortableDirectory => Path.Combine(CurrentDirectory, Constants.UserDirPortableFolderName) + Path.DirectorySeparatorChar;
        /// <summary>
        /// Gets the directory where the executable file is placed. Ends in Path.DirectorySeperator
        /// </summary>
        public static string CurrentDirectory
        {
            get
            {
                if (_currdir == null)
                    _currdir = AppDomain.CurrentDomain.BaseDirectory;
                return _currdir;
            }
        }

        public static void Crash(Exception e, bool nothrow = false)
        {
            if (!_crashed)
            {
                _crashed = true;
                try
                {
                    glGame.Track.BackupTrack();
                }
                catch
                { }
            }

            string logDir = Settings.Local.UserDirPath;
            if (logDir == null || !Directory.Exists(Settings.Local.UserDirPath))
                logDir = CurrentDirectory;

            string logFilePath = logDir + "log.txt";
            string msgBoxSpearator = Environment.NewLine + Environment.NewLine;
            string title = "Game crashed!";
            string msg = $"Unhandled Exception: {e.Message}{msgBoxSpearator}{e.StackTrace}{msgBoxSpearator}{msgBoxSpearator}Would you like to export the crash data to a log file?{msgBoxSpearator}Log file path: {logFilePath}";

            if (OperatingSystem.IsWindows()) {
                /* TODO MessageBox from user32.dll instead of winforms
                System.Windows.Forms.DialogResult btn = System.Windows.Forms.MessageBox.Show(msg, title, System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Error);

                if (btn == System.Windows.Forms.DialogResult.Yes)
                {
                    string now = DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss");
                    string headerSeparator = new string('-', 3);
                    string header = $"{headerSeparator} {AssemblyInfo.FullVersion} {headerSeparator} {now} {headerSeparator}";
                    string newLine = "\r\n";

                    if (!File.Exists(logFilePath))
                        File.Create(logFilePath).Dispose();

                    string oldRecords = File.ReadAllText(logFilePath, System.Text.Encoding.UTF8);
                    string newRecord = header + newLine + newLine + e.ToString() + newLine;
                    if (!string.IsNullOrEmpty(oldRecords))
                        newRecord = newLine + newRecord;

                    File.WriteAllText(logFilePath, oldRecords + newRecord, System.Text.Encoding.UTF8);
                }*/
            }

            if (!nothrow)
                throw e;
        }

        public static void NonFatalError(string err) { 
            if (OperatingSystem.IsWindows()) {
                /* TODO MessageBox from user32.dll instead of winforms
                System.Windows.Forms.MessageBox.Show("Non Fatal Error: " + err);
            */}
        }
        public static void Run(string[] givenArgs)
        {
            if (Debugger.IsAttached)
                System.Diagnostics.Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            else
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            args = givenArgs;
            Settings.Load();

            Random = new Random();
            GameResources.Init();

            using (Toolkit.Init(new ToolkitOptions { EnableHighResolution = true, Backend = PlatformBackend.Default }))
            {
                using (glGame = new MainWindow())
                {
                    UI.InputUtils.SetWindow(glGame);
                    glGame.RenderSize = new System.Drawing.Size(glGame.Width, glGame.Height);
                    Rendering.GameRenderer.Game = glGame;
                    MemoryStream ms = new MemoryStream(GameResources.icon);
                    glGame.Icon = new System.Drawing.Icon(ms);

                    ms.Dispose();
                    glGame.Title = WindowTitle;
                    glGame.Run(Constants.FrameRate, 0); // TODO: Maybe not limit this
                }
                Audio.AudioService.CloseDevice();
            }
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Crash((Exception)e.ExceptionObject);

            throw (Exception)e.ExceptionObject;
        }
        public static void UpdateCheck()
        {
            string subVer = AssemblyInfo.SubVersion;
            if (subVer == "closed" || subVer == "test")
                return;

            if (Settings.CheckForUpdates)
            {
                new System.Threading.Thread(() =>
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            string recentVersion = wc.DownloadString($"{Constants.GithubRawHeader}/main/version").Trim();

                            if (recentVersion.Length > 0)
                            { 
                                try
                                {
                                    // Try to check every number of N.N.N.N format
                                    List<int> current = AssemblyInfo.Version.Split('.').Select(int.Parse).ToList();
                                    List<int> recent = recentVersion.Split('.').Select(int.Parse).ToList();

                                    if (recent[0] > current[0] || recent[1] > current[1] || recent[2] > current[2] || recent[3] > current[3])
                                        NewVersion = recentVersion;
                                }
                                catch
                                {
                                    // Fallback to string comparison
                                    if (recentVersion != AssemblyInfo.Version)
                                        NewVersion = recentVersion;
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                })
                {
                    IsBackground = true
                }.Start();
            }
        }
        public static int GetWindowWidth() => glGame.Width;
        public static int GetWindowHeight() => glGame.Height;
    }

    internal static class AssemblyInfo
    {
        public static string Title => GetExecutingAssemblyAttribute<AssemblyTitleAttribute>(a => a.Title);
        public static string Version => GetExecutingAssemblyAttribute<AssemblyFileVersionAttribute>(a => a.Version);
        public static string FullVersion => GetExecutingAssemblyAttribute<AssemblyInformationalVersionAttribute>(a => a.InformationalVersion);
        public static string SubVersion => Assembly.GetExecutingAssembly().GetCustomAttribute<CustomAttributes>().SubVersion;
        public static List<string> ChangelogLines => Assembly.GetExecutingAssembly().GetCustomAttribute<CustomAttributes>().Changelog;

        private static string GetExecutingAssemblyAttribute<T>(Func<T, string> value) where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(T));
            return value.Invoke(attribute);
        }
    }
}
