//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using OpenTK;
using System;

namespace linerider.Game
{
    public class StandardLine : GameLine
    {
        public static double Zone = 10;
        /// <summary>
        /// Extension direction
        /// </summary>
        public enum Ext
        {
            // Imperative to trk format this does not exceed 2 bits
            None = 0,
            Left = 1,
            Right = 2,
            Both = 3
        }

        public Vector2d DiffNormal;
        public double ExtensionRatio;
        protected double DotScalar;
        public double Distance;
        public Ext Extension;
        public LineTrigger Trigger = null;
        public Vector2d Difference;
        public bool inv = false;

        protected double limit_left => Extension.HasFlag(Ext.Left) ? -ExtensionRatio : 0.0;
        protected double limit_right => Extension.HasFlag(Ext.Right) ? 1.0 + ExtensionRatio : 1.0;

        /// <summary>
        /// "Left" according to the inv field
        /// </summary>
        public override Vector2d Start => inv ? Position2 : Position1;
        /// <summary>
        /// "Right" according to the inv field
        /// </summary>
        public override Vector2d End => inv ? Position1 : Position2;
        public override LineType Type => LineType.Standard;
        public override System.Drawing.Color Color => Settings.Colors.StandardLine;

        protected StandardLine()
        {
        }
        public StandardLine(Vector2d p1, Vector2d p2, bool inv = false)
        {
            Position1 = p1;
            Position2 = p2;
            this.inv = inv;
            CalculateConstants();
            Extension = Ext.None;
        }
        public override string ToString() => "{" +
                "\"type\":0," +
                $"\"x1\":{Position1.X}," +
                $"\"y1\":{Position1.Y}," +
                $"\"x2\":{Position2.X}," +
                $"\"y2\":{Position2.Y}," +
                $"\"flipped\":{(inv ? "true" : "false")}," +
                $"\"leftExtended\":{(Extension == Ext.Left || Extension == Ext.Both ? "true" : "false")}," +
                $"\"rightExtended\":{(Extension == Ext.Right || Extension == Ext.Both ? "true" : "false")}" +
                "}";
        /// <summary>
        /// Calculates the line constants, needs called if a point changes.
        /// </summary>
        public virtual void CalculateConstants()
        {
            Difference = Position2 - Position1;
            double sqrDistance = Difference.LengthSquared;
            DotScalar = 1 / sqrDistance;
            Distance = Math.Sqrt(sqrDistance);

            DiffNormal = Difference * (1 / Distance); // Normalize
            // Flip to be the angle towards the top of the line
            DiffNormal = inv ? DiffNormal.PerpendicularRight : DiffNormal.PerpendicularLeft;
            ExtensionRatio = Math.Min(0.25, Zone / Distance);
        }
        public virtual bool Interact(ref SimulationPoint p)
        {
            if (Vector2d.Dot(p.Momentum, DiffNormal) > 0)
            {
                Vector2d startDelta = p.Location - Position1;
                double disty = Vector2d.Dot(DiffNormal, startDelta);
                if (disty > 0 && disty < Zone)
                {
                    double distx = Vector2d.Dot(startDelta, Difference) * DotScalar;
                    if (distx <= limit_right && distx >= limit_left)
                    {
                        Vector2d pos = p.Location - disty * DiffNormal;
                        _ = p.Previous;
                        if (p.Friction != 0)
                        {
                            Vector2d friction = DiffNormal.Yx * p.Friction * disty;
                            if (p.Previous.X >= pos.X)
                                friction.X = -friction.X;
                            if (p.Previous.Y >= pos.Y)
                                friction.Y = -friction.Y;
                            p = p.Replace(pos, p.Previous + friction);
                        }
                        else
                        {
                            p = p.Replace(pos);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public override GameLine Clone()
        {
            LineTrigger trigger = null;
            if (Trigger != null)
            {
                trigger = new LineTrigger()
                {
                    ZoomTrigger = Trigger.ZoomTrigger,
                    ZoomFrames = Trigger.ZoomFrames,
                    ZoomTarget = Trigger.ZoomTarget
                };
            }
            return new StandardLine()
            {
                ID = ID,
                Difference = Difference,
                DiffNormal = DiffNormal,
                Distance = Distance,
                DotScalar = DotScalar,
                Extension = Extension,
                ExtensionRatio = ExtensionRatio,
                inv = inv,
                Position1 = Position1,
                Position2 = Position2,
                Trigger = trigger
            };
        }

        public static StandardLine CloneFromRed(StandardLine standardLine)
        {
            LineTrigger trigger = null;
            if (standardLine.Trigger != null)
            {
                trigger = new LineTrigger()
                {
                    ZoomTrigger = standardLine.Trigger.ZoomTrigger,
                    ZoomFrames = standardLine.Trigger.ZoomFrames,
                    ZoomTarget = standardLine.Trigger.ZoomTarget
                };
            }
            StandardLine newLine = new StandardLine()
            {
                ID = standardLine.ID,
                Extension = standardLine.Extension,
                inv = standardLine.inv,
                Position1 = standardLine.Position1,
                Position2 = standardLine.Position2,
                Trigger = trigger,
            };
            newLine.CalculateConstants();
            return newLine;
        }
    }
}
