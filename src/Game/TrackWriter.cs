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
using linerider.Rendering;
using linerider.Utils;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace linerider
{
    /// <summary>
    /// A class that wraps the complication of modifying the track with thread
    /// safety and guarantees. Always wrap undoable actions with
    /// UndoManager.BeginAction and UndoManager.EndAction
    /// </summary>
    public class TrackWriter : TrackReader
    {
        private readonly bool _disposed = false;
        private bool _updateextensions = true;
        private UndoManager _undo;
        private Timeline _timeline;
        private SimulationRenderer _renderer;
        /// <summary>
        /// Gets or sets the track name
        /// </summary>
        public override string Name
        {
            get => Track.Name;
            set => Track.Name = value;
        }
        public Track Track => _disposed ? throw new ObjectDisposedException("TrackWriter") : _track;
        public EditorGrid Cells => _disposed ? throw new ObjectDisposedException("TrackWriter") : _editorcells;
        public List<GameTrigger> Triggers
        {
            get => _disposed ? throw new ObjectDisposedException("TrackWriter") : _track.Triggers;
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("TrackWriter");
                _track.Triggers = value;
            }
        }
        protected TrackWriter(ResourceSync.ResourceLock sync, Track track)
        : base(sync, track)
        {
            _track = track;
            _sync = sync;
        }
        public static TrackWriter AcquireWrite(
            ResourceSync sync,
            Track track,
            SimulationRenderer renderer,
            UndoManager undo,
            Timeline timeline,
            EditorGrid cells) => new(sync.AcquireWrite(), track)
            {
                _undo = undo,
                _renderer = renderer,
                _timeline = timeline,
                _editorcells = cells
            };
        public void NotifyTrackChanged() => game.Track.NotifyTrackChanged();
        /// <summary>
        /// Disables saving changes to the undo buffer.
        /// </summary>
        public void DisableUndo() => _undo = null;
        /// <summary>
        /// A function in place so the undo manager can restore states without
        /// fooling with the original extensions.
        /// </summary>
        public void DisableExtensionUpdating() => _updateextensions = false;
        /// <summary>
        /// Adds the line to the track, grid, and renderer. 
        /// Updates extensions
        /// Notifies the undo/buffer managers
        /// </summary>
        public void AddLine(GameLine line)
        {
            if (line is StandardLine)
                SaveCells(line.Position1, line.Position2);

            Track.AddLine(line);
            _editorcells.AddLine(line);
            _renderer.AddLine(line);
            RegisterUndoAction(null, line);
            if (_updateextensions && line is StandardLine stl)
                AddExtensions(stl);
        }
        /// <summary>
        /// Moves the line in the track, grid, and renderer. 
        /// Updates extensions
        /// Notifies the undo/timeline managers
        /// </summary>
        public void MoveLine(
            GameLine line,
            Vector2d pos1,
            Vector2d pos2,
            bool render = true)
        {
            if (line.Position1 != pos1 || line.Position2 != pos2)
            {
                GameLine clone = line.Clone();
                if (line is StandardLine std)
                {
                    SaveCells(line.Position1, line.Position2);
                    SaveCells(pos1, pos2);
                    if (_updateextensions)
                        RemoveExtensions(std);
                    Track.MoveLine(std, pos1, pos2);
                    if (_updateextensions)
                        AddExtensions(std);
                }
                else
                {
                    line.Position1 = pos1;
                    line.Position2 = pos2;
                }
                _editorcells.RemoveLine(clone);
                _editorcells.AddLine(line);

                RegisterUndoAction(clone, line);
                if (render)
                    _renderer.RedrawLine(line);
            }
        }
        /// <summary>
        /// Replaces the line in the track, grid, and renderer. 
        /// Updates extensions
        /// Notifies the undo/timeline managers
        /// </summary>
        public void ReplaceLine(GameLine oldline, GameLine newline)
        {
            if (oldline.ID != newline.ID)
                throw new Exception("can only replace lines with the same id");
            RegisterUndoAction(oldline, newline);

            if (oldline is StandardLine std)
            {
                SaveCells(oldline.Position1, oldline.Position2);
                SaveCells(newline.Position1, newline.Position2);
                if (_updateextensions)
                    RemoveExtensions(std);
                StandardLine newstd = (StandardLine)newline;
                using (Track.Grid.Sync.AcquireWrite())
                {
                    // This could be a moveline, i think.
                    Track.Grid.RemoveLine(std);
                    Track.Grid.AddLine(newstd);
                }
                if (_updateextensions)
                    AddExtensions(newstd);
            }

            _editorcells.RemoveLine(oldline);
            _editorcells.AddLine(newline);

            Track.LineLookup[newline.ID] = newline;
            if (oldline.Type != newline.Type)
            {
                _renderer.RemoveLine(oldline);
                _renderer.AddLine(newline);
            }
            else
            {
                _renderer.RedrawLine(newline);
            }
        }

        /// <summary>
        /// Removes the line from the track, grid, and renderer.
        /// Updates extensions
        /// Notifies the undo/timeline managers
        /// </summary>
        public void RemoveLine(GameLine line)
        {
            if (line is StandardLine std)
            {
                SaveCells(line.Position1, line.Position2);
                if (_updateextensions)
                    RemoveExtensions(std);
            }
            RegisterUndoAction(line, null);
            Track.RemoveLine(line);
            _editorcells.RemoveLine(line);
            _renderer.RemoveLine(line);
        }
        private void AddExtensions(StandardLine input) => UpdateExtensions(input, true);
        private void RemoveExtensions(StandardLine input) => UpdateExtensions(input, false);
        private SimulationCell GetPairs(StandardLine input)
        {
            SimulationCell list = new();
            EditorCell c1 = _editorcells.GetCellFromPoint(input.Position1);
            EditorCell c2 = _editorcells.GetCellFromPoint(input.Position2);
            LineContainer<GameLine>[] cells =
            [
                c1,
                c1 == c2 ? null : c2
            ];
            foreach (LineContainer<GameLine> cell in cells)
            {
                if (cell == null)
                    continue;
                foreach (GameLine line in cell)
                {
                    if (line.Type == LineType.Scenery)
                        continue;
                    if (line.ID != input.ID)
                    {
                        if (
                            line.End == input.Start ||
                            line.Start == input.End)
                        {
                            list.AddLine((StandardLine)line);
                        }
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// Checks every line with the input line's ends if its extensions
        /// pair with input and adds or removes them
        /// </summary>
        /// <param name="input">the input line</param>
        /// <param name="add">should extensions be added or removed</param>
        private void UpdateExtensions(StandardLine input, bool add)
        {
            // TODO: this method could be faster. Its now called on every moveline
            // etc
            Debug.Assert(
                input != null,
                "passed null line to update extensions");

            SimulationCell list = GetPairs(input);
            double inputangle = Angle.FromVector(input.End - input.Start).Degrees;
            GameLine inputclone = input.Clone();
            bool changemade = false;
            foreach (StandardLine connected in list)
            {
                double angle2 = Angle.FromVector(
                    connected.End - connected.Start)
                    .Degrees;
                bool startlink = input.Start == connected.End;
                double diff = startlink
                ? new Angle(angle2 - inputangle).Degrees
                : new Angle(inputangle - angle2).Degrees;
                if (diff > 0 && diff <= 180)
                {
                    GameLine clone = connected.Clone();
                    if (!changemade)
                    {
                        SaveCells(input.Position1, input.Position1);
                        changemade = true;
                    }
                    SaveCells(connected.Position1, connected.Position1);

                    StandardLine.Ext inputflag = (startlink ^ input.inv)
                        ? StandardLine.Ext.Left
                        : StandardLine.Ext.Right;
                    StandardLine.Ext connectedflag = (startlink ^ connected.inv)
                        ? StandardLine.Ext.Right
                        : StandardLine.Ext.Left;
                    if (add)
                    {
                        input.Extension |= inputflag;
                        connected.Extension |= connectedflag;
                    }
                    else
                    {
                        input.Extension &= ~inputflag;
                        connected.Extension &= ~connectedflag;
                    }
                    RegisterUndoAction(clone, connected);
                }
            }
            // We do this outside of loop in case we lose both extensions!
            if (changemade)
                RegisterUndoAction(inputclone, input);
        }
        /// <summary>
        /// State a change to the undo manager
        /// always needs to be in PAIRS with a before and after
        /// </summary>
        private void RegisterUndoAction(GameLine before, GameLine after) => _undo?.AddChange(before, after);
        /// <summary>
        /// State a change to the timeline manager
        /// call this before making the change, as the timeline manager
        /// needs it for thread safety
        /// </summary>
        /// <param name="linestart">line.Position</param>
        /// <param name="lineend">line.Position2</param>
        private void SaveCells(Vector2d linestart, Vector2d lineend) => _timeline.SaveCells(linestart, lineend);
    }
}