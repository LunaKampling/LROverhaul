﻿using OpenTK;
using System;
using System.Collections.Generic;
using linerider.Game;
using linerider.Rendering;

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
            lines = new List<GameLine>();
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
                if (negative) position = new Vector2d(Math.Max(rider.Body[i].Location.X, position.X), Math.Max(rider.Body[i].Location.Y, position.Y));
                else position = new Vector2d(Math.Min(rider.Body[i].Location.X, position.X), Math.Min(rider.Body[i].Location.Y, position.Y));
            }
            var o = negative ? offset : -offset;
            position += new Vector2d(o, o);
        }

        private void GenerateSingleLine(TrackWriter trk, Vector2d ConPoint, Vector2d MVector)
        {
            double xWidth = MVector.X < 0 ? -lineWidth : lineWidth; //Is the momentum negative?
            double yWidth = MVector.Y < 0 ? lineWidth : -lineWidth;

            double xLeft = vert ? position.X : ConPoint.X + yWidth; //calculates line positions
            double xRight = vert ? position.X : ConPoint.X - yWidth;
            double yLeft = vert ? ConPoint.Y + xWidth : position.Y;
            double yRight = vert ? ConPoint.Y - xWidth : position.Y;

            Vector2d lineLeft = new Vector2d(xLeft, yLeft); 
            Vector2d lineRight = new Vector2d(xRight, yRight); 

            if (reverse) lines.Add(CreateLine(trk, lineRight, lineLeft, lineType, reverse, multiplier)); //flips left and right because of the way the reverse draw works
            else lines.Add(CreateLine(trk, lineLeft, lineRight, lineType, reverse, multiplier));
            
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
