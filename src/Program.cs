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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;

namespace linerider
{
    public static class Program
    {
#if DEBUG
        public static bool IsDebugged = false;
        public static bool LogGL => false;
#endif
        public static string BinariesFolder = "bin";
        public static readonly CultureInfo Culture = new CultureInfo("en-US");
        public static string FullVersion = AssemblyInfo.Version + (AssemblyInfo.TestVersion.Length > 1 ? " " : "") + AssemblyInfo.TestVersion;
        public static string NewVersion = null;
        public static readonly string WindowTitle = AssemblyInfo.Title + " \u22C5 " + FullVersion;
        public static Random Random;
        private static bool _crashed;
        private static MainWindow glGame;
        private static string _currdir;
        private static string _userdir;
        public static string[] args;

        /// <summary>
        /// Gets the current directory. Ends in Path.DirectorySeperator
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
                    _userdir += Path.DirectorySeparatorChar + "LRA" + Path.DirectorySeparatorChar;
                }
                return _userdir;
            }
        }
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
                glGame.Track.BackupTrack();
            }
            if (System.Windows.Forms.MessageBox.Show(
                "Unhandled Exception: " +
                e.Message +
                Environment.NewLine +
                e.StackTrace +
                Environment.NewLine +
                "Would you like to export the crash data to a log.txt?",
                "Error!",
                System.Windows.Forms.MessageBoxButtons.YesNo)
                 == System.Windows.Forms.DialogResult.Yes)
            {
                if (!File.Exists(UserDirectory + "log.txt"))
                    File.Create(UserDirectory + "log.txt").Dispose();

                string append = WindowTitle + "\r\n" + e.ToString() + "\r\n";
                string begin = File.ReadAllText(UserDirectory + "log.txt", System.Text.Encoding.ASCII);
                File.WriteAllText(UserDirectory + "log.txt", begin + append, System.Text.Encoding.ASCII);
            }
            if (!nothrow)
                throw e;
        }

        public static void NonFatalError(string err) => System.Windows.Forms.MessageBox.Show("Non Fatal Error: " + err);
        public static void Run(string[] givenArgs)
        {
#if DEBUG
            if (IsDebugged)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }
#endif
            args = givenArgs;
            if (!Directory.Exists(UserDirectory))
            {
                Directory.CreateDirectory(UserDirectory);
                System.Windows.Forms.MessageBox.Show("LRA User directory created at:\r\n" + UserDirectory);
            }
            Settings.Load();
            // Create critical settings if needed
            if (Settings.DefaultSaveFormat == null)
            {
                Settings.DefaultSaveFormat = ".trk";
            }
            if (Settings.DefaultQuicksaveFormat == null)
            {
                Settings.DefaultQuicksaveFormat = ".trk";
            }
            if (Settings.DefaultAutosaveFormat == null)
            {
                Settings.DefaultAutosaveFormat = ".trk";
            }
            if (Settings.DefaultCrashBackupFormat == null)
            {
                Settings.DefaultCrashBackupFormat = ".trk";
            }
            if (Settings.AutosavePrefix == null)
            {
                Settings.DefaultCrashBackupFormat = "Autosave";
            }
            Settings.Save();

            if (!Directory.Exists(UserDirectory + "Songs"))
                Directory.CreateDirectory(UserDirectory + "Songs");
            if (!Directory.Exists(UserDirectory + "Tracks"))
                Directory.CreateDirectory(UserDirectory + "Tracks");
            if (!Directory.Exists(UserDirectory + "Riders"))
                Directory.CreateDirectory(UserDirectory + "Riders");
            if (!Directory.Exists(UserDirectory + "Scarves"))
                Directory.CreateDirectory(UserDirectory + "Scarves");
            if (!Directory.Exists(UserDirectory + "Renders"))
                Directory.CreateDirectory(UserDirectory + "Renders");

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
                    glGame.Run(60, 0); // TODO: Maybe not limit this
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
            if (AssemblyInfo.TestVersion.ToLower().Contains("closed"))
                return;

            if (Settings.CheckForUpdates)
            {
                new System.Threading.Thread(() =>
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            string recentversion = wc.DownloadString($"{Constants.GithubRawHeader}/main/version");
                            int idx = recentversion.IndexOfAny(new char[] { '\r', '\n' });
                            if (idx != -1)
                                recentversion = recentversion.Remove(idx);
                            if (recentversion != AssemblyInfo.Version && recentversion.Length > 0)
                                NewVersion = recentversion;
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
        public static string TestVersion => GetExecutingAssemblyAttribute<AssemblyMetadataAttribute>(a => a.Value);

        private static string GetExecutingAssemblyAttribute<T>(Func<T, string> value) where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(T));
            return value.Invoke(attribute);
        }
    }
}
