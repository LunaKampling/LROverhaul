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
using linerider.IO;
using linerider.Utils;
using System;
using System.Collections.Generic;
namespace linerider
{
    public class TrackReader : GameService, IDisposable
    {
        protected ResourceSync.ResourceLock _sync;
        protected Track _track;
        private Track Track => _disposed ? throw new ObjectDisposedException("TrackWriter") : _track;
        /// <summary>
        /// Returns the read-only track name.
        /// </summary>
        public virtual string Name
        {
            get => Track.Name;
            set => throw new NotSupportedException("Track reader cannot set Name");
        }
        /// <summary>
        /// The loaded track filename, if any
        /// </summary>
        public string Filename => Track.Filename;
        protected EditorGrid _editorcells;
        private bool _disposed = false;
        protected TrackReader(ResourceSync.ResourceLock sync, Track track)
        {
            _track = track;
            _sync = sync;
        }
        public static TrackReader AcquireRead(ResourceSync sync, Track track, EditorGrid cells) => new TrackReader(sync.AcquireRead(), track) { _editorcells = cells };

        public GameLine GetNewestLine() => Track.Lines.Count == 0 ? null : Track.LineLookup[Track.Lines.First.Value];

        public GameLine GetOldestLine() => Track.Lines.Count == 0 ? null : Track.LineLookup[Track.Lines.Last.Value];
        public IEnumerable<GameLine> GetLinesInRect(DoubleRect rect, bool precise)
        {
            EditorCell ret = _editorcells.LinesInRect(rect);
            if (precise)
            {
                List<GameLine> newret = new List<GameLine>(ret.Count);
                foreach (GameLine line in ret)
                {
                    if (Line.DoesLineIntersectRect(
                        line,
                        new DoubleRect(
                            rect.Left,
                            rect.Top,
                            rect.Width,
                            rect.Height))
                            )
                    {
                        newret.Add(line);
                    }
                }
                return newret;
            }
            return ret;
        }

        /// <summary>
        /// Ticks the rider in the simulation
        /// </summary>
        public Rider TickBasic(Rider state, int maxiteration = 6) => state.Simulate(_track.Grid, _track.Bones, null, maxiteration);
        public string SaveTrackTrk(string savename) => TRKWriter.SaveTrack(_track, savename);
        public Dictionary<string, bool> GetFeatures() => TrackIO.GetTrackFeatures(Track);
        public void Dispose()
        {
            if (!_disposed)
            {
                _sync.Dispose();
                _track = null;
                _disposed = true;
            }
        }
    }
}
