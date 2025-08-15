using OpenTK.Mathematics;
using System.Collections.Generic;

namespace linerider.Game.LineGenerator
{
    internal class LineGenerator : Generator
    {
        public Vector2d positionStart; //Start of the line
        public Vector2d positionEnd; //End of the line
        public int multiplier = 1;
        public float width = 1;
        public bool invert = false;
        public bool reverse = false;
        public LineType lineType;

        public LineGenerator(string _name, Vector2d _positionStart, Vector2d _positionEnd, LineType _lineType)
        {
            name = _name;
            lines = [];
            positionStart = _positionStart;
            positionEnd = _positionEnd;
            lineType = _lineType;
        }

        public override void Generate_Internal(TrackWriter trk) // Generates the line
        {
            lines.Add(CreateLine(trk, invert ? positionEnd : positionStart, invert ? positionStart : positionEnd, lineType, reverse, multiplier, width));
            return;
        }
        public override void Generate_Preview_Internal(TrackWriter trk)
        {
            Generate_Internal(trk);
            return;
        }
    }
}