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

using linerider.Utils;
using OpenTK;
using OpenTK.Mathematics;
using System;
namespace linerider.Game
{
    public abstract class GameLine : Line
    {
        public const int UninitializedID = int.MinValue;
        public int ID = UninitializedID;
        public float Width = 1;
        public SelectionState SelectionState = SelectionState.None;
        public Layer layer = null;
        public abstract Color Color { get; }
        public abstract LineType Type { get; }
        /// <summary>
        /// "Left" 
        /// </summary>
        public virtual Vector2d Start => Position1;
        /// <summary>
        /// "Right"
        /// </summary>
        public virtual Vector2d End => Position2;

        public override int GetHashCode() => ID;

        public Color GetColor()
        {
            switch (Type)
            {
                case LineType.Standard:
                    return Settings.Colors.StandardLine;
                case LineType.Acceleration:
                    return Settings.Colors.AccelerationLine;
                case LineType.Scenery:
                    return Settings.Colors.SceneryLine;
                default:
                    throw new Exception("Unable to get the color for this line, its type is unknown");
            }
        }
        public abstract GameLine Clone();
    }
}