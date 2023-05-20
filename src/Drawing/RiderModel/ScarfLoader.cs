using linerider.Rendering;
using linerider.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace linerider.Drawing.RiderModel
{
    public class ScarfSegment
    {
        public int Color { get; set; }
        public byte Opacity { get; set; }
    }

    public class ScarfLoader
    {
        private FileType Type;
        private string Filepath;

        private enum FileType
        {
            TextLegacy,
            Text,
            Image,
        }

        public ScarfLoader(string filepath)
        {
            string ext = Path.GetExtension(filepath).ToLower();
            bool isImage = ext == ".png";
            if (isImage)
            {
                Type = FileType.Image;
            }
            else // Treat any other file as text file
            {
                string fileHeader = File.ReadLines(filepath).First();
                bool isLegacyFile = fileHeader == "#LRTran Scarf File";
                bool isValidFile = fileHeader == "#Line Rider Scarf File";
                if (!isValidFile && !isLegacyFile)
                    throw new Exception("Is not valid scarf file");

                if (isLegacyFile)
                    Type = FileType.TextLegacy;
                else
                    Type = FileType.Text;
            }

            Filepath = filepath;
        }

        public List<ScarfSegment> Load()
        {
            List<ScarfSegment> segments = new List<ScarfSegment>();

            switch (Type)
            {
                case FileType.TextLegacy:
                    segments = LoadTextLegacy();
                    break;
                case FileType.Text:
                    segments = LoadText();
                    break;
                case FileType.Image:
                    segments = LoadImage();
                    break;
            }

            return segments;
        }

        private List<ScarfSegment> LoadTextLegacy()
        {
            List<ScarfSegment> segments = new List<ScarfSegment>();
            IEnumerable<string> lines = File.ReadAllLines(Filepath).Skip(1);

            foreach (string line in lines)
            {
                ScarfSegment segment = new ScarfSegment();
                segment.Color = Convert.ToInt32(line.Substring(0, line.IndexOf(",")), 16);
                segment.Opacity = Convert.ToByte(line.Substring(line.IndexOf(" ") + 1), 16);
                segments.Add(segment);
            }

            return segments;
        }

        private List<ScarfSegment> LoadText()
        {
            List<ScarfSegment> segments = new List<ScarfSegment>();
            IEnumerable<string> lines = File.ReadAllLines(Filepath).Skip(1);

            foreach (string line in lines)
            {
                ScarfSegment segment = new ScarfSegment();
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

                segment.Color = Convert.ToInt32(colorRaw, 16);
                segment.Opacity = Convert.ToByte(opacityRaw);

                segments.Add(segment);
            }

            return segments;
        }

        private List<ScarfSegment> LoadImage()
        {
            List<ScarfSegment> segments = new List<ScarfSegment>();
            Bitmap scarfImg = new Bitmap(Filepath);

            for (int i = scarfImg.Width - 1; i >= 0; i -= 1)
            {
                Color color = scarfImg.GetPixel(i, 0);

                ScarfSegment segment = new ScarfSegment()
                {
                    Color = (int)(color.ToArgb() & 0x00FFFFFF),
                    Opacity = color.A,
                };

                segments.Add(segment);
            }

            return segments;
        }
    }
}
