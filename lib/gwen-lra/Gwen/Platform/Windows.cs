using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

// TODO: compile/run only on windows

namespace Gwen.Platform
{
    /// <summary>
    /// Windows-specific utility functions.
    /// </summary>
    public static class Windows
    {
        private const string FontRegKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

        private static Dictionary<string, string> m_FontPaths;

        /// <summary>
        /// Gets a font file path from font name.
        /// </summary>
        /// <param name="fontName">Font name.</param>
        /// <returns>Font file path.</returns>
        public static string GetFontPath(string fontName)
        {
            // Is this reliable? we rely on lazy jitting to not run win32 code on linux
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return null;

            if (m_FontPaths == null)
                InitFontPaths();

            return !m_FontPaths.ContainsKey(fontName) ? null : m_FontPaths[fontName];
        }

        private static void InitFontPaths()
        {
            // Very hacky but better than nothing
            m_FontPaths = new Dictionary<string, string>();
            string fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

            RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            RegistryKey subkey = key.OpenSubKey(FontRegKey);
            foreach (string fontName in subkey.GetValueNames())
            {
                string fontFile = (string)subkey.GetValue(fontName);
                if (!fontName.EndsWith(" (TrueType)"))
                    continue;
                string font = fontName.Replace(" (TrueType)", "");
                m_FontPaths[font] = Path.Combine(fontsDir, fontFile);
            }
            key.Dispose();
        }
    }
}
