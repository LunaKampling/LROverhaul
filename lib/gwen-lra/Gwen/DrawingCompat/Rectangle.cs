// Replacement for System.Drawing.Rectangle that can cast to SkiaSharp.SKRectI
using SkiaSharp;

public struct Rectangle(int left, int top, int width, int height)
{
    int left = left;
    int top = top;
    int width = width;
    int height = height;
    public static readonly Rectangle Empty = new(0, 0, 0, 0);

    public int Bottom
    {
        get
        {
            return top + height;
        }
    }
    public int Top
    {
        get
        {
            return top;
        }
    }
    public int Right
    {
        get
        {
            return left + width;
        }
    }
    public int Left
    {
        get
        {
            return left;
        }
    }
    public int X
    {
        get => left;
        set => left = value;
    }
    public int Y
    {
        get => top;
        set => top = value;
    }
    public int Width
    {
        get => width;
        set => width = value;
    }
    public int Height
    {
        get => height;
        set => height = value;
    }

    public bool IsEmpty
    {
        get
        {
            return top == 0 && left == 0 && width == 0 && height == 0;
        }
    }

    public Point Location
    {
        get => new(left, top);
        set
        {
            left = value.X;
            top = value.Y;
        }
    }

    public Size Size
    {
        get => new(width, height);
        set
        {
            width = value.Width;
            height = value.Height;
        }
    }

    public static Rectangle FromLTRB(int l, int t, int r, int b)
    {
        return new Rectangle(l, t, r - l, b - t);
    }

    public bool Equals(Rectangle r) => r.X == X && r.Y == Y && r.Width == Width && r.Height == Height;
    public override bool Equals(object o)
    {
        if (o is Rectangle)
            return Equals((Rectangle)o);
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(Rectangle left, Rectangle right) => left.Equals(right);
    public static bool operator !=(Rectangle left, Rectangle right) => !left.Equals(right);

    public static implicit operator SKRectI(Rectangle r) => new(r.Left, r.Top, r.Right, r.Bottom);
    public static implicit operator SKRect(Rectangle r) => new(r.Left, r.Top, r.Right, r.Bottom);
}