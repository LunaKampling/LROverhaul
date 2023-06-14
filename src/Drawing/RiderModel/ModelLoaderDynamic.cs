using linerider.Utils;
using System.Drawing;

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

        private Bitmap Rotate90(Bitmap img)
        {
            Bitmap clone = new Bitmap(img);
            clone.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return clone;
        }

        private DoubleRect CalcRect(Bitmap img, Point shift) => new DoubleRect(
                (img.Width / 2 + shift.X) * -1 * TextureScale,
                (img.Height / 2 + shift.Y) * -1 * TextureScale,
                img.Width * TextureScale,
                img.Height * TextureScale
            );
    }
}
