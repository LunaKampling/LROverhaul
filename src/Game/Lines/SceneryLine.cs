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
using OpenTK.Mathematics;

namespace linerider.Game
{
    public class SceneryLine : GameLine
    {
        public override LineType Type => LineType.Scenery;
        public override System.Drawing.Color Color => Settings.Colors.SceneryLine;
        protected SceneryLine()
        {
        }
        public SceneryLine(Vector2d p1, Vector2d p2)
        {
            Position1 = p1;
            Position2 = p2;
        }
        public override string ToString() => "{" +
                "\"type\":2," +
                $"\"x1\":{Position1.X}," +
                $"\"y1\":{Position1.Y}," +
                $"\"x2\":{Position2.X}," +
                $"\"y2\":{Position2.Y}," +
                $"\"width\":{Width}" +
                "}";
        public override GameLine Clone() => new SceneryLine(Position1, Position2)
        {
            ID = ID,
            Width = Width
        };
    }
}