using System;

namespace Gwen
{
    /// <summary>
    /// Represents outer spacing.
    /// </summary>
    public struct Margin(int left, int top, int right, int bottom) : IEquatable<Margin>
    {
        public int Top = top;
        public int Bottom = bottom;
        public int Left = left;
        public int Right = right;
        public int Width => Left + Right;
        public int Height => Top + Bottom;
        public Size Size => new(Width, Height);

        // Common values
        public static Margin Zero = new(0, 0, 0, 0);
        public static Margin One = new(1, 1, 1, 1);
        public static Margin Two = new(2, 2, 2, 2);
        public static Margin Three = new(3, 3, 3, 3);
        public static Margin Four = new(4, 4, 4, 4);
        public static Margin Five = new(5, 5, 5, 5);
        public static Margin Six = new(6, 6, 6, 6);
        public static Margin Seven = new(7, 7, 7, 7);
        public static Margin Eight = new(8, 8, 8, 8);
        public static Margin Nine = new(9, 9, 9, 9);
        public static Margin Ten = new(10, 10, 10, 10);

        public bool Equals(Margin other) => other.Top == Top && other.Bottom == Bottom && other.Left == Left && other.Right == Right;

        public static Margin operator +(Margin lhs, Margin rhs)
        {
            return new Margin(
                lhs.Left + rhs.Left,
                lhs.Top + rhs.Top,
                lhs.Right + rhs.Right,
                lhs.Bottom + rhs.Bottom);
        }
        public static Margin operator -(Margin lhs, Margin rhs)
        {
            return new Margin(
                lhs.Left - rhs.Left,
                lhs.Top - rhs.Top,
                lhs.Right - rhs.Right,
                lhs.Bottom - rhs.Bottom);
        }
        public static bool operator ==(Margin lhs, Margin rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Margin lhs, Margin rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override bool Equals(object obj) => !(obj is null) && obj.GetType() == typeof(Margin) && Equals((Margin)obj);

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Top;
                result = (result * 397) ^ Bottom;
                result = (result * 397) ^ Left;
                result = (result * 397) ^ Right;
                return result;
            }
        }
    }
}
