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

using OpenTK.Mathematics;
using System;
namespace linerider.Utils
{
    public struct FloatRect : IEquatable<FloatRect>
    {
        public float Left;
        public float Top;
        public float Width;
        public float Height;
        public Vector2 Vector => new(Left, Top);
        public Vector2 Size => new(Width, Height);
        public float Right => Left + Width;
        public float Bottom => Top + Height;
        public FloatRect(float left, float top, float width, float height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
        public FloatRect(Vector2 position, Vector2 size)
        {
            this = new FloatRect(position.X, position.Y, size.X, size.Y);
        }

        public static FloatRect FromLRTB(float left, float right, float top, float bottom) => new(left, top, right - left, bottom - top);
        public Vector2 EllipseClamp(Vector2 position)
        {
            Vector2 center = Vector + Size / 2;
            float xrad = Width / 2;
            float yrad = Height / 2;

            Vector2 diff = position - center;
            if ((diff.X * diff.X / (xrad * xrad) + diff.Y * diff.Y / (yrad * yrad)) > 1.0)
            {
                double m = Math.Atan2(diff.Y * xrad / yrad, diff.X);
                return new Vector2(
                    (float)(center.X + xrad * Math.Cos(m)),
                    (float)(center.Y + yrad * Math.Sin(m)));
            }
            return position;
        }

        public Vector2 Clamp(Vector2 v)
        {
            if (!Contains(v.X, v.Y))
            {
                float l = Left;
                float t = Top;
                float r = l + Width;
                float b = t + Height;
                v.X = MathHelper.Clamp(v.X, l, r);
                v.Y = MathHelper.Clamp(v.Y, t, b);
            }
            return v;
        }

        public FloatRect Inflate(float width, float height)
        {
            FloatRect rect = this;

            rect.Left -= width;
            rect.Top -= height;
            rect.Width += 2 * width;
            rect.Height += 2 * height;
            return rect;
        }

        public bool Contains(float x, float y)
        {
            float num = Math.Min(Left, Left + Width);
            float num2 = Math.Max(Left, Left + Width);
            float num3 = Math.Min(Top, Top + Height);
            float num4 = Math.Max(Top, Top + Height);
            return x >= num && x < num2 && y >= num3 && y < num4;
        }
        public bool Intersects(FloatRect rect) => Intersects(rect, out _);
        public bool Intersects(FloatRect rect, out FloatRect overlap)
        {
            float val = Math.Min(Left, Left + Width);
            float val2 = Math.Max(Left, Left + Width);
            float val3 = Math.Min(Top, Top + Height);
            float val4 = Math.Max(Top, Top + Height);
            float val5 = Math.Min(rect.Left, rect.Left + rect.Width);
            float val6 = Math.Max(rect.Left, rect.Left + rect.Width);
            float val7 = Math.Min(rect.Top, rect.Top + rect.Height);
            float val8 = Math.Max(rect.Top, rect.Top + rect.Height);
            float num = Math.Max(val, val5);
            float num2 = Math.Max(val3, val7);
            float num3 = Math.Min(val2, val6);
            float num4 = Math.Min(val4, val8);
            if (num < num3 && num2 < num4)
            {
                overlap.Left = num;
                overlap.Top = num2;
                overlap.Width = num3 - num;
                overlap.Height = num4 - num2;
                return true;
            }
            overlap.Left = 0f;
            overlap.Top = 0f;
            overlap.Width = 0f;
            overlap.Height = 0f;
            return false;
        }
        public override string ToString() => string.Concat(
                [
                    "[FloatRect] Left(",
                    Left,
                    ") Top(",
                    Top,
                    ") Width(",
                    Width,
                    ") Height(",
                    Height,
                    ")"
                ]);
        public override bool Equals(object obj) => obj is FloatRect && obj.Equals(this);
        public bool Equals(FloatRect other) => Left == other.Left && Top == other.Top && Width == other.Width && Height == other.Height;
        public override int GetHashCode() => (int)((uint)Left ^ ((uint)Top << 13 | (uint)Top >> 19) ^ ((uint)Width << 26 | (uint)Width >> 6) ^ ((uint)Height << 7 | (uint)Height >> 25));
        public static bool operator ==(FloatRect r1, FloatRect r2)
        {
            return r1.Equals(r2);
        }
        public static bool operator !=(FloatRect r1, FloatRect r2)
        {
            return !r1.Equals(r2);
        }
        public static FloatRect operator /(FloatRect r1, float r2)
        {
            r1.Top /= r2;
            r1.Left /= r2;
            r1.Width /= r2;
            r1.Height /= r2;
            return r1;
        }
        public static FloatRect operator *(FloatRect r1, float r2)
        {
            r1.Top *= r2;
            r1.Left *= r2;
            r1.Width *= r2;
            r1.Height *= r2;
            return r1;
        }
    }
}