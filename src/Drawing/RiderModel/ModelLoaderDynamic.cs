using linerider.Utils;
using SkiaSharp;

namespace linerider.Drawing.RiderModel
{
    internal class ModelLoaderDynamic : ModelLoader
    {
        public static readonly double TextureScale = 0.015;

        private class ContactPointPos
        {
            public static readonly Point Sled = new Point(-552, -167);
            public static readonly Point Body = new Point(-424, 0);
            public static readonly Point Arm = new Point(-216, 0);
            public static readonly Point Leg = new Point(-224, 0);
        }

        protected override void ApplyScarfColorsToBody()
        {
            base.ApplyScarfColorsToBody();
            RotateBody();
        }

        protected override void ApplyRects() => Models.SetRects(
                CalcRect(Model.Sled, ContactPointPos.Sled),
                CalcRect(Model.SledBroken, ContactPointPos.Sled),
                CalcRect(Model.Body, ContactPointPos.Body),
                CalcRect(Model.Arm, ContactPointPos.Arm),
                CalcRect(Model.Leg, ContactPointPos.Leg)
            );

        protected override void ApplyRope()
        {
            if (Model.HasRope)
                Models.SetRope(Model.Rope.Width * TextureScale, Model.Rope.GetPixel(0, 0));
            else
                Models.SetRope(14 * TextureScale, Color.Black);
        }

        private void RotateBody()
        {
            Model.Body = Rotate90(Model.Body);
            Model.BodyDead = Rotate90(Model.BodyDead);
        }

        private SKBitmap Rotate90(SKBitmap img)
        {
            SKBitmap clone = new SKBitmap(img.Height, img.Width, img.ColorType, img.AlphaType);
            using (var surface = new SKCanvas(clone)) {
                surface.Translate(clone.Width, 0);
                surface.RotateDegrees(90);
                surface.DrawBitmap(img, 0, 0);
            }
            return clone;
        }

        private DoubleRect CalcRect(SKBitmap img, Point shift) => new DoubleRect(
                (img.Width / 2 + shift.X) * -1 * TextureScale,
                (img.Height / 2 + shift.Y) * -1 * TextureScale,
                img.Width * TextureScale,
                img.Height * TextureScale
            );
    }
}
