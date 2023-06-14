using OpenTK;
namespace linerider.Game
{

    public struct CellLocation
    {
        public Vector2d Remainder;
        public GridPoint Point;
        public CellLocation(GridPoint point, Vector2d remainder)
        {
            Point = point;
            Remainder = remainder;
        }
        public int X
        {
            get => Point.X;
            set => Point.X = value;
        }

        public int Y
        {
            get => Point.Y;
            set => Point.Y = value;
        }
    }
}