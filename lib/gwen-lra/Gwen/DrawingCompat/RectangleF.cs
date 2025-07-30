using SkiaSharp;

public struct RectangleF(float left, float top, float width, float height)
{
    float left = left;
    float top = top;
    float width = width;
    float height = height;
    public static readonly Rectangle Empty = new(0, 0, 0, 0);

    public float Bottom
    {
        get
        {
            return top + height;
        }
    }
    public float Top
    {
        get
        {
            return top;
        }
    }
    public float Right
    {
        get
        {
            return left + width;
        }
    }
    public float Left
    {
        get
        {
            return left;
        }
    }
    public float X
    {
        get => left;
        set => left = value;
    }
    public float Y
    {
        get => top;
        set => top = value;
    }
    public float Width
    {
        get => width;
        set => width = value;
    }
    public float Height
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

    /*public Point Location {
        get => new Point(left, top);
        set {
            left = value.X;
            top = value.Y;
        }
    }

    public Size Size {
        get => new Size(width, height);
        set {
            width = value.Width;
            height = value.Height;
        }
    }*/

    public static RectangleF FromLTRB(int l, int t, int r, int b)
    {
        return new RectangleF(l, t, r - l, b - t);
    }

    public bool Equals(RectangleF r) => r.X == X && r.Y == Y && r.Width == Width && r.Height == Height;
    public override bool Equals(object o)
    {
        if (o is RectangleF)
            return Equals((RectangleF)o);
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(RectangleF left, RectangleF right) => left.Equals(right);
    public static bool operator !=(RectangleF left, RectangleF right) => !left.Equals(right);

    public static implicit operator SKRect(RectangleF r) => new(r.Left, r.Top, r.Right, r.Bottom);
}