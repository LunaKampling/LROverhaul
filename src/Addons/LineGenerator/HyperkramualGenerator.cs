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

        private void GenerateHyperKramLines(TrackWriter trk, Rider rider)
        {
            double xWidth = cumulativeMomentum.X < 0 ? -lineWidth : lineWidth; //Is the momentum negative?
            double yWidth = cumulativeMomentum.Y < 0 ? lineWidth : -lineWidth;

            double xLeft = vert ? position.X : HykPos.X + yWidth; //calculates line position for hyperkramual
            double xRight = vert ? position.X : HykPos.X - yWidth;
            double yLeft = vert ? HykPos.Y + xWidth : position.Y;
            double yRight = vert ? HykPos.Y - xWidth : position.Y;

            Vector2d lineLeft = new Vector2d(xLeft, yLeft);
            Vector2d lineRight = new Vector2d(xRight, yRight);

            lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));

            xLeft = vert ? position.X + 2*xWidth + yWidth : HykPos.X; //calculates line position for line feeding into hyperkramual
            xRight = vert ? position.X + 2*xWidth - yWidth : HykPos.X;
            yLeft = vert ? HykPos.Y : position.Y + xWidth - 2*yWidth;
            yRight = vert ? HykPos.Y : position.Y - xWidth - 2*yWidth;

            lineLeft = new Vector2d(xLeft, yLeft);
            lineRight = new Vector2d(xRight, yRight);

            lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));

            negativeHyk = vert ? cumulativeMomentum.Y < 0 : cumulativeMomentum.X < 0; //is the momentum negative?

            for (var i = 0; i < rider.Body.Length; i++) 
            {
                if (selected_points[i])
                {
                    if (!hasOutliers)
                    {
                        hasOutliers = vert ? negativeHyk ? //is there an outlier amongst the momentumvectors?
                            rider.Body[i].Momentum.Y > 0 :
                            rider.Body[i].Momentum.Y < 0 :
                            negativeHyk ?
                            rider.Body[i].Momentum.X > 0 :
                            rider.Body[i].Momentum.X < 0;
                    }
                    if (firstSelected)
                    {
                        hykFurthest = new Vector2d(rider.Body[i].Location.X, rider.Body[i].Location.Y); //
                    }
                    else
                    {
                        if (negativeHyk) hykFurthest = new Vector2d(Math.Max(rider.Body[i].Location.X, hykFurthest.X), Math.Max(rider.Body[i].Location.Y, hykFurthest.Y));
                        else hykFurthest = new Vector2d(Math.Min(rider.Body[i].Location.X, hykFurthest.X), Math.Min(rider.Body[i].Location.Y, hykFurthest.Y));
                    }
                }
            }
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

                xLeft += vert ? 0 : extensionOffset;
                xRight += vert ? 0: extensionOffset;
                yLeft += vert ? extensionOffset : 0;
                yRight += vert ? extensionOffset : 0;

                lineLeft = new Vector2d(xLeft, yLeft);
                lineRight = new Vector2d(xRight, yRight);

                lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));
            }
        }
        private void GenerateSingleLine(TrackWriter trk, Vector2d ConPoint, Vector2d MVector, int i)
        {
            bool hykPoint = selected_points[i];
            double xWidth = MVector.X < 0 ? -lineWidth : lineWidth; //Is the momentum negative?
            double yWidth = MVector.Y < 0 ? lineWidth : -lineWidth;

            double xLeft = vert ? position.X - (hykPoint ? 2 * o : 0) : ConPoint.X + yWidth; //calculates line positions
            double xRight = vert ? position.X - (hykPoint ? 2 * o : 0) : ConPoint.X - yWidth;
            double yLeft = vert ? ConPoint.Y + xWidth : position.Y - (hykPoint ? 2 * o : 0);
            double yRight = vert ? ConPoint.Y - xWidth : position.Y - (hykPoint ? 2 * o : 0);

            Vector2d lineLeft = new Vector2d(xLeft, yLeft); 
            Vector2d lineRight = new Vector2d(xRight, yRight); 

            lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));
            
            double distanceToConPoint = vert ? position.X - ConPoint.X : position.Y - ConPoint.Y; //calculates distance between kramual and contact point

            if (Math.Abs(distanceToConPoint) > 10) //is this distance more than 1 Gwell?
            {
                double exOff = 10 - offset;
                double extensionOffset = negative ? -distanceToConPoint + exOff : -distanceToConPoint - exOff; //calculates how close the second line can be

                xLeft = vert ? position.X + extensionOffset : ConPoint.X + yWidth;
                xRight = vert ? position.X + extensionOffset : ConPoint.X - yWidth;
                yLeft = vert ? ConPoint.Y + xWidth : position.Y + extensionOffset;
                yRight = vert ? ConPoint.Y - xWidth : position.Y + extensionOffset;

                lineLeft = new Vector2d(xLeft, yLeft);
                lineRight = new Vector2d(xRight, yRight);

                lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, false));
            }
        }

        public override void Generate_Internal(TrackWriter trk) // Generates the line
        {
            MainWindow game = GameRenderer.Game;
            int fr = game.Track.Offset;
            int it = 6;
            Rider rider = game.Track.Timeline.GetFrame(fr, it);

            DefineLocation(rider);

            GenerateHyperKramLines(trk, rider);

            for (int i = 0; i < rider.Body.Length; i++)
            {
                GenerateSingleLine(trk, rider.Body[i].Location, rider.Body[i].Momentum, i);
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
