// Replacement for System.Drawing.Point that can cast to SkiaSharp.SKPointI
using SkiaSharp;

public struct Point {
    int x;
    int y;

    public Point(int x, int y) {
        this.x = x;
        this.y = y;
    }
    public Point(int value) {
        this.x = value;
        this.y = value;
    }

    public static readonly Point Empty = new Point(0);
    
    public int X {
        get => x;
        set => x = value;
    } 

    public int Y {
        get => y;
        set => y = value;
    }

    public bool IsEmpty {
        get => x == 0 && y == 0;
    }

    public bool Equals(Point p) => p.X == X && p.Y == Y;
    public override bool Equals(object o) {
        if (o is Point) 
            return Equals((Point)o);
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator== (Point left, Point right) => left.Equals(right);
    public static bool operator!= (Point left, Point right) => !left.Equals(right);

    public static implicit operator SKPointI(Point p) => new SKPointI(p.X, p.Y);
}