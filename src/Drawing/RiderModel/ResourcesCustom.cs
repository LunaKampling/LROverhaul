using linerider.Utils;
using SkiaSharp;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace linerider.Drawing.RiderModel
{
    internal class ResourcesCustom : Resources
    {
        private string SkinPath { get; }

        public ResourcesCustom(string skinName)
        {
            SkinPath = Path.Combine(Settings.Local.UserDirPath, Constants.RidersFolderName, skinName);
            Cache.SetHome(SkinPath);
        }

        public override void Load()
        {
            Legacy = File.Exists(Path.Combine(SkinPath, "brokensled.png"));

            if (Legacy)
            {
                Filenames.SledBroken = "brokensled.png";
            }

            string ropePath = Path.Combine(SkinPath, Filenames.Rope);
            string regionsPath = Path.Combine(SkinPath, Filenames.RegionsCache);
            string regionsPngPath = Path.Combine(SkinPath, Filenames.Regions);
            string palettePngPath = Path.Combine(SkinPath, Filenames.Palette);

            Body = SKBitmap.Decode(Path.Combine(SkinPath, Filenames.Body));
            BodyDead = SKBitmap.Decode(Path.Combine(SkinPath, Filenames.BodyDead));

            Sled = SKBitmap.Decode(Path.Combine(SkinPath, Filenames.Sled));
            SledBroken = SKBitmap.Decode(Path.Combine(SkinPath, Filenames.SledBroken));

            Arm = SKBitmap.Decode(Path.Combine(SkinPath, Filenames.Arm));
            Leg = SKBitmap.Decode(Path.Combine(SkinPath, Filenames.Leg));

            if (File.Exists(ropePath))
                Rope = SKBitmap.Decode(ropePath);

            if (File.Exists(palettePngPath))
                Palette = SKBitmap.Decode(palettePngPath);

            if (File.Exists(regionsPngPath))
                Regions = SKBitmap.Decode(regionsPngPath);

            if (File.Exists(regionsPath) && File.ReadLines(regionsPath).First() == RegionsCache.HeaderMain)
                RegionsCacheLines = [.. File.ReadAllLines(regionsPath)];

            if (RegionsCacheLines != null)
            {
                bool success;
                try
                {
                    success = Cache.Parse(RegionsCacheLines);
                }
                catch
                {
                    success = false;
                }

                if (!success)
                {
                    Debug.WriteLine("Regions cache is outdated, recreating...");
                    Cache.Delete();
                    Cache.Build();
                }
            }
            else if (Regions != null)
            {
                Debug.WriteLine("Building regions cache...");
                Cache.Build();
            }
        }
    }
}
