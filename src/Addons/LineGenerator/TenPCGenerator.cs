using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace linerider.Game.LineGenerator
{
    internal class TenPCGenerator : Generator
    {
        public Vector2d speed;
        public double rotation;

        public TenPCGenerator(string _name, Vector2d _speed, double _rotation)
        {
            name = _name;
            lines = new List<GameLine>();
            speed = _speed;
            rotation = _rotation;
        }

        private void GenerateSingleLine(TrackWriter trk, SimulationPoint PointCur, SimulationPoint PointNext, Vector2d PointTarget, double OffsetMult = 1.0) // Generates the line corresponding to a single contact-point, given its position on iteration 1, the next frame's momentum tick, and the intended final momentum tick
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
            Vector2d lineLeft = lineCentre - 0.5 * width * TargetDirection;
            Vector2d lineRight = lineCentre + 0.5 * width * TargetDirection;

            for (int i = 0; i < multilinesRequired; i++)
            {
                lines.Add(CreateLine(trk, lineLeft, lineRight, LineType.Acceleration, inverse, (int)multiplierRequired));
                lineLeft += vert_displacement / 100.0 * NormalDirection;
                lineRight += vert_displacement / 100.0 * NormalDirection;
            }
        }

        public override void Generate_Internal(TrackWriter trk)
        {
            //int startFrame = 0;
            Random rnd = new Random();
            _ = rnd.NextDouble();

            int curFrame = game.Track.Offset;
            Rider curRider = game.Track.Timeline.GetFrame(curFrame);
            Rider curRiderIter1 = game.Track.Timeline.GetFrame(curFrame, 1);
            Rider nextMomentumRider = curRider.Simulate(game.Track.GetTrack(), 0);

            Vector2d[] defaultRider = RiderConstants.CreateDefaultRider();
            Matrix2d rotationMat = Matrix2d.CreateRotation(MathHelper.DegreesToRadians(rotation));

            for (int i = 0; i < defaultRider.Length; i++)
            {
                Vector2d riderRotated = new Vector2d(rotationMat.M11 * defaultRider[i].X + rotationMat.M21 * defaultRider[i].Y,
                                                    rotationMat.M12 * defaultRider[i].X + rotationMat.M22 * defaultRider[i].Y);
                Vector2d target = curRider.Body[0].Location + riderRotated + speed;
                double offset = 1.0 + rnd.NextDouble();
                GenerateSingleLine(trk, curRiderIter1.Body[i], nextMomentumRider.Body[i], target, offset);
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
