using linerider.Utils;

namespace linerider.Drawing.RiderModel
{
    internal class ModelLoaderLegacy : ModelLoader
    {
        private class Rect
        {
            public static readonly DoubleRect Sled = new(-0.6875, -2.3125, 17.9195, 8.95975);
            public static readonly DoubleRect SledBroken = new(-0.3645, -2.3125, 17.477, 8.7385);
            public static readonly DoubleRect Body = new(0.026, -3.145, 13.944, 6.972);
            public static readonly DoubleRect Arm = new(-0.657, -1.2305, 7.82, 3.91);
            public static readonly DoubleRect Leg = new(-0.6535, -2.013, 8.02, 4.01);
        }

        protected override void ApplyRects() => Models.SetRects(
                Rect.Sled,
                Rect.SledBroken,
                Rect.Body,
                Rect.Arm,
                Rect.Leg
            );

        protected override void ApplyRope()
        {
            if (Model.HasRope)
                Models.SetRope(Model.Rope.Width * ModelLoaderDynamic.TextureScale, Model.Rope.GetPixel(0, 0));
            else
                Models.SetRope(0.1, Color.Black);
        }
    }
}