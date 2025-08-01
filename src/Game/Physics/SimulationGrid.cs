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
using linerider.Utils;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace linerider
{
    public partial class SimulationGrid : ISimulationGrid
    {
        public const int CellSize = 14;
        public int GridVersion = 62;
        public ResourceSync Sync { get; } = new ResourceSync();
        private readonly Dictionary<int, SimulationCell> Cells = new(4096);

        public List<CellLocation> GetGridPositions(StandardLine line) => GetGridPositions(line, GridVersion);
        public void AddLine(StandardLine line)
        {
            List<CellLocation> positions = GetGridPositions(line);
            using (Sync.AcquireWrite())
            {
                foreach (CellLocation pos in positions)
                {
                    Register(line, pos.X, pos.Y);
                }
            }
        }
        public void RemoveLine(StandardLine line)
        {
            List<CellLocation> positions = GetGridPositions(line);
            using (Sync.AcquireWrite())
            {
                foreach (CellLocation pos in positions)
                {
                    Unregister(line, pos.X, pos.Y);
                }
            }
        }
        /// <summary>
        /// Removes the line ID in all old grid cells and adds it back to new
        /// ones
        /// </summary>
        public void MoveLine(Vector2d old1, Vector2d old2, StandardLine line)
        {
            List<CellLocation> oldpos = GetGridPositions(old1, old2, GridVersion);
            List<CellLocation> newpos = GetGridPositions(line);
            using (Sync.AcquireWrite())
            {
                foreach (CellLocation v in oldpos)
                {
                    Unregister(line, v.X, v.Y);
                }
                foreach (CellLocation v in newpos)
                {
                    Register(line, v.X, v.Y);
                }
            }
        }
        public virtual SimulationCell GetCell(int x, int y)
        {
            int pos = GetCellKey(x, y);
            return !Cells.TryGetValue(pos, out SimulationCell cell) ? null : cell;
        }

        public SimulationCell PointToChunk(Vector2d pos) => GetCell((int)Math.Floor(pos.X / CellSize), (int)Math.Floor(pos.Y / CellSize));

        protected int GetCellKey(int x, int y)
        {
            unchecked
            {
                int hash = 27;
                hash = hash * 486187739 + x;
                hash = hash * 486187739 + y;
                return hash;
            }
        }
        private void Register(StandardLine l, int x, int y)
        {
            int key = GetCellKey(x, y);
            if (!Cells.TryGetValue(key, out SimulationCell cell))
            {
                cell = new SimulationCell();
                Cells[key] = cell;
            }
            cell.AddLine(l);
        }

        private void Unregister(StandardLine l, int x, int y)
        {
            int pos = GetCellKey(x, y);
            if (!Cells.TryGetValue(pos, out SimulationCell cell))
                return;
            cell.RemoveLine(l.ID);
        }
    }
}
