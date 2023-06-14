using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace linerider.Drawing.RiderModel
{
    internal class ResourcesCustom : Resources
    {
        private string SkinPath { get; }

        public ResourcesCustom(string skinName)
        {
            SkinPath = Path.Combine(Program.UserDirectory, "Riders", skinName);
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

            Body = new Bitmap(Path.Combine(SkinPath, Filenames.Body));
            BodyDead = new Bitmap(Path.Combine(SkinPath, Filenames.BodyDead));

            Sled = new Bitmap(Path.Combine(SkinPath, Filenames.Sled));
            SledBroken = new Bitmap(Path.Combine(SkinPath, Filenames.SledBroken));

            Arm = new Bitmap(Path.Combine(SkinPath, Filenames.Arm));
            Leg = new Bitmap(Path.Combine(SkinPath, Filenames.Leg));

            if (File.Exists(ropePath))
                Rope = new Bitmap(ropePath);

            if (File.Exists(palettePngPath))
                Palette = new Bitmap(palettePngPath);

            if (File.Exists(regionsPngPath))
                Regions = new Bitmap(regionsPngPath);

            if (File.Exists(regionsPath) && File.ReadLines(regionsPath).First() == RegionsCache.HeaderMain)
                RegionsCacheLines = File.ReadAllLines(regionsPath).ToList();

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
