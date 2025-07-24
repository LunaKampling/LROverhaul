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
using linerider.Game;
using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace linerider
{

    public partial class SimulationGrid
    {

        private static bool CheckBounds(Rectangle r, int x, int y) => x >= r.Left && x <= r.Right && y >= r.Top && y <= r.Bottom;

        public static GridPoint GetGridPoint(double posx, double posy)
        {
            int x = (int)Math.Floor(posx / CellSize);
            int y = (int)Math.Floor(posy / CellSize);
            return new GridPoint(x, y);
        }
        public static CellLocation CellInfo(double posx, double posy)
        {
            GridPoint gp = GetGridPoint(posx, posy);
            return new CellLocation(gp, new Vector2d(posx - CellSize * gp.X, posy - CellSize * gp.Y));
        }
        public static List<CellLocation> GetGridPositions(StandardLine line, int gridversion) => GetGridPositions(line.Position1, line.Position2, gridversion);
        public static List<CellLocation> GetGridPositions(Vector2d linestart, Vector2d lineend, int gridversion)
        {
            Vector2d diff = lineend - linestart;
            List<CellLocation> ret = new List<CellLocation>();
            CellLocation cell = CellInfo(linestart.X, linestart.Y);
            CellLocation gridend = CellInfo(lineend.X, lineend.Y);

            ret.Add(cell);
            if (cell.X == gridend.X && cell.Y == gridend.Y)
                return ret;

            int p1X = Math.Min(cell.X, gridend.X),
                p2X = Math.Max(cell.X, gridend.X),
                p1Y = Math.Min(cell.Y, gridend.Y),
                p2Y = Math.Max(cell.Y, gridend.Y);
            Rectangle box = Rectangle.FromLTRB(p1X, p1Y, p2X, p2Y);
            Vector2d current = linestart;
            Vector2d scalar = new Vector2d(1 / diff.Y, 1 / diff.X);
            bool xforwards = diff.X > 0;
            bool yforwards = diff.Y > 0;
            if (gridversion == 62)
            {
                while (true)
                {
                    double boundaryx = GetStep(yforwards, cell.Y, cell.Remainder.Y);
                    double boundaryy = GetStep(xforwards, cell.X, cell.Remainder.X);

                    Vector2d step = new Vector2d(
                        diff.X * boundaryx,
                        diff.Y * boundaryy) * scalar;

                    current.X += (Math.Abs(step.X) < Math.Abs(boundaryy)) ? step.X : boundaryy;
                    current.Y += (Math.Abs(step.Y) < Math.Abs(boundaryx)) ? step.Y : boundaryx;
                    cell = CellInfo(current.X, current.Y);
                    if (!CheckBounds(box, cell.X, cell.Y))
                        return ret;
                    ret.Add(cell);
                }
            }
            else if (gridversion == 61) // eh
            {
                ret = GetGridPositions61(linestart, lineend);
            }
            else if (gridversion == 60)
            {
                Vector2d line_normal_unit = diff.Normalized().PerpendicularLeft; // Does not depend on flipped
                Vector2d line_halfway = 0.5 * new Vector2d(Math.Abs(diff.X), Math.Abs(diff.Y));
                Vector2d line_midpoint = linestart + diff * 0.5;
                Vector2d absolute_normal = new Vector2d(Math.Abs(line_normal_unit.X), Math.Abs(line_normal_unit.Y));

                for (int cell_x = p1X; cell_x <= p2X; cell_x++)
                {
                    for (int cell_y = p1Y; cell_y <= p2Y; cell_y++)
                    {
                        Vector2d curr_pos = CellSize * new Vector2d(cell_x + 0.5, cell_y + 0.5);
                        CellLocation next_cell_pos = CellInfo(curr_pos.X, curr_pos.Y);
                        Vector2d dist_between_centers = line_midpoint - curr_pos;
                        double dist_from_cell_center = Vector2d.Dot(absolute_normal, next_cell_pos.Remainder);
                        double cell_overlap_into_hitbox = Vector2d.Dot(new Vector2d(dist_from_cell_center, dist_from_cell_center), absolute_normal);
                        double norm_dist_between_centers = Vector2d.Dot(line_normal_unit, dist_between_centers);
                        double dist_from_line = Math.Abs(norm_dist_between_centers * line_normal_unit.X) + Math.Abs(norm_dist_between_centers * line_normal_unit.Y);
                        if (line_halfway.X + next_cell_pos.Remainder.X >= Math.Abs(dist_between_centers.X)
                            && line_halfway.Y + next_cell_pos.Remainder.Y >= Math.Abs(dist_between_centers.Y)
                            && cell_overlap_into_hitbox >= dist_from_line)
                        {
                            ret.Add(next_cell_pos);
                        }
                    }
                }
            }
            return ret;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetStep(bool forwards, double cellpos, double remainder) => forwards
                ? cellpos < 0 ?
                CellSize + remainder :
                CellSize - remainder
                : cellpos < 0 ?
                -CellSize - remainder :
                -(remainder + 1);
        private static List<CellLocation> GetGridPositions61(Vector2d start, Vector2d end)
        {
            Vector2d diff = end - start;
            List<CellLocation> ret = new List<CellLocation>();
            CellLocation cell = CellInfo(start.X, start.Y);
            CellLocation gridend = CellInfo(end.X, end.Y);

            ret.Add(cell);
            if ((diff.X == 0 && diff.Y == 0) || (cell.X == gridend.X && cell.Y == gridend.Y))
                return ret;

            int p1X = Math.Min(cell.X, gridend.X),
                p2X = Math.Max(cell.X, gridend.X),
                p1Y = Math.Min(cell.Y, gridend.Y),
                p2Y = Math.Max(cell.Y, gridend.Y);
            Rectangle box = Rectangle.FromLTRB(p1X, p1Y, p2X, p2Y);
            Vector2d current = start;
            double slope = 0;
            double _loc13 = 0;
            double isbelowactualY = 0;
            if (diff.X != 0 && diff.Y != 0)
            {
                slope = diff.Y / diff.X;
                _loc13 = 1.0 / slope;
                isbelowactualY = start.Y - slope * start.X;
            }
            while (true)
            {
                double difY, difX;
                difX = -cell.Remainder.X + (diff.X > 0 ? CellSize : -1);
                difY = -cell.Remainder.Y + (diff.Y > 0 ? CellSize : -1);
                if (diff.X == 0)
                {
                    current.Y += difY;
                }
                else if (diff.Y == 0)
                {
                    current.X += difX;
                }
                else
                {
                    double whyyy = Math.Round(slope * (current.X + difX) + isbelowactualY);
                    if (Math.Abs(whyyy - current.Y) < Math.Abs(difY))
                    {
                        current.X += difX;
                        current.Y = whyyy;
                    }
                    else if (Math.Abs(whyyy - current.Y) == Math.Abs(difY))
                    {
                        current.X += difX;
                        current.Y += difY;
                    }
                    else
                    {
                        current.X = Math.Round((current.Y + difY - isbelowactualY) * _loc13);
                        current.Y += difY;
                    }
                }
                cell = CellInfo(current.X, current.Y);
                if (CheckBounds(box, cell.X, cell.Y))
                {
                    ret.Add(cell);
                    continue;
                }
                return ret;
            }
        }
    }
}
