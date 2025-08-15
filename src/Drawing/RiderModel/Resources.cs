using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace linerider.Drawing.RiderModel
{
    internal class Resources
    {
        protected Filenames Filenames = new();

        public List<string> RegionsCacheLines;
        public SKBitmap Body { get; set; }
        public SKBitmap BodyDead { get; set; }
        public SKBitmap Sled { get; set; }
        public SKBitmap SledBroken { get; set; }
        public SKBitmap Arm { get; set; }
        public SKBitmap Leg { get; set; }
        public SKBitmap Rope { get; set; }
        public SKBitmap Palette { get; set; }
        public SKBitmap Regions { get; set; }
        public bool Legacy { get; protected set; }
        protected RegionsCache Cache { get; }

        public List<Rectangle> RegionsBody => Cache.RegionsBody;
        public List<Rectangle> RegionsBodyDead => Cache.RegionsBodyDead;
        public bool HasPalette => Palette != null;
        public bool HasRegions => RegionsBody.Any() && RegionsBodyDead.Any();
        public bool HasRope => Rope != null;

        public Resources()
        {
            Cache = new RegionsCache();
            Cache.SetFilenames(Filenames);
        }

        public virtual void Load() => throw new NotImplementedException();
    }
}