using OpenTK;
using System;
using System.Collections.Generic;

namespace linerider.Game.LineGenerator
{
    public class CircleGenerator : Generator
    {
        public double radius; // Radius of the circle
        public Vector2d position; // Centre of the circle
        public int lineCount; // Number of lines used to generate the circle
        public int multiplier = 1;
        public float width = 1;
        public bool invert;
        public bool reverse;
        public LineType lineType;

        public CircleGenerator(string _name, double _radius, Vector2d _position, int _lineCount, bool _invert)
        {
            name = _name;
            lines = new List<GameLine>();
            radius = _radius;
            position = _position;
            lineCount = _lineCount;
            invert = _invert;
            lineType = LineType.Standard;
        }

        public override void Generate_Internal(TrackWriter trk)
        {
            List<Vector2d> points = new List<Vector2d>();
            for (double frac = 0.0; frac < 1.0; frac += 1.0 / lineCount)
            {
                double ang = frac * 2.0 * Math.PI;
                points.Add(position + radius * new Vector2d(Math.Cos(ang), Math.Sin(ang)));
            }
            if (invert != reverse) // XOR
            {
                for (int i = 1; i < points.Count; i++)
                {
                    addLine(trk, points[i], points[i - 1], lineType, reverse);
                }
                addLine(trk, points[0], points[points.Count - 1], lineType, reverse);
            }
            else
            {
                for (int i = 1; i < points.Count; i++)
                {
                    addLine(trk, points[i - 1], points[i], lineType, reverse);
                }
                addLine(trk, points[points.Count - 1], points[0], lineType, reverse);
            }
        }
        private void addLine(TrackWriter trk, Vector2d start, Vector2d end, LineType type, bool inv)
        {
            switch (type)
            {
                case LineType.Standard:
                    lines.Add(CreateLine(trk, start, end, type, inv));
                    break;

                case LineType.Acceleration:
                    lines.Add(CreateLine(trk, start, end, type, inv, multiplier));
                    break;

                case LineType.Scenery:
                    lines.Add(CreateLine(trk, start, end, type, inv, 1, width));
                    break;
            }
        }
        public override void Generate_Preview_Internal(TrackWriter trk) => Generate_Internal(trk);
    }
}
