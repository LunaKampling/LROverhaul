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
using System;
using System.Diagnostics;
using System.IO;

namespace linerider.IO.ffmpeg
{
    public static class FFMPEG
    {
        private const int MaximumBuffers = 25;
        private static bool inited = false;
        public static bool HasExecutable => File.Exists(ffmpeg_path);
        public static string ffmpeg_dir
        {
            get
            {
                string baseDir = Path.Combine(Settings.Local.UserDirPath, Constants.FFmpegBaseFolderName);

                string dir;
                if (OperatingSystem.IsMacOS())
                    dir = Path.Combine(baseDir, "mac");
                else if (OperatingSystem.IsWindows())
                    dir = Path.Combine(baseDir, "win");
                else if (OperatingSystem.IsLinux())
                    dir = Path.Combine(baseDir, "linux");
                else
                    return null;

                return dir;
            }
        }
        public static string ffmpeg_path
        {
            get
            {
                string baseDir = ffmpeg_dir;
                string executableFileName = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
                return baseDir != null ? Path.Combine(baseDir, executableFileName) : null;
            }
        }
        static FFMPEG()
        {
        }
        private static void TryInitialize()
        {
            if (inited)
                return;
            inited = true;
            if (ffmpeg_path == null)
                throw new Exception("Unable to detect platform for ffmpeg");
            MakeffmpegExecutable();
        }
        public static string ConvertSongToOgg(string file, Func<string, bool> stdout)
        {
            TryInitialize();
            if (!file.EndsWith(".ogg", true, Program.Culture))
            {
                FFMPEGParameters par = new();
                par.AddOption("i", "\"" + file + "\"");
                par.OutputFilePath = file.Remove(file.IndexOf(".", StringComparison.Ordinal)) + ".ogg";
                if (File.Exists(par.OutputFilePath))
                {
                    if (File.Exists(file))
                    {
                        File.Delete(par.OutputFilePath);
                    }
                    else
                    {
                        return par.OutputFilePath;
                    }
                }
                Execute(par, stdout);

                file = par.OutputFilePath;
            }
            return file;
        }
        public static void Execute(FFMPEGParameters parameters, Func<string, bool> stdout)
        {
            TryInitialize();
            if (string.IsNullOrWhiteSpace(ffmpeg_path))
            {
                throw new Exception("Path to FFMPEG executable cannot be null");
            }

            if (parameters == null)
            {
                throw new Exception("FFMPEG parameters cannot be completely null");
            }
            using Process ffmpegProcess = new();
            ProcessStartInfo info = new(ffmpeg_path)
            {
                Arguments = parameters.ToString(),
                WorkingDirectory = Path.GetDirectoryName(ffmpeg_dir),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            ffmpegProcess.StartInfo = info;
            _ = ffmpegProcess.Start();
            if (stdout != null)
            {
                while (true)
                {
                    string str = "";
                    try
                    {
                        str = ffmpegProcess.StandardError.ReadLine();
                    }
                    catch
                    {
                        Console.WriteLine("stdout log failed");
                        break;
                        // Ignored 
                    }
                    if (ffmpegProcess.HasExited)
                        break;
                    str ??= "";
                    if (!stdout.Invoke(str))
                    {
                        ffmpegProcess.Kill();
                        return;
                    }
                }
            }
            else
            {
                /*if (debug)
                {
                    string processOutput = ffmpegProcess.StandardError.ReadToEnd();
                }*/

                ffmpegProcess.WaitForExit();
            }
        }
        private static void MakeffmpegExecutable()
        {
            if (OperatingSystem.IsLinux())
            {
                try
                {
                    using Process chmod = new();
                    ProcessStartInfo info = new("/bin/chmod")
                    {
                        Arguments = "+x ffmpeg",
                        WorkingDirectory = Path.GetDirectoryName(ffmpeg_dir),
                        UseShellExecute = false,
                    };
                    chmod.StartInfo = info;
                    _ = chmod.Start();
                    if (!chmod.WaitForExit(1000))
                    {
                        chmod.Close();
                    }
                }
                catch (Exception e)
                {
                    Utils.ErrorLog.WriteLine(
                        "chmod error on ffmpeg" + Environment.NewLine + e.ToString());
                }
            }
        }
    }
}