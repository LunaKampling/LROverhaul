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
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using linerider.Game;
using linerider.Utils;
using System.Security.Policy;

namespace linerider.Game
{

    public struct Rider
    {
        /// <summary>
        /// Represents the bounds of the grid cells used to simulate this rider state.
        /// </summary>
        public readonly RectLRTB PhysicsBounds;
        public readonly ImmutablePointCollection Body;
        public readonly ImmutablePointCollection Scarf;
        public readonly bool Crashed;
        public readonly bool SledBroken;
        public bool UseRemount;
        public readonly int remountState; //0 = Alive, 1 = dead cooldown, 2 = dead can remount, 3 = remounting
        public readonly int remountTimer;
        private Rider(SimulationPoint[] body, SimulationPoint[] scarf, RectLRTB physbounds, bool dead, bool sledbroken, bool useRemount, int rState = 0, int rTimer = 0)
        {
            Body = new ImmutablePointCollection(body);
            Scarf = new ImmutablePointCollection(scarf);
            Crashed = dead;
            SledBroken = sledbroken;
            PhysicsBounds = physbounds;
            UseRemount = useRemount;
            remountState = rState;
            remountTimer = rTimer;
        }
        public static Rider Create(Vector2d start, Vector2d momentum, bool useRemount, bool frictionless)
        {
            var joints = new SimulationPoint[RiderConstants.DefaultRider.Length];
            var scarf = new SimulationPoint[RiderConstants.DefaultScarf.Length + 1];
            RectLRTB pbounds = new RectLRTB();
            for (int i = 0; i < joints.Length; i++)
            {
                var coord = (RiderConstants.DefaultRider[i] + start);
                var prev = coord - momentum;
                switch (i)
                {
                    case RiderConstants.SledTL:
                    case RiderConstants.BodyButt:
                    case RiderConstants.BodyShoulder:
                        joints[i] = new SimulationPoint(coord, prev, Vector2d.Zero, frictionless ? 0 : 0.8);
                        break;
                    case RiderConstants.BodyHandLeft:
                    case RiderConstants.BodyHandRight:
                        joints[i] = new SimulationPoint(coord, prev, Vector2d.Zero, frictionless ? 0 : 0.1);
                        break;
                    default:
                        joints[i] = new SimulationPoint(coord, prev, Vector2d.Zero, frictionless ? 0 : 0.0);
                        break;
                }
                if (i == 0)
                {
                    pbounds = new RectLRTB(ref joints[0]);
                }
                else
                {
                    var cellx = (int)Math.Floor(joints[i].Location.X / 14);
                    var celly = (int)Math.Floor(joints[i].Location.Y / 14);

                    pbounds.left = Math.Min(cellx - 1, pbounds.left);
                    pbounds.top = Math.Min(celly - 1, pbounds.top);
                    pbounds.right = Math.Max(cellx + 1, pbounds.right);
                    pbounds.bottom = Math.Max(celly + 1, pbounds.bottom);
                }
            }
            scarf[0] = joints[RiderConstants.BodyShoulder];
            for (int i = 0; i < RiderConstants.DefaultScarf.Length; i++)
            {
                var pos = scarf[0].Location + RiderConstants.DefaultScarf[i];
                scarf[i + 1] = new SimulationPoint(pos, pos, Vector2d.Zero, 0.9);
            }
            return new Rider(joints, scarf, pbounds, false, false, useRemount);
        }


        public Vector2d CalculateCenter()
        {
            if (Crashed)
                return Body[4].Location;
            var anchorsaverage = new Vector2d();
            for (int i = 0; i < Body.Length; i++)
            {
                anchorsaverage += Body[i].Location;
            }
            return anchorsaverage / Body.Length;
        }

        public Vector2d CalculateMomentum()
        {
            var mo = Vector2d.Zero;
            for (int i = 0; i < Body.Length; i++)
            {
                mo += Body[i].Momentum;
            }
            mo /= Body.Length;
            return mo;
        }
        public static Rider Lerp(Rider r1, Rider r2, float percent)
        {
            SimulationPoint[] joints = new SimulationPoint[r1.Body.Length];
            SimulationPoint[] scarf = new SimulationPoint[r1.Scarf.Length];
            bool dead = r1.Crashed;
            for (int i = 0; i < joints.Length; i++)
            {
                joints[i] = r1.Body[i].Replace(Vector2d.Lerp(r1.Body[i].Location, r2.Body[i].Location, percent));
            }
            for (int i = 0; i < scarf.Length; i++)
            {
                scarf[i] = r1.Scarf[i].Replace(Vector2d.Lerp(r1.Scarf[i].Location, r2.Scarf[i].Location, percent));
            }
            return new Rider(joints, scarf, r1.PhysicsBounds, dead, r1.SledBroken, r1.UseRemount);
        }
        private static void FlutterScarf(SimulationPoint[] scarf, int frameid, double momentum)
        {
            var baseoffset = frameid;
            // this creates a sense of 'progression' in the flutter, otherwise
            // it feels robotic
            baseoffset += (frameid / 20);

            int rate = (20 - (int)(14 * Math.Min(1, momentum / 100)));
            int offset = baseoffset % rate;
            for (int i = 1; i < scarf.Length; i++)
            {
                double scalar = (i - 1.0) / (scarf.Length - 1);

                var flutter = WaveFlutter(
                    scarf[i].Location,
                    scarf[i - 1].Location,
                    scalar * (Math.PI * 4) + offset);
                flutter *= momentum / 100;

                if (Utility.fastrand(frameid) % 4 == 0)
                    flutter *= scalar * 2;
                scarf[i] = scarf[i].Replace(scarf[i].Location, scarf[i].Previous + flutter);
            }
        }
        private static Vector2d WaveFlutter(Vector2d pos, Vector2d prev, double radian)
        {
            var ang = Angle.FromLine(prev, pos);
            ang.Degrees += Math.Cos(radian) * 90;
            return ang.MovePoint(Vector2d.Zero, 2);
        }
        private unsafe static void ProcessLines(
            ISimulationGrid grid,
            SimulationPoint[] body,
            ref RectLRTB physinfo,
            LinkedList<int> collisions = null)
        {
            int bodylen = body.Length;
            for (int i = 0; i < bodylen; i++)
            {
                var startpos = body[i].Location;
                var cellx = (int)Math.Floor(startpos.X / 14);
                var celly = (int)Math.Floor(startpos.Y / 14);

                //every itreration is at least 3x3, so asjust the info for that
                int newbox = (int)Math.Floor(1 + (StandardLine.Zone / 14));

                physinfo.left = Math.Min(cellx - newbox, physinfo.left);
                physinfo.top = Math.Min(celly - newbox, physinfo.top);
                physinfo.right = Math.Max(cellx + newbox, physinfo.right);
                physinfo.bottom = Math.Max(celly + newbox, physinfo.bottom);
                for (var x = 0-newbox; x <= newbox; x++)
                {
                    for (var y = 0-newbox; y <= newbox; y++)
                    {
                        var cell = grid.GetCell(cellx + x, celly + y);
                        if (cell == null)
                            continue;
                        foreach (var line in cell)
                        {
                            if (line.Interact(ref body[i]))
                            {
                                collisions?.AddLast(line.ID);
                            }
                        }
                    }
                }
            }
        }
        public static void ProcessScarfBones(Bone[] bones, SimulationPoint[] scarf)
        {
            for (int i = 0; i < scarf.Length-1; i++)
            {
                var bone = bones[i];
                int j1 = bone.joint1;
                int j2 = bone.joint2;
                var d = scarf[j1].Location - scarf[j2].Location;
                var len = d.Length;
                if (!bone.OnlyRepel || len < bone.RestLength)
                {
                    double scalar = ((len - bone.RestLength) / len);
                    scarf[j2] = scarf[j2].AddPosition(d * scalar);
                }
            }
        }
        public unsafe static bool TestSurvivable(Bone[] bones, SimulationPoint[] body, double enduranceMultiplier = 1.0) //Tests if a given Bosh state can survive with a given endurance multiplier
        {
            int bonelen = bones.Length;
            for (int i = 0; i < bonelen; i++)
            {
                var bone = bones[i];
                int j1 = bone.joint1;
                int j2 = bone.joint2;
                var d = body[j1].Location - body[j2].Location;
                var len = d.Length;

                if (!bone.OnlyRepel || len < bone.RestLength)
                {
                    var scalar = (len - bone.RestLength) / len * 0.5;
                    // instead of 0 checking dista the rationale is technically dista could be really really small
                    // and round off into infinity which gives us the NaN error.
                    if (double.IsInfinity(scalar))
                    {
                        scalar = 0;
                    }
                    if (bone.Breakable && (scalar > bone.RestLength * RiderConstants.EnduranceFactor * enduranceMultiplier))
                    {
                        return false; //If any of Bosh's bones is unsurvivable, return false as this position will kill Bosh
                    }
                }
            }

            return true; //If none of Bosh's bones are unsurvivable, the state survives so return true
        }

        public unsafe static void ProcessRemount(Bone[] bones, SimulationPoint[] body, ref bool dead, ref bool sledbroken, ref int rState, ref int rTimer)
        {
            if (sledbroken == true)
            {
                rState = 2; //Set just in case he gets stuck in remount state with no sled or something
                return;
            }

            switch (rState)
            {
                default: //Invalid remount state
                    break;
                case 0: //Rider on sled
                    if (dead) //If Bosh dies, enter dismounting state, set timer to 0
                    {
                        rState = 1;
                        rTimer = 0;
                    }
                    break;
                case 1: //Dismounting (dead & cannot remount)
                    if (rTimer >= 30)
                    {
                        rState = 2;
                        rTimer = 0;
                    }
                    else { rTimer += 1; }
                    break;
                case 2: //Dismounted (dead & can remount)
                    if (TestSurvivable(bones, body, 2.0))
                    {
                        if (rTimer >= 3) //If Bosh has been within 2x endurance range for 3 consecutive frames, go to remounting phase
                        {
                            rState = 3;
                            dead = false;
                            rTimer = 0;
                        }
                        else { rTimer += 1; }

                    }
                    else { rTimer = 0; }
                    break;
                case 3: //Remounting
                    if (!TestSurvivable(bones, body, 2.0) || dead) //If Bosh can't survive with 2x endurance range, go back to dismounted phase (TODO check if this is redundant)
                    {
                        rState = 2;
                        rTimer = 0;
                        dead = true;
                    }
                    if (TestSurvivable(bones, body, 1.0))
                    {
                        if (rTimer >= 3) //If Bosh has been within the standard endurance range for 3 consecutive frames, go to normal mounted phase
                        {
                            rState = 0;
                            rTimer = 0;
                        }
                        else { rTimer += 1; }
                    }
                    else { rTimer = 0; }
                    break;
            }
        }

        public unsafe static void ProcessBones(Bone[] bones, SimulationPoint[] body, ref bool dead, ref int rState, List<int> breaks = null)
        {
            int bonelen = bones.Length;

            double strengthMult = rState == 3 ? RiderConstants.RemountStrengthMultiplier : 1.0;
            double enduranceMult = rState == 3 ? RiderConstants.RemountEnduranceMultiplier : 1.0;

            for (int i = 0; i < bonelen; i++)
            {
                var bone = bones[i];
                int j1 = bone.joint1;
                int j2 = bone.joint2;
                var d = body[j1].Location - body[j2].Location;
                var len = d.Length;

                if (!bone.OnlyRepel || len < bone.RestLength)
                {
                    var scalar = (len - bone.RestLength) / len * 0.5;
                    // instead of 0 checking dista the rationale is technically dista could be really really small
                    // and round off into infinity which gives us the NaN error.
                    if (double.IsInfinity(scalar))
                    {
                        scalar = 0;
                    }
                    if (bone.Breakable && (dead || scalar > bone.RestLength * RiderConstants.EnduranceFactor * enduranceMult))
                    {
                        dead = true;
                        breaks?.Add(i);
                    }
                    else
                    {
                        d *= scalar * strengthMult;
                        body[j1] = body[j1].AddPosition(-d);
                        body[j2] = body[j2].AddPosition(d);
                    }
                }
            }
        }
        public static DoubleRect GetBounds(Rider r)
        {
            double left, right, top, bottom;
            right = left = r.Body[0].Location.X;
            top = bottom = r.Body[0].Location.Y;
            for (int i = 0; i < r.Body.Length; i++)
            {
                var pos = r.Body[i].Location;
                right = Math.Max(pos.X, right);
                left = Math.Min(pos.X, left);
                top = Math.Min(pos.Y, top);
                bottom = Math.Max(pos.Y, bottom);
            }
            DoubleRect ret = new DoubleRect(left, top, right - left, bottom - top);
            return ret;
        }
        public Rider Simulate(Track track, int maxiteration = 6, LinkedList<int> collisions = null)
        {
            return Simulate(track.Grid, track.Bones, collisions, maxiteration);
        }
        public Rider Simulate(
            ISimulationGrid grid,
            Bone[] bones,
            LinkedList<int> collisions,
            int maxiteration = 6,
            bool stepscarf = true,
            int frameid = 0)
        {
            SimulationPoint[] body = Body.Step();
            int bodylen = Body.Length;
            bool dead = Crashed;
            bool sledbroken = SledBroken;
            int rState = remountState;
            int rTimer = remountTimer;
            RectLRTB phys = new RectLRTB(ref body[0]);
            using (grid.Sync.AcquireRead())
            {
                for (int i = 0; i < maxiteration; i++)
                {
                    ProcessBones(bones, body, ref dead, ref rState);
                    ProcessLines(grid, body, ref phys, collisions);
                }
            }
            if (maxiteration == 6)
            {
                var nose = body[RiderConstants.SledTR].Location - body[RiderConstants.SledTL].Location;
                var tail = body[RiderConstants.SledBL].Location - body[RiderConstants.SledTL].Location;
                var head = body[RiderConstants.BodyShoulder].Location - body[RiderConstants.BodyButt].Location;
                if (!dead && ((nose.X * tail.Y) - (nose.Y * tail.X) < 0 || // tail fakie
                             (nose.X * head.Y) - (nose.Y * head.X) > 0))   // head fakie
                {
                    dead = true;
                    sledbroken = true;
                }
            }
            
            if (UseRemount)
            {
                ProcessRemount(bones, body, ref dead, ref sledbroken, ref rState, ref rTimer);
            }
            
            SimulationPoint[] scarf;
            if (stepscarf)
            {
                scarf = Scarf.Step(friction: true);

                if (Settings.multiScarfAmount * Settings.multiScarfSegments > RiderConstants.ScarfBones.Length) { Settings.multiScarfAmount = 1; } //if too big set to zero

                if (Settings.multiScarfAmount > 1) //If using dual scarf
                {
                    
                    List<SimulationPoint>[] scarves = new List<SimulationPoint>[Settings.multiScarfAmount];
                    List<SimulationPoint> finalScarf = new List<SimulationPoint>();

                    for (int i = 0; i < Settings.multiScarfAmount; i++) {
                        scarf[i * Settings.multiScarfSegments] = body[RiderConstants.BodyShoulder];
                        scarves[i] = new List<SimulationPoint>();

                        if (i != Settings.multiScarfAmount - 1)
                        {
                            for (int k = 0; k < Settings.multiScarfSegments; k++)
                            {
                                scarves[i].Add(scarf[k + (i * Settings.multiScarfSegments)]);
                            }
                        }
                        else
                        {
                            for (int k = 0; k < scarf.Length - (i * Settings.multiScarfSegments); k++) 
                            {
                                scarves[i].Add(scarf[k + (i * Settings.multiScarfSegments)]);
                            }
                        }

                        SimulationPoint[] scarfArr = scarves[i].ToArray();

                        FlutterScarf(scarfArr, frameid, Utility.LengthFast(scarf[i * Settings.multiScarfSegments].Momentum)+(i*5));
                        ProcessScarfBones(RiderConstants.ScarfBones, scarfArr);

                        for (int j = 0; j < scarfArr.Length; j++) { finalScarf.Add(scarfArr[j]); }
                    }

                    scarf = finalScarf.ToArray();
                }
                else
                {
                    scarf[0] = body[RiderConstants.BodyShoulder];
                    FlutterScarf(scarf, frameid, Utility.LengthFast(scarf[0].Momentum));
                    ProcessScarfBones(RiderConstants.ScarfBones, scarf);
                }
            }
            else
            {
                scarf = new SimulationPoint[Scarf.Length];
            }
            return new Rider(body, scarf, phys, dead, sledbroken, UseRemount, rState, rTimer);
        }
        public List<int> Diagnose(
            ISimulationGrid grid,
            Bone[] bones,
            int maxiteration = 6)
        {
            var ret = new List<int>();
            if (Crashed)
                return ret;
            Debug.Assert(maxiteration != 0, "Momentum tick can't die but attempted diagnose");

            SimulationPoint[] body = Body.Step();
            int bodylen = Body.Length;
            bool dead = Crashed;
            int rState = remountState;
            RectLRTB phys = new RectLRTB(ref body[0]);
            List<int> breaks = new List<int>();
            using (grid.Sync.AcquireRead())
            {
                for (int i = 0; i < maxiteration; i++)
                {
                    ProcessBones(bones, body, ref dead, ref rState, breaks);
                    if (dead)
                    {
                        return breaks;
                    }
                    ProcessLines(grid, body, ref phys);
                }
            }
            if (maxiteration == 6)
            {
                var nose = body[RiderConstants.SledTR].Location - body[RiderConstants.SledTL].Location;
                var tail = body[RiderConstants.SledBL].Location - body[RiderConstants.SledTL].Location;
                var head = body[RiderConstants.BodyShoulder].Location - body[RiderConstants.BodyButt].Location;
                if ((nose.X * tail.Y) - (nose.Y * tail.X) < 0) // tail fakie

                {
                    dead = true;
                    ret.Add(-1);
                }
                if ((nose.X * head.Y) - (nose.Y * head.X) > 0)// head fakie
                {
                    dead = true;
                    ret.Add(-2);
                }
            }

            return ret;
        }
        public Line[] GetScarfLines()
        {
            Line[] ret = new Line[Scarf.Length - 1];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new Line(Scarf[i].Location, Scarf[i + 1].Location);
            }
            return ret;
        }
        public override string ToString()
        {
            return "Rider { " + CalculateMomentum().Length + " pixels/frame, " + CalculateCenter().ToString() + " center}";
        }
    }
}