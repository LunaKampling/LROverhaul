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
    public struct DoubleRect : IEquatable<DoubleRect>
    {
        public static readonly DoubleRect Empty = new(0, 0, 0, 0);
        public double Left;
        public double Top;
        public double Width;
        public double Height;
        public Vector2d Vector
        {
            get => new(Left, Top);
            set
            {
                Left = value.X;
                Top = value.Y;
            }
        }
        public Vector2d Size
        {
            get => new(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }
        public double Right => Left + Width;
        public double Bottom => Top + Height;
        public DoubleRect(double left, double top, double width, double height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
        public DoubleRect(Vector2d Position, Vector2d Size)
        {
            Left = Position.X;
            Top = Position.Y;
            Width = Size.X;
            Height = Size.Y;
        }
        /// <summary>
        /// Forces LRTB layout
        /// </summary>
        /// <returns></returns>
        public DoubleRect MakeLRTB()
        {
            Vector2d vec1 = Vector;
            Vector2d vec2 = vec1 + Size;
            Vector2d topleft = new(
                Math.Min(vec1.X, vec2.X), Math.Min(vec1.Y, vec2.Y));
            Vector2d bottomright = new(
                Math.Max(vec1.X, vec2.X), Math.Max(vec1.Y, vec2.Y));
            return new DoubleRect(topleft, bottomright - topleft);
        }
        public static DoubleRect FromLRTB(double left, double right, double top, double bottom) => new(left, top, right - left, bottom - top);

        public FloatRect ToFloatRect() => new((float)Left, (float)Top, (float)Width, (float)Height);

        public Vector2d EllipseClamp(Vector2d position)
        {
            Vector2d center = Vector + Size / 2;
            double xrad = Width / 2;
            double yrad = Height / 2;

            Vector2d diff = position - center;
            if ((diff.X * diff.X / (xrad * xrad) + diff.Y * diff.Y / (yrad * yrad)) > 1.0)
            {
                double m = Math.Atan2(diff.Y * xrad / yrad, diff.X);
                return new Vector2d(
                    center.X + xrad * Math.Cos(m),
                    center.Y + yrad * Math.Sin(m));
            }
            return position;
        }

        public Vector2d Clamp(Vector2d v)
        {
            double l = Left;
            double t = Top;
            double r = l + Width;
            double b = t + Height;
            return new Vector2d(
                MathHelper.Clamp(v.X, l, r),
                MathHelper.Clamp(v.Y, t, b)
            );
        }

        public DoubleRect Inflate(double width, double height)
        {
            DoubleRect rect = this;

            rect.Left -= width;
            rect.Top -= height;
            rect.Width += 2 * width;
            rect.Height += 2 * height;
            return rect;
        }
        public DoubleRect Scale(double scale)
        {
            DoubleRect rect = this;

            double width = (Right - Left) * scale;
            rect.Left -= width / 2 - Width / 2;
            rect.Width = width;

            double height = (Bottom - Top) * scale;
            rect.Top -= height / 2 - Height / 2;
            rect.Height = height;
            return rect;
        }
        public DoubleRect Scale(double x, double y)
        {
            DoubleRect rect = this;

            double width = (Right - Left) * x;
            rect.Left -= width / 2 - Width / 2;
            rect.Width = width;

            double height = (Bottom - Top) * y;
            rect.Top -= height / 2 - Height / 2;
            rect.Height = height;
            return rect;
        }

        public bool Contains(double x, double y)
        {
            double num = Math.Min(Left, Left + Width);
            double num2 = Math.Max(Left, Left + Width);
            double num3 = Math.Min(Top, Top + Height);
            double num4 = Math.Max(Top, Top + Height);
            return x >= num && x < num2 && y >= num3 && y < num4;
        }
        public bool Intersects(DoubleRect rect) => Intersects(rect, out _);
        public bool Intersects(DoubleRect rect, out DoubleRect overlap)
        {
            double val = Math.Min(Left, Left + Width);
            double val2 = Math.Max(Left, Left + Width);
            double val3 = Math.Min(Top, Top + Height);
            double val4 = Math.Max(Top, Top + Height);
            double val5 = Math.Min(rect.Left, rect.Left + rect.Width);
            double val6 = Math.Max(rect.Left, rect.Left + rect.Width);
            double val7 = Math.Min(rect.Top, rect.Top + rect.Height);
            double val8 = Math.Max(rect.Top, rect.Top + rect.Height);
            double num = Math.Max(val, val5);
            double num2 = Math.Max(val3, val7);
            double num3 = Math.Min(val2, val6);
            double num4 = Math.Min(val4, val8);
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
                    "[DoubleRect] Left(",
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
        public bool Equals(DoubleRect other) => Left == other.Left && Top == other.Top && Width == other.Width && Height == other.Height;
        public override int GetHashCode() => (int)((uint)Left ^ ((uint)Top << 13 | (uint)Top >> 19) ^ ((uint)Width << 26 | (uint)Width >> 6) ^ ((uint)Height << 7 | (uint)Height >> 25));
        public static bool operator ==(DoubleRect r1, DoubleRect r2)
        {
            return r1.Equals(r2);
        }
        public static bool operator !=(DoubleRect r1, DoubleRect r2)
        {
            return !r1.Equals(r2);
        }
        public static DoubleRect operator /(DoubleRect r1, double r2)
        {
            r1.Top /= r2;
            r1.Left /= r2;
            r1.Width /= r2;
            r1.Height /= r2;
            return r1;
        }
        public static DoubleRect operator *(DoubleRect r1, double r2)
        {
            r1.Top *= r2;
            r1.Left *= r2;
            r1.Width *= r2;
            r1.Height *= r2;
            return r1;
        }
    }
}