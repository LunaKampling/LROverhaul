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
using System.Collections.Concurrent;
namespace linerider.Game
{
    /// <summary>
    /// An overlay around an existing simulation grid.
    /// This grid's cells are checked first before the underlying grid.
    /// </summary>
    public class SimulationGridOverlay : ISimulationGrid
    {
        public ResourceSync Sync => BaseGrid.Sync;
        public SimulationGrid BaseGrid;
        public ConcurrentDictionary<GridPoint, SimulationCell> Overlay = new ConcurrentDictionary<GridPoint, SimulationCell>();
        public SimulationCell GetCell(int x, int y)
        {
            GridPoint gp = new GridPoint(x, y);
            return !Overlay.TryGetValue(gp, out SimulationCell cell) ? BaseGrid.GetCell(x, y) : cell;
        }
        /// <summary>
        /// Adds an overlay if it does not already exist for that point
        /// </summary>
        /// <returns>true if the overlay was added</returns>
        public bool AddOverlay(GridPoint point, SimulationCell cell)
        {
            if (!Overlay.ContainsKey(point))
            {
                Overlay[point] = cell;
                return true;
            }
            return false;
        }
        public void Clear() => Overlay.Clear();
    }
}