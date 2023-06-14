//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using linerider.Rendering;
using linerider.Utils;
using System.Drawing;

namespace linerider.Drawing.RiderModel
{
    public static class Models
    {
        public static DoubleRect SledRect;
        public static DoubleRect SledBrokenRect;
        public static DoubleRect BodyRect;
        public static DoubleRect ArmRect;
        public static DoubleRect LegRect;

        public static int SledTexture;
        public static int SledBrokenTexture;
        public static int BodyTexture;
        public static int BodyDeadTexture;
        public static int ArmTexture;
        public static int LegTexture;

        public static readonly FloatRect BodyUV = new FloatRect(0, 0, 1, 1);
        public static readonly FloatRect BodyDeadUV = new FloatRect(0, 0, 1, 1);

        public static readonly FloatRect SledUV = new FloatRect(0, 0, 1, 1);
        public static readonly FloatRect SledBrokenUV = new FloatRect(0, 0, 1, 1);

        public static readonly FloatRect ArmUV = new FloatRect(0, 0, 1, 1);
        public static readonly FloatRect LegUV = new FloatRect(0, 0, 1, 1);

        public static float RopeThickness = 0;
        public static Color RopeColor = Color.Black;

        public static void SetSprites(Bitmap body_img, Bitmap bodydead_img, Bitmap sled_img, Bitmap sledbroken_img, Bitmap arm_img, Bitmap leg_img)
        {
            BodyTexture = StaticRenderer.LoadTexture(body_img);
            BodyDeadTexture = StaticRenderer.LoadTexture(bodydead_img);

            SledTexture = StaticRenderer.LoadTexture(sled_img);
            SledBrokenTexture = StaticRenderer.LoadTexture(sledbroken_img);

            ArmTexture = StaticRenderer.LoadTexture(arm_img);
            LegTexture = StaticRenderer.LoadTexture(leg_img);
        }

        public static void SetRope(double thickness, Color color)
        {
            RopeThickness = (float)thickness;
            RopeColor = color;
        }

        public static void SetRects(DoubleRect sled, DoubleRect sledbroken, DoubleRect body, DoubleRect arm, DoubleRect leg)
        {
            SledRect = sled;
            SledBrokenRect = sledbroken;
            BodyRect = body;
            ArmRect = arm;
            LegRect = leg;
        }
    }
}