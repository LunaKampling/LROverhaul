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

using linerider.Audio;
using linerider.Game;
using linerider.Utils;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Diagnostics;

namespace linerider
{
    public class Track
    {
        public SimulationGrid Grid = new();
        public LinkedList<int> Lines = new();
        private int LinesMin = -1;
        private int LinesMax = 0;
        public Dictionary<int, GameLine> LineLookup = [];
        public List<GameTrigger> Triggers = [];

        public string Name = Constants.InternalDefaultTrackName;
        public string Filename = null;
        public Song Song;
        private Vector2d _start = Vector2d.Zero;
        public Bone[] Bones = new Bone[RiderConstants.Bones.Length];
        public Vector2d StartOffset
        {
            get => _start;
            set
            {
                _start = value;
                GenerateBones();
            }
        }
        public bool HasDefaultBackground
        {
            get
            {
                Color defaultColor = Settings.Colors.ExportBg;
                return BGColorR == defaultColor.R &&
                    BGColorG == defaultColor.G &&
                    BGColorB == defaultColor.B;
            }
        }
        public bool HasDefaultLineColor
        {
            get
            {
                Color defaultColor = Settings.Colors.ExportLine;
                return LineColorR == defaultColor.R &&
                    LineColorG == defaultColor.G &&
                    LineColorB == defaultColor.B;
            }
        }
        public float StartZoom = Constants.DefaultZoom;
        public int SceneryLines { get; private set; }
        public int BlueLines { get; private set; }
        public int RedLines { get; private set; }
        public bool ZeroStart = false;
        public bool frictionless = false;
        public bool Remount = true;
        public int BGColorR = Settings.Colors.ExportBg.R;
        public int BGColorG = Settings.Colors.ExportBg.G;
        public int BGColorB = Settings.Colors.ExportBg.B;
        public int LineColorR = Settings.Colors.ExportLine.R;
        public int LineColorG = Settings.Colors.ExportLine.G;
        public int LineColorB = Settings.Colors.ExportLine.B;
        public float YGravity = 1; // Default gravity
        public float XGravity = 0; // Default gravity
        public double GravityWellSize = 10; // Default Gravity Well Size

        public Track()
        {
            GenerateBones();
        }
        public GameLine[] GetLines()
        {
            GameLine[] ret = new GameLine[LineLookup.Count];
            int index = ret.Length - 1;
            foreach (int id in Lines)
            {
                ret[index] = LineLookup[id];
                index--;
            }
            return ret;
        }
        public GameLine[] GetSortedLines()
        {
            GameLine[] ret = new GameLine[LineLookup.Count];
            SortedSet<int> temp = [.. Lines];
            int index = 0;
            // Sorted as -2 -1 0 1 2
            foreach (int line in temp)
                ret[index++] = LineLookup[line];

            return ret;
        }
        private void GenerateBones()
        {
            // If the start offset is different the floating point math could
            // result in a slightly different restlength and cause inconsistency.
            ImmutablePointCollection joints = GetStart().Body;
            Bone[] bones = new Bone[RiderConstants.Bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                Bone bone = RiderConstants.Bones[i];
                double rest = (joints[bone.joint1].Location - joints[bone.joint2].Location).Length;
                if (bone.OnlyRepel)
                    rest *= 0.5;
                bones[i] = new Bone(
                    bone.joint1,
                    bone.joint2,
                    rest,
                    bone.Breakable,
                    bone.OnlyRepel);
            }
            Bones = bones;
        }
        public void AddLine(GameLine line)
        {
            if (line.Type == LineType.Scenery)
            {
                if (line.ID == GameLine.UninitializedID)
                {
                    line.ID = Lines.Count > 0 ? LinesMin - 1 : -1;
                }
                if (line.ID < LinesMin)
                {
                    LinesMin = line.ID;
                }
            }
            else
            {
                if (line.ID == GameLine.UninitializedID)
                {
                    line.ID = Lines.Count > 0 ? LinesMax + 1 : 0;
                }
                if (line.ID > LinesMax)
                {
                    LinesMax = line.ID;
                }
            }
            switch (line.Type)
            {
                case LineType.Standard:
                    BlueLines++;
                    break;
                case LineType.Acceleration:
                    RedLines++;
                    break;
                case LineType.Scenery:
                    SceneryLines++;
                    break;
            }
            //Debug.WriteLine("Before Assert: " + line.ID);
            Debug.Assert(
                !LineLookup.ContainsKey(line.ID),
                "Lines occupying the same ID -- really bad");
            LineLookup.Add(line.ID, line);
            // Here is where using a linkedlist shines:
            // we can make the most recent change at the front so if it gets
            // looked up it's easier and faster to find
            _ = Lines.AddFirst(line.ID);

            if (line is StandardLine stl)
                AddLineToGrid(stl);
        }
        public void RemoveLine(GameLine line)
        {
            switch (line.Type)
            {
                case LineType.Standard:
                    BlueLines--;
                    break;
                case LineType.Acceleration:
                    RedLines--;
                    break;
                case LineType.Scenery:
                    SceneryLines--;
                    break;
            }
            _ = LineLookup.Remove(line.ID);
            _ = Lines.Remove(line.ID);
            if (line.ID == LinesMax)
            {
                LinesMax = line.ID - 1;
            }
            if (line.ID == LinesMin)
            {
                LinesMin = line.ID + 1;
            }

            if (line is StandardLine stl)
                RemoveLineFromGrid(stl);
        }
        public void MoveLine(StandardLine line, Vector2d new1, Vector2d new2)
        {
            Vector2d old = line.Position1;
            Vector2d old2 = line.Position2;
            line.Position1 = new1;
            line.Position2 = new2;
            line.CalculateConstants();
            Grid.MoveLine(old, old2, line);
        }
        public int GetVersion() => Grid.GridVersion;

        public bool IsLineCollided(int id) => false;

        /// <summary>
        /// Adds the line to the physics grid.
        /// </summary>
        public void AddLineToGrid(StandardLine line) => Grid.AddLine(line);
        /// <summary>
        /// Removes the line from the physics
        /// </summary>
        public void RemoveLineFromGrid(StandardLine line) => Grid.RemoveLine(line);
        public Rider GetStart() => Rider.Create(StartOffset, new Vector2d(ZeroStart ? 0 : RiderConstants.StartingMomentum, 0), Remount, frictionless);
        public void SetVersion(int version) => Grid.GridVersion = version;
    }
}