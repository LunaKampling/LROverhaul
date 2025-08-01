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

using System;

namespace linerider.Game
{
    public struct ImmutablePointCollection(SimulationPoint[] points)
    {
        public SimulationPoint this[int index] => _points[index];
        public int Length => _points.Length;
        private readonly SimulationPoint[] _points = points;

        /// <summary>
        /// Fast compare method for two collections so we can bypass
        /// struct copies
        /// </summary>
        public bool CompareTo(ImmutablePointCollection comparand)
        {
            int len = _points.Length;
            if (comparand.Length != len)
                throw new ArgumentException("Mismatched point collections");
            for (int i = 0; i < len; i++)
            {
                if (!SimulationPoint.FastEquals(
                    ref _points[i],
                    ref comparand._points[i]))
                    return false;
            }
            return true;
        }
        public SimulationPoint[] Step(bool friction = false, bool gravity = true)
        {
            int len = _points.Length;
            SimulationPoint[] ret = new SimulationPoint[len];
            for (int i = 0; i < len; i++)
            {
                ret[i] = !friction ? _points[i].Step(gravity) : _points[i].StepFriction();
            }
            return ret;
        }
    }
}