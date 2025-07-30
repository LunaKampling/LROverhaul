using OpenTK.Mathematics;
namespace linerider.Game
{

    public struct CellLocation(GridPoint point, Vector2d remainder)
    {
        public Vector2d Remainder = remainder;
        public GridPoint Point = point;

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