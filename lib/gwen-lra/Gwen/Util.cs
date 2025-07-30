using System;
using System.Text.RegularExpressions;

namespace Gwen
{
    /// <summary>
    /// Misc utility functions.
    /// </summary>
    public static class Util
    {
        public static int Round(float x) => (int)Math.Round(x, MidpointRounding.AwayFromZero);
        /*
        public static int Trunc(float x)
        {
            return (int)Math.Truncate(x);
        }
        */
        public static int Ceil(float x) => (int)Math.Ceiling(x);

        public static Rectangle FloatRect(float x, float y, float w, float h) => new((int)x, (int)y, (int)w, (int)h);

        public static int Clamp(int x, int min, int max) => x < min ? min : x > max ? max : x;

        public static float Clamp(float x, float min, float max) => x < min ? min : x > max ? max : x;

        public static Rectangle ClampRectToRect(Rectangle inside, Rectangle outside, bool clampSize = false)
        {
            if (inside.X < outside.X)
                inside.X = outside.X;

            if (inside.Y < outside.Y)
                inside.Y = outside.Y;

            if (inside.Right > outside.Right)
            {
                if (clampSize)
                    inside.Width = outside.Width;
                else
                    inside.X = outside.Right - inside.Width;
            }
            if (inside.Bottom > outside.Bottom)
            {
                if (clampSize)
                    inside.Height = outside.Height;
                else
                    inside.Y = outside.Bottom - inside.Height;
            }

            return inside;
        }
        public static Rectangle RectangleClamp(Rectangle inside, Rectangle outside)
        {
            int x = Math.Max(inside.X, outside.X);
            int y = Math.Max(inside.Y, outside.Y);
            int w = inside.Width - (inside.X - x);
            int h = inside.Height - (inside.Y - y);
            if (x + w > outside.Right)
            {
                w = Math.Min(outside.Right, x + w) - x;
            }
            if (y + h > outside.Bottom)
            {
                h = Math.Min(outside.Bottom, y + h) - y;
            }
            return new Rectangle(x, y, w, h);
        }
        // From http://stackoverflow.com/questions/359612/how-to-change-rgb-color-to-hsv
        public static HSV ToHSV(this Color color)
        {
            HSV hsv = new();
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hsv.h = color.GetHue();
            hsv.s = (max == 0) ? 0 : 1f - 1f * min / max;
            hsv.v = max / 255f;

            return hsv;
        }

        public static Color HSVToColor(float h, float s, float v)
        {
            int hi = Convert.ToInt32(Math.Floor(h / 60)) % 6;
            float f = h / 60 - (float)Math.Floor(h / 60);

            v *= 255;
            int va = Convert.ToInt32(v);
            int p = Convert.ToInt32(v * (1 - s));
            int q = Convert.ToInt32(v * (1 - f * s));
            int t = Convert.ToInt32(v * (1 - (1 - f) * s));

            if (hi == 0)
                return Color.FromArgb(255, va, t, p);
            return hi == 1
                ? Color.FromArgb(255, q, va, p)
                : hi == 2
                ? Color.FromArgb(255, p, va, t)
                : hi == 3 ? Color.FromArgb(255, p, q, va) : hi == 4 ? Color.FromArgb(255, t, p, va) : Color.FromArgb(255, va, p, q);
        }

        // Can't create extension operators
        public static Color Subtract(this Color color, Color other) => Color.FromArgb(color.A - other.A, color.R - other.R, color.G - other.G, color.B - other.B);

        public static Color Add(this Color color, Color other) => Color.FromArgb(color.A + other.A, color.R + other.R, color.G + other.G, color.B + other.B);

        public static Color Multiply(this Color color, float amount) => Color.FromArgb(color.A, (int)(color.R * amount), (int)(color.G * amount), (int)(color.B * amount));

        public static Rectangle Add(this Rectangle r, Rectangle other) => new(r.X + other.X, r.Y + other.Y, r.Width + other.Width, r.Height + other.Height);

        /// <summary>
        /// Splits a string but keeps the separators intact (at the end of split parts).
        /// </summary>
        /// <param name="text">String to split.</param>
        /// <param name="separators">Separator characters.</param>
        /// <returns>Split strings.</returns>
        public static string[] SplitAndKeep(string text, string separators) => Regex.Split(text, @"(?=[" + separators + "])");
    }
}
