using linerider.Rendering;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace linerider.Game.LineGenerator
{
    internal class KramualGenerator : Generator
    {
        public Vector2d position; //Kramual's position
        public bool vert = false; //Vertical or horizontal?
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
        private bool negative;

        private Vector2d cumulativeMomentum;

        public KramualGenerator(string _name, Vector2d _position, LineType _lineType)
        {
            name = _name;
            position = _position;
            lines = [];
            lineType = _lineType;
        }

        private void DefineLocation(Rider rider)
        {
            cumulativeMomentum = new Vector2d(0, 0);
            for (int i = 0; i < rider.Body.Length; i++)
            {
                cumulativeMomentum += rider.Body[i].Momentum; //adds all momentum together
            }

            negative = vert ? cumulativeMomentum.X < 0 : cumulativeMomentum.Y < 0; //is the momentum negative?
            position = new Vector2d(rider.Body[0].Location.X, rider.Body[0].Location.Y);

            for (int i = 1; i < rider.Body.Length; i++) //depending on momentum and axis identifies the outmost contact point
            {
                position = negative ? vert ?
                            rider.Body[i].Location.X > position.X ? new Vector2d(rider.Body[i].Location.X + offset, position.Y) : position :
                            rider.Body[i].Location.Y > position.Y ? new Vector2d(position.X, rider.Body[i].Location.Y + offset) : position :
                            vert ?
                            rider.Body[i].Location.X < position.X ? new Vector2d(rider.Body[i].Location.X - offset, position.Y) : position :
                            rider.Body[i].Location.Y < position.Y ? new Vector2d(position.X, rider.Body[i].Location.Y - offset) : position;
            }
        }

        private void GenerateSingleLine(TrackWriter trk, Vector2d ConPoint, Vector2d MVector)
        {
            bool horMomentumNeg = MVector.X < 0; //Is the momentum negative for this contact point?
            bool verMomentumNeg = MVector.Y < 0;

            double xLeft = vert ? position.X : ConPoint.X + (verMomentumNeg ? lineWidth : -lineWidth); //calculates line positions
            double xRight = vert ? position.X : ConPoint.X + (verMomentumNeg ? -lineWidth : lineWidth);
            double yLeft = vert ? ConPoint.Y + (horMomentumNeg ? -lineWidth : lineWidth) : position.Y;
            double yRight = vert ? ConPoint.Y + (horMomentumNeg ? lineWidth : -lineWidth) : position.Y;

            Vector2d lineLeft = new(xLeft, yLeft);
            Vector2d lineRight = new(xRight, yRight);

            if (reverse) lines.Add(CreateLine(trk, lineRight, lineLeft, lineType, reverse, multiplier)); //flips left and right because of the way the reverse draw works
            else lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, reverse, multiplier));

            double distanceToConPoint = vert ? position.X - ConPoint.X : position.Y - ConPoint.Y; //calculates distance between kramual and contact point

            if (distanceToConPoint > 10 | distanceToConPoint < -10) //is this distance more than 1 Gwell?
            {
                double extensionOffset = negative ? -distanceToConPoint + 10 - offset : -distanceToConPoint - 10 + offset; //calculates how close the second line can be

                xLeft = vert ? position.X + extensionOffset : ConPoint.X + (verMomentumNeg ? lineWidth : -lineWidth);
                xRight = vert ? position.X + extensionOffset : ConPoint.X + (verMomentumNeg ? -lineWidth : lineWidth);
                yLeft = vert ? ConPoint.Y + (horMomentumNeg ? -lineWidth : lineWidth) : position.Y + extensionOffset;
                yRight = vert ? ConPoint.Y + (horMomentumNeg ? lineWidth : -lineWidth) : position.Y + extensionOffset;

                lineLeft = new Vector2d(xLeft, yLeft);
                lineRight = new Vector2d(xRight, yRight);

                if (reverse) lines.Add(CreateLine(trk, lineRight, lineLeft, lineType, reverse, multiplier));
                else lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, reverse, multiplier));
            }
        }

        public override void Generate_Internal(TrackWriter trk) // Generates the line
        {
            MainWindow game = GameRenderer.Game;
            int fr = overrideFrame ? frame : game.Track.Offset;
            int it = overrideIteration ? iteration : game.Track.IterationsOffset;
            Rider rider = game.Track.Timeline.GetFrame(fr, it);

            if (overridePosition == false) //check if position even needs to be calculated
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