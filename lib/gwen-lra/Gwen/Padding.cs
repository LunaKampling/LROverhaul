using System;
using System.Drawing;

namespace Gwen
{
    /// <summary>
    /// Represents inner spacing.
    /// </summary>
    public readonly struct Padding : IEquatable<Padding>
    {
        public readonly int Top;
        public readonly int Bottom;
        public readonly int Left;
        public readonly int Right;
        public int Width => Left + Right;
        public int Height => Top + Bottom;
        public Size Size => new Size(Width, Height);

        // Common values
        public static readonly Padding Zero = new Padding(0, 0, 0, 0);
        public static readonly Padding One = new Padding(1, 1, 1, 1);
        public static readonly Padding Two = new Padding(2, 2, 2, 2);
        public static readonly Padding Three = new Padding(3, 3, 3, 3);
        public static readonly Padding Four = new Padding(4, 4, 4, 4);
        public static readonly Padding Five = new Padding(5, 5, 5, 5);

        public Padding(int left, int top, int right, int bottom)
        {
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
        }

        public bool Equals(Padding other) => other.Top == Top && other.Bottom == Bottom && other.Left == Left && other.Right == Right;
        public static Padding operator +(Padding lhs, Padding rhs)
        {
            return new Padding(
                lhs.Left + rhs.Left,
                lhs.Top + rhs.Top,
                lhs.Right + rhs.Right,
                lhs.Bottom + rhs.Bottom);
        }
        public static Padding operator -(Padding lhs, Padding rhs)
        {
            return new Padding(
                lhs.Left - rhs.Left,
                lhs.Top - rhs.Top,
                lhs.Right - rhs.Right,
                lhs.Bottom - rhs.Bottom);
        }
        public static bool operator ==(Padding lhs, Padding rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Padding lhs, Padding rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override bool Equals(object obj) => !(obj is null) && obj.GetType() == typeof(Padding) && Equals((Padding)obj);

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
