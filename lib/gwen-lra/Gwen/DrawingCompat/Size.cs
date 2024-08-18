// Replacement for System.Drawing.Size that can cast to SkiaSharp.SKSizeI
using SkiaSharp;

public struct Size {
    int w;
    int h;
    public Size(int w, int h) {
        this.w = w;
        this.h = h;
    }
    public Size(Point p) {
        this.w = p.X;
        this.h = p.Y;
    }

    public static readonly Size Empty = new Size(0, 0);

    public int Height {
        get => h;
        set => h = value;
    }
    public int Width {
        get => w;
        set => w = value;
    }

    public static Size operator+ (Size left, Size right) => new Size(left.Width + right.Width, left.Height + right.Height);
    public static Size operator- (Size left, Size right) => new Size(left.Width - right.Width, left.Height - right.Height);

    public bool Equals(Size s) => s.Width == Width && s.Height == Height;
    public override bool Equals(object o) {
        if (o is Size) 
            return Equals((Size)o);
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator== (Size left, Size right) => left.Equals(right);
    public static bool operator!= (Size left, Size right) => !left.Equals(right);

    public static implicit operator SKSizeI(Size s) => new SKSizeI(s.Width, s.Height);
    public static explicit operator Size(OpenTK.Mathematics.Vector2i v) => new Size(v.X, v.Y);
}