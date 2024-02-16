using OpenTK;
using System;
using System.Collections.Generic;
using linerider.Game;
using linerider.Rendering;

namespace linerider.Game.LineGenerator
{
    internal class HyperkramualGenerator : Generator
    {
        public Vector2d position; //Kramual's position
        public Vector2d hykFurthest; //Hyperkramual's most outlying contact point
        public Vector2d MTickOffset = new Vector2d(10, 10); //Momentum Tick Offset
        public bool vert = false; //Vertical or horizontal?
        public bool hasOutliers; //Is a momentum vector going in a different direction than the norm?
        public LineType lineType = LineType.Standard; 
        // public int frame = 0;
        // public int iteration = 0;
        // public bool overrideFrame = false;
        // public bool overrideIteration = false;
        private double offset = 1E-05;
        private double o;
        private double lineWidth = 1E-05;
        private bool negative;
        private bool negativeHyk;
        public bool[] selected_points;
        public bool firstSelected = true;

        public Vector2d HykPos = new Vector2d(10,-10);

        private Vector2d cumulativeMomentum;

        public HyperkramualGenerator(string _name, Vector2d _position)
        {
            name = _name;
            position = _position;
            lines = new List<GameLine>();
            selected_points = new bool[10];
            selected_points[4] = true;
            selected_points[5] = true;
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
                if (negative) position = new Vector2d(Math.Max(rider.Body[i].Location.X, position.X), Math.Max(rider.Body[i].Location.Y, position.Y));
                else position = new Vector2d(Math.Min(rider.Body[i].Location.X, position.X), Math.Min(rider.Body[i].Location.Y, position.Y));
            }
            o = negative ? offset : -offset;
            position += new Vector2d(3*o, 3*o);
        }

