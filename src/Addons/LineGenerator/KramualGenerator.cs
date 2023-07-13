using OpenTK;
using System;
using System.Collections.Generic;
using linerider.Game;
using linerider.Rendering;

namespace linerider.Game.LineGenerator
{
    internal class KramualGenerator : Generator
    {
        public Vector2d position; //Kramual's position
        public bool vert; //Vertical or horizontal?
        public int multiplier = 1;
        public bool reverse = false;
        public LineType lineType;
        public int frame = 0;
        public int iteration = 0;
        public bool overrideFrame = false;
        public bool overrideIteration = false;
        public bool overridePosition = false;
        private double offset = 1E-05;
        private double lineWidth = 1E-05;

        private Vector2d cumulativeMomentum;

        public KramualGenerator(string _name, Vector2d _position, LineType _lineType)
        {
            name = _name;
            position = _position;
            lines = new List<GameLine>();
            lineType = _lineType;
        }

        private void DefineLocation(Rider rider)
        {
            for (int i = 0; i < rider.Body.Length; i++)
            {
                cumulativeMomentum += rider.Body[i].Momentum;
            }
            bool negative = vert ? cumulativeMomentum.X < 0 : cumulativeMomentum.Y < 0;
            position = new Vector2d(rider.Body[0].Location.X, rider.Body[0].Location.Y);
            
            for (int i = 1; i < rider.Body.Length; i++)
            {
                position =  negative ? vert ?
                            rider.Body[i].Location.X > position.X ? new Vector2d(rider.Body[i].Location.X + offset, position.Y) : position :
                            rider.Body[i].Location.Y > position.Y ? new Vector2d(position.X, rider.Body[i].Location.Y + offset) : position :
                            vert ?
                            rider.Body[i].Location.X < position.X ? new Vector2d(rider.Body[i].Location.X - offset, position.Y) : position :
                            rider.Body[i].Location.Y < position.Y ? new Vector2d(position.X, rider.Body[i].Location.Y - offset) : position;
            }
        }

        private void GenerateSingleLine(TrackWriter trk, Vector2d ConPoint, Vector2d MVector)
        {
            bool horMomentumNeg = MVector.X < 0;
            bool verMomentumNeg = MVector.Y < 0;

            double xLeft = vert ? position.X : ConPoint.X + (verMomentumNeg ? offset : -offset);
            double xRight = vert ? position.X : ConPoint.X + (verMomentumNeg ? -offset : offset);
            double yLeft = vert ? ConPoint.Y + (horMomentumNeg ? -offset : offset) : position.Y;
            double yRight = vert ? ConPoint.Y + (horMomentumNeg ? offset : -offset) : position.Y;

            Vector2d lineLeft = new Vector2d(xLeft, yLeft);
            Vector2d lineRight = new Vector2d(xRight, yRight);
            lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, reverse, multiplier));
        }

        public override void Generate_Internal(TrackWriter trk) // Generates the line
        {
            MainWindow game = GameRenderer.Game;
            int fr = overrideFrame ? frame : game.Track.Offset;
            int it = overrideIteration ? iteration : game.Track.IterationsOffset;
            Rider rider = game.Track.Timeline.GetFrame(fr, it);

            if (overridePosition == false)
            {
                DefineLocation(rider);
            }
            for (int i = 0; i < rider.Body.Length; i++)
            {
                GenerateSingleLine(trk, rider.Body[i].Location, rider.Body[i].Momentum);
            }
            return;
        }
        public override void Generate_Preview_Internal(TrackWriter trk)
        {
            Generate_Internal(trk);
            return;
        }
    }
}
