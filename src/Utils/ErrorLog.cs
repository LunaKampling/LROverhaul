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

using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.IO;

namespace linerider.Utils
{
    public static class ErrorLog
    {
        public static void WriteLine(string log)
        {
            try
            {
                string dir = Settings.Local.UserDirPath;
                if (dir == null || !Directory.Exists(Settings.Local.UserDirPath))
                    dir = Program.CurrentDirectory;

                StreamWriter fs = File.AppendText(dir + "errors.txt");
                fs.WriteLine(log);
                fs.Dispose();
            }
            catch
            {
            }
        }
        public static void PrintGLLog()
        {
            const bool logother = false;
            int logged = GL.GetInteger((GetPName)OpenTK.Graphics.OpenGL4.All.DebugLoggedMessages);
            if (logged > 0)
            {
                int max = GL.GetInteger((GetPName)OpenTK.Graphics.OpenGL4.All.MaxDebugMessageLength);
                DebugSource[] sources = new DebugSource[1];
                DebugType[] types = new DebugType[1];
                int[] ids = new int[1];
                DebugSeverity[] severities = new DebugSeverity[1];
                int[] lengths = new int[1];
                for (int i = 0; i < logged; i++)
                {
                    string messageLog;
                    int count = GL.GetDebugMessageLog(
                        1,
                        max,
                        sources,
                        types,
                        ids,
                        severities,
                        lengths,
                        out messageLog);
                    Debug.Assert(count != 0, "Unable to get full debug message log");
                    if (!logother && types[0] == DebugType.DebugTypeOther)
                    {
                        continue;
                    }
                    Debug.Indent();
                    Debug.WriteLine(
                        "[gl {0}, severity {1} source {2}]",
                        types[0],
                         severities[0], sources[0]);
                    Debug.Unindent();
                    Debug.WriteLine(messageLog.ToString());
                }
            }
        }
    }
}