using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

namespace linerider.Drawing.RiderModel
{
    internal class RegionsCache
    {
        public const string HeaderMain = "#Line Rider Scarf Regions File";
        public const string HeaderHashes = "#File hashes";
        public const string HeaderBody = "#Body regions";
        public const string HeaderBodyDead = "#Crashed body regions";

        public Filenames Filenames;

        public List<Rectangle> RegionsBody = new List<Rectangle>();
        public List<Rectangle> RegionsBodyDead = new List<Rectangle>();

        private enum ParseMode
        {
            None,
            Hashes,
            BodyRegions,
            BodyDeadRegions,
        };

        public string SkinHomePath;

        public void SetHome(string path) => SkinHomePath = path;

        internal void SetFilenames(Filenames filenames) => Filenames = filenames;

        public void Delete() => File.Delete(Path.Combine(SkinHomePath, Filenames.RegionsCache));

        public bool Parse(List<string> cacheLines)
        {
            RectangleConverter converter = new RectangleConverter();
            ParseMode mode = ParseMode.None;

            foreach (string cacheLine in cacheLines)
            {
                if (string.IsNullOrEmpty(cacheLine))
                    continue;

                switch (cacheLine)
                {
                    case HeaderHashes:
                    {
                        mode = ParseMode.Hashes;
                        continue;
                    }
                    case HeaderBody:
                    {
                        mode = ParseMode.BodyRegions;
                        continue;
                    }
                    case HeaderBodyDead:
                    {
                        mode = ParseMode.BodyDeadRegions;
                        continue;
                    }
                }

                switch (mode)
                {
                    case ParseMode.Hashes:
                    {
                        if (string.IsNullOrEmpty(SkinHomePath))
                            continue;

                        string[] parts = cacheLine.Split(';');
                        string filename = parts[0];
                        string hashShouldBe = parts[1];

                        using (FileStream inputStream = File.OpenRead(Path.Combine(SkinHomePath, filename)))
                        {
                            byte[] hash = MD5.Create().ComputeHash(inputStream);
                            string hashStr = Convert.ToBase64String(hash);

                            // Hash is invalid
                            if (hashStr != hashShouldBe)
                                return false;
                        }
                    }
                    break;
                    case ParseMode.BodyRegions:
                    case ParseMode.BodyDeadRegions:
                    {
                        _ = new Rectangle();
                        Rectangle region = (Rectangle)converter.ConvertFrom(null, Program.Culture, cacheLine);

                        if (mode == ParseMode.BodyRegions)
                            RegionsBody.Add(region);
                        else
                            RegionsBodyDead.Add(region);
                    }
                    break;
                }
            }
            return true;
        }

        public void Build()
        {
            string[] filesToHash = { Filenames.Regions, Filenames.Body, Filenames.BodyDead };
            Bitmap regionsPNG = new Bitmap(Path.Combine(SkinHomePath, Filenames.Regions));
            Bitmap bodyPNG = new Bitmap(Path.Combine(SkinHomePath, Filenames.Body));
            Bitmap bodyDeadPNG = new Bitmap(Path.Combine(SkinHomePath, Filenames.BodyDead));
            List<string> regionsFileLines = new List<string>();

            RegionsBody.Clear();
            RegionsBodyDead.Clear();

            regionsFileLines.Add(HeaderMain);
            regionsFileLines.Add("#This file is auto-regenerated when png files are changed");
            regionsFileLines.Add(string.Empty);

            // Calc file hashes
            regionsFileLines.Add(HeaderHashes);
            foreach (string filename in filesToHash)
                using (FileStream inputStream = File.OpenRead(Path.Combine(SkinHomePath, filename)))
                {
                    byte[] hash = MD5.Create().ComputeHash(inputStream);
                    string hashStr = Convert.ToBase64String(hash);

                    regionsFileLines.Add($"{filename};{hashStr}");
                }
            regionsFileLines.Add(string.Empty);

            // Calc scarf regions
            List<string> aliveRegions = new List<string>();
            List<string> crashedRegions = new List<string>();
            RectangleConverter converter = new RectangleConverter();
            for (int i = 0; i < regionsPNG.Width; i++)
            {
                Rectangle aliveRegion = new Rectangle(bodyPNG.Width, bodyPNG.Height, 0, 0);
                Rectangle crashedRegion = new Rectangle(bodyPNG.Width, bodyPNG.Height, 0, 0);
                Color colorToFind = regionsPNG.GetPixel(i, 0);
                colorToFind = Color.FromArgb(255, colorToFind); // Add 255 alpha

                for (int x = 0; x < bodyPNG.Width; x++)
                {
                    for (int y = 0; y < bodyPNG.Height; y++)
                    {
                        Color bodyColor = bodyPNG.GetPixel(x, y);
                        Color bodyDeadColor = bodyDeadPNG.GetPixel(x, y);
                        if (bodyColor.Equals(colorToFind))
                        {
                            if (aliveRegion.X > x)
                                aliveRegion.X = x;
                            if (aliveRegion.Y > y)
                                aliveRegion.Y = y;
                            if (aliveRegion.Width < x)
                                aliveRegion.Width = x - aliveRegion.X + 1;
                            if (aliveRegion.Height < y)
                                aliveRegion.Height = y - aliveRegion.Y + 1;
                        }
                        if (bodyDeadColor.Equals(colorToFind))
                        {
                            if (crashedRegion.X > x)
                                crashedRegion.X = x;
                            if (crashedRegion.Y > y)
                                crashedRegion.Y = y;
                            if (crashedRegion.Width < x)
                                crashedRegion.Width = x - crashedRegion.X + 1;
                            if (crashedRegion.Height < y)
                                crashedRegion.Height = y - crashedRegion.Y + 1;
                        }
                    }
                }

                if (aliveRegion.X == bodyPNG.Width && aliveRegion.Y == bodyPNG.Height)
                    aliveRegion = new Rectangle(0, 0, 0, 0);
                if (crashedRegion.X == bodyPNG.Width && crashedRegion.Y == bodyPNG.Height)
                    crashedRegion = new Rectangle(0, 0, 0, 0);

                aliveRegions.Add(converter.ConvertToString(aliveRegion));
                crashedRegions.Add(converter.ConvertToString(crashedRegion));

                RegionsBody.Add(aliveRegion);
                RegionsBodyDead.Add(crashedRegion);
            }

            regionsFileLines.Add(HeaderBody);
            regionsFileLines.AddRange(aliveRegions);
            regionsFileLines.Add(string.Empty);
            regionsFileLines.Add(HeaderBodyDead);
            regionsFileLines.AddRange(crashedRegions);

            string cachePath = Path.Combine(SkinHomePath, Filenames.RegionsCache);

            using (TextWriter writer = new StreamWriter(cachePath))
                foreach (string line in regionsFileLines)
                    writer.WriteLine(line);

            File.SetAttributes(cachePath, FileAttributes.Hidden);
        }
    }
}
