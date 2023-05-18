using Newtonsoft.Json.Linq;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;

namespace linerider.Drawing.RiderModel
{
    public static class ScarfColors
    {
        private static List<int> Colors = new List<int>();
        private static List<byte> Opacity = new List<byte>();

        public static void Add(int color, byte opacity)
        {
            Colors.Add(color);
            Opacity.Add(opacity);
        }
        public static void Insert(int color, byte opacity, int index)
        {
            Colors.Insert(index, color);
            Opacity.Insert(index, opacity);
        }
        public static void Remove(int index)
        {
            Colors.RemoveAt(index);
            Opacity.RemoveAt(index);
        }
        public static bool AreEmpty()
        {
            return !Colors.Any();
        }
        public static int Count()
        {
            return Colors.Count();
        }
        public static List<int> GetColorList()
        {
            return Colors;
        }
        public static List<byte> GetOpacityList()
        {
            return Opacity;
        }
        public static void RemoveAll()
        {
            Colors.Clear();
            Opacity.Clear();
        }
        public static void Shift(int shift) // Shifts scarf colors to the left
        {
            for (int i = 0; i < shift; i++)
            {
                Insert(GetColorList()[Count() - 1], GetOpacityList()[Count() - 1], 0);
                Remove(Count() - 1);
            }
        }
        public static void Reload()
        {
            bool isDefaultScarf = Settings.SelectedScarf.Equals("*default*");

            try
            {
                if (isDefaultScarf)
                    SetDefault();
                else
                    SetFromFile();
            }
            catch
            {
                SetDefault();
            }
        }
        private static void SetFromFile()
        {
            string scarfLocation = Path.Combine(Program.UserDirectory, "Scarves", Settings.SelectedScarf);
            ScarfLoader loader = new ScarfLoader(scarfLocation);

            RemoveAll();

            List<ScarfSegment> segments = loader.Load();
            foreach (ScarfSegment segment in segments)
                Add(segment.Color, segment.Opacity);
        }
        /*private static void SetFromFileOld(string filepath)
        {
            string fileHeader = File.ReadLines(filepath).First();
            bool isLegacyFile = fileHeader == "#LRTran Scarf File";
            bool isValidFile = fileHeader == "#Line Rider Scarf File";
            if (!isValidFile && !isLegacyFile)
                throw new Exception("Is not valid scarf file");

            RemoveAll();

            IEnumerable<string> lines = File.ReadAllLines(filepath).Skip(1);
            foreach (string line in lines)
            {
                int color;
                byte opacity;

                if (isLegacyFile)
                {
                    color = Convert.ToInt32(line.Substring(0, line.IndexOf(",")), 16);
                    opacity = Convert.ToByte(line.Substring(line.IndexOf(" ") + 1), 16);
                }
                else
                {
                    string colorRaw;
                    string opacityRaw;

                    if (line.Contains(',')) // Color + opacity
                    {
                        string[] parts = line.Split(',');
                        colorRaw = parts[0].Trim();
                        opacityRaw = parts[1].Trim();
                    }
                    else // Color only
                    {
                        colorRaw = line.Trim();
                        opacityRaw = "255";
                    }

                    if (!colorRaw.StartsWith("0x"))
                        colorRaw = $"0x{colorRaw}";

                    if (opacityRaw.EndsWith("%")) // Convert "100%" to "256"
                    {
                        double precentage = double.Parse(opacityRaw.TrimEnd('%'));
                        opacityRaw = Math.Round(precentage / 100 * 255 - 1).ToString();
                    }

                    color = Convert.ToInt32(colorRaw, 16);
                    opacity = Convert.ToByte(opacityRaw);
                }

                Add(color, opacity);
            }
        }*/
        private static void SetDefault()
        {
            RemoveAll();
            // Add(0xD10101, 0xFF);
            // Add(0xFF6464, 0xFF);
            Add(0xB93332, 0xFF);
            Add(0xEF7A5D, 0xFF);
        }
    }
}