        private void WrongSide(TrackWriter trk, Vector2d ConPoint, Vector2d MVector)
        {
            negativeHyk = vert ? cumulativeMomentum.Y < 0 : cumulativeMomentum.X < 0; //is the momentum negative?
            bool wrongSide;
            var bigOffset = 10 * offset;

            if (vert)
                {
                    wrongSide = (HykPos.Y > ConPoint.Y && MVector.Y > 0) | (HykPos.Y < ConPoint.Y && MVector.Y < 0); //is the contact point on the wrong side?

                    if (wrongSide)
                    {
                        var n = negative ? 1 : -1;
                        Vector2d m = MVector + new Vector2d(1e-05 * n, 0);
                        m = negativeHyk ? m.PerpendicularLeft : m.PerpendicularRight;
                        var diff = Math.Abs(HykPos.Y - ConPoint.Y) + bigOffset;
                        m *= diff / Math.Abs(m.Y); //how far do we need to move it?

                        Vector2d lineLeft = ConPoint + m + bigOffset * m.PerpendicularRight;
                        Vector2d lineRight = ConPoint + m + bigOffset * m.PerpendicularLeft;

                        lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));

                        position.X = negative ? 
                            Math.Max(Math.Max(lineLeft.X, lineRight.X) + bigOffset, position.X) : 
                            Math.Min(Math.Min(lineLeft.X, lineRight.X) - bigOffset, position.X); //Did we move the contact point further than the original kramual position?
                    }
                }
                else
                {
                    wrongSide = (HykPos.X > ConPoint.X && MVector.X > 0) | (HykPos.X < ConPoint.X && MVector.X < 0); //Is the contact point on the wrong side?

                    if (wrongSide)
                    {
                        var n = negativeHyk ? 1 : -1;
                        Vector2d m = MVector + new Vector2d(1e-05 * n, 0);
                        m = negativeHyk ? m.PerpendicularLeft : m.PerpendicularRight;
                        var diff = Math.Abs(HykPos.X - ConPoint.X) + bigOffset;
                        m *= diff / m.X; //How far do we need to move it?

                        Vector2d lineLeft = ConPoint + m + bigOffset * m.PerpendicularRight;
                        Vector2d lineRight = ConPoint + m + bigOffset * m.PerpendicularLeft;

                        lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));

                        position.Y = negative ? 
                            Math.Max(Math.Max(lineLeft.Y, lineRight.Y) + offset, position.Y) : 
                            Math.Min(Math.Min(lineLeft.Y, lineRight.Y) - offset, position.Y); //Did we move the contact point further than the original kramual position?
                    }
                }
        }

        private void GenerateHyperKramLines(TrackWriter trk, Rider rider)
        {
            double xWidth = cumulativeMomentum.X < 0 ? -lineWidth : lineWidth; //Is the momentum negative?
            double yWidth = cumulativeMomentum.Y < 0 ? lineWidth : -lineWidth;

            double xLeft  = vert ? position.X : HykPos.X + yWidth; //calculates line position for hyperkramual
            double xRight = vert ? position.X : HykPos.X - yWidth;
            double yLeft  = vert ? HykPos.Y + xWidth : position.Y;
            double yRight = vert ? HykPos.Y - xWidth : position.Y;

            Vector2d lineLeft = new Vector2d(xLeft, yLeft);
            Vector2d lineRight = new Vector2d(xRight, yRight);

            lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));

            var bigOffset = 8 * o;

            xLeft  = vert ? position.X + 2*xWidth + yWidth - bigOffset : HykPos.X; //calculates line position for line feeding into hyperkramual
            xRight = vert ? position.X + 2*xWidth - yWidth - bigOffset : HykPos.X;
            yLeft  = vert ? HykPos.Y : position.Y + xWidth - 2*yWidth - bigOffset;
            yRight = vert ? HykPos.Y : position.Y - xWidth - 2*yWidth - bigOffset;

            lineLeft = new Vector2d(xLeft, yLeft);
            lineRight = new Vector2d(xRight, yRight);

            lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));

            negativeHyk = vert ? cumulativeMomentum.Y < 0 : cumulativeMomentum.X < 0; //is the momentum negative?

            for (var i = 0; i < rider.Body.Length; i++)
            {
                if (selected_points[i])
                {
                    if (!hasOutliers) //still gotta fix oops
                    {
                        var tested = vert ? rider.Body[i].Momentum.Y : rider.Body[i].Momentum.X;
                        hasOutliers = negativeHyk ? (tested > 0) : (tested < 0);
                    }

                    if (firstSelected)
                    {
                        hykFurthest = rider.Body[i].Location;
                        firstSelected = false;
                    }
                    else
                    {
                        if (negativeHyk) hykFurthest = new Vector2d(Math.Max(rider.Body[i].Location.X, hykFurthest.X), Math.Max(rider.Body[i].Location.Y, hykFurthest.Y));
                        else hykFurthest = new Vector2d(Math.Min(rider.Body[i].Location.X, hykFurthest.X), Math.Min(rider.Body[i].Location.Y, hykFurthest.Y));
                    }
                }
            }
            firstSelected = true;

            Console.WriteLine("Outliers: " + hasOutliers);
            if (hasOutliers)
            {
                lines.Add(CreateLine(trk, lineRight, lineLeft, lineType, false)); //add feeding line for outlier
                hasOutliers = false;
            }

            double hykDistance = vert ? hykFurthest.Y - HykPos.Y : hykFurthest.X - HykPos.X;
            if (Math.Abs(hykDistance) > 10) //is this distance more than 1 Gwell?
            {
                double exOff = 10 - offset;
                double extensionOffset = negativeHyk ? hykDistance + exOff : hykDistance - exOff; //calculates how close the second line can be

                xLeft  += vert ? 0 : extensionOffset;
                xRight += vert ? 0 : extensionOffset;
                yLeft  += vert ? extensionOffset : 0;
                yRight += vert ? extensionOffset : 0;

                lineLeft = new Vector2d(xLeft, yLeft);
                lineRight = new Vector2d(xRight, yRight);

                lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));
            }
        }
        private void GenerateSingleLine(TrackWriter trk, Vector2d ConPoint, Vector2d MVector, bool hykPoint)
        {
            double xWidth = MVector.X < 0 ? -lineWidth : lineWidth; //Is the momentum negative?
            double yWidth = MVector.Y < 0 ? lineWidth : -lineWidth;

            double xLeft  = vert ? position.X - (hykPoint ? 10 * o : 0) : ConPoint.X + yWidth; //calculates line positions
            double xRight = vert ? position.X - (hykPoint ? 10 * o : 0) : ConPoint.X - yWidth;
            double yLeft  = vert ? ConPoint.Y + xWidth : position.Y - (hykPoint ? 10 * o : 0);
            double yRight = vert ? ConPoint.Y - xWidth : position.Y - (hykPoint ? 10 * o : 0);

            Vector2d lineLeft = new Vector2d(xLeft, yLeft); 
            Vector2d lineRight = new Vector2d(xRight, yRight);

            //Console.WriteLine(ConPoint.ToString() + lineLeft.ToString() + lineRight.ToString());

            lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));
            
            double distanceToConPoint = vert ? position.X - ConPoint.X : position.Y - ConPoint.Y; //calculates distance between kramual and contact point

            if (Math.Abs(distanceToConPoint) > 10) //is this distance more than 1 Gwell?
            {
                double exOff = 10 - offset;
                double extensionOffset = negative ? -distanceToConPoint + exOff : -distanceToConPoint - exOff; //calculates how close the second line can be

                xLeft  = vert ? position.X + extensionOffset : ConPoint.X + yWidth;
                xRight = vert ? position.X + extensionOffset : ConPoint.X - yWidth;
                yLeft  = vert ? ConPoint.Y + xWidth : position.Y + extensionOffset;
                yRight = vert ? ConPoint.Y - xWidth : position.Y + extensionOffset;

                lineLeft = new Vector2d(xLeft, yLeft);
                lineRight = new Vector2d(xRight, yRight);

                lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));
            }
        }

        private void Generate10PCSingleLine(TrackWriter trk, SimulationPoint PointCur, SimulationPoint PointNext, Vector2d PointTarget, double OffsetMult = 1.0) // Generates the line corresponding to a single contact-point, given its position on iteration 1, the next frame's momentum tick, and the intended final momentum tick
        {
            const double vert_displacement = 1.0e-3; // How far to displace the contact-point along the line
            const double width = 1.0e-5; // How wide the line is

            bool inverse = false;
            Vector2d TargetDirection = PointTarget - PointNext.Location; // Normalised direction to move the point
            double speedRequired = TargetDirection.Length; // The speed required to get the momentum tick point to the correct location
            TargetDirection /= TargetDirection.Length; // Normalised
            Vector2d NormalDirection = TargetDirection.PerpendicularLeft; // Line normal (initially assumes not inverted)

            if (Vector2d.Dot(PointCur.Momentum, NormalDirection) <= 0) // If the line's direction is wrong, it needs to be inverted
            {
                inverse = true;
                NormalDirection = TargetDirection.PerpendicularRight;
            }

            double multiplierRequired = speedRequired / RedLine.ConstAcc; // These will be converted to ints later
            int multilinesRequired = (int)Math.Ceiling(multiplierRequired / 255.0);
            multiplierRequired /= multilinesRequired; // This now represents the multiplier per line

            Vector2d lineCentre = PointCur.Location - vert_displacement * OffsetMult * NormalDirection;
            Vector2d lineLeft  = lineCentre - 0.5 * width * TargetDirection;
            Vector2d lineRight = lineCentre + 0.5 * width * TargetDirection;

            for (int i = 0; i < multilinesRequired; i++)
            {
                lines.Add(CreateLine(trk, lineLeft, lineRight, LineType.Acceleration, inverse, (int)multiplierRequired));
                lineLeft  += vert_displacement / 100.0 * NormalDirection;
                lineRight += vert_displacement / 100.0 * NormalDirection;
            }
        }

        private void HykGen(TrackWriter trk, Rider rider, int fr, int it)
        {
            DefineLocation(rider);
            GenerateHyperKramLines(trk, rider);

            for (var i = 0; i < rider.Body.Length; i++)
            {
                if (selected_points[i]) WrongSide(trk, rider.Body[i].Location, rider.Body[i].Momentum);
            }

            game.Track.NotifyTrackChanged();
            rider = game.Track.Timeline.GetFrame(fr, it); //needs to be updated for kramual line generation to work properly

            for (int i = 0; i < rider.Body.Length; i++)
            {
                GenerateSingleLine(trk, rider.Body[i].Location, rider.Body[i].Momentum, selected_points[i]);
            }
        }
        public override void Generate_Internal(TrackWriter trk) // Generates the line
        {
            MainWindow game = GameRenderer.Game;
            int fr = game.Track.Offset;
            int it = 6;
            Rider rider = game.Track.Timeline.GetFrame(fr, it);

            HykGen(trk, rider, fr, it); //generating future momentum tick data based on final hyperkramual position

            game.Track.NotifyTrackChanged(); //updating game so momentum tick of next frame is properly calculated (we're not doing that by hand, why would we?)
            Rider curRiderIt1 = game.Track.Timeline.GetFrame(fr, 1);
            Rider curRiderMTick = game.Track.Timeline.GetFrame(fr, 0);
            Rider futureRider = game.Track.Timeline.GetFrame(fr + 1, 0); //laying down the inputs for 10pc gen

            foreach (GameLine line in lines)
            {
                trk.RemoveLine(line);
            }
            lines.Clear(); //removing all the hyperkramual lines because of future 10pc

            for (int i = 0; i < rider.Body.Length; i++)
            {
                Random rnd = new Random();
                _ = rnd.NextDouble();

                double offset = 1.0 + rnd.NextDouble();
                Vector2d target = curRiderMTick.Body[i].Location + MTickOffset;

                Generate10PCSingleLine(trk, curRiderIt1.Body[i], futureRider.Body[i], target, offset);
            } //autostabilizing 10pc based on momentum tick generated based on hyperkramual data

            game.Track.NotifyTrackChanged();
            rider = game.Track.Timeline.GetFrame(fr, it); //updating the game, again, for the hyperkramual

            HykGen(trk, rider, fr, it); 

            return;
        }
        public override void Generate_Preview_Internal(TrackWriter trk)
        {
            Generate_Internal(trk);
            return;
        }
    }
}
