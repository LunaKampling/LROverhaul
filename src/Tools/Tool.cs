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
using linerider.UI;
using linerider.Utils;
using OpenTK;
using OpenTK.Windowing.Common.Input;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Key = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace linerider.Tools
{
    public abstract class Tool : GameService
    {
        private static readonly Swatch Default = new Swatch();
        public virtual Bitmap Icon => new Bitmap(1, 1);
        public virtual Hotkey Hotkey => Hotkey.None;
        public virtual string Name => "";
        public virtual Swatch Swatch => Default;
        public virtual bool ShowSwatch => false;
        protected virtual double SnapRadius => 2 / game.Track.Zoom;
        protected virtual bool EnableSnap
        {
            get
            {
                bool toggle = InputUtils.CheckPressed(Hotkey.ToolToggleSnap);
                return Settings.Editor.SnapNewLines != toggle;
            }
        }
        /// <summary>
        /// Returns the current tooltip that should be displayed at the
        /// mouse cursor
        /// </summary>
        public virtual string Tooltip => "";
        public virtual bool Active { get; protected set; }
        public virtual bool NeedsRender => false;
        public abstract MouseCursor Cursor { get; }
        public bool IsMouseButtonDown => IsLeftMouseDown || IsRightMouseDown;
        public bool IsLeftMouseDown = false;
        public bool IsRightMouseDown = false;
        /// <summary>
        /// Determines whether to receive mouse movement events when they happen
        /// or only the last one before frame update.
        /// Leaving this false can be very good for performance.
        /// </summary>
        public virtual bool RequestsMousePrecision => false;

        public Tool()
        {
        }

        protected Vector2d ScreenToGameCoords(Vector2d mouse) => game.ScreenPosition + mouse / game.Track.Zoom;
        public virtual void OnUndoRedo(bool isundo, object undohint)
        {
        }
        public virtual void OnMouseMoved(Vector2d pos)
        {
        }

        public virtual void OnMouseDown(Vector2d pos) => IsLeftMouseDown = true;

        public virtual void OnMouseRightDown(Vector2d pos) => IsRightMouseDown = true;

        public virtual void OnMouseUp(Vector2d pos) => IsLeftMouseDown = false;

        public virtual void OnMouseRightUp(Vector2d pos) => IsRightMouseDown = false;

        public virtual bool OnKeyDown(Key k) => false;

        public virtual bool OnKeyUp(Key k) => false;

        public virtual void Render()
        {
        }

        public virtual void Stop()
        {
        }

        /// <summary>
        /// Cancels the current action, but not necessarily by deleting state
        /// </summary>
        public virtual void Cancel()
        {
        }

        public virtual void OnChangingTool()
        {
        }
        protected bool IsLineSnappedByKnob(TrackReader trk, Vector2d point, GameLine line, out bool knob1)
        {
            double rad = SnapRadius;
            Vector2d closer = Utility.CloserPoint(point, line.Position1, line.Position2);
            if ((point - closer).Length - line.Width < rad)
            {
                knob1 = closer == line.Position1;
                return true;
            }

            knob1 = false;
            return false;
        }
        protected GameLine SelectLine(TrackReader trk, Vector2d position, out bool knob)
        {
            knob = false;

            double relativeKnobSize = Settings.LimitLineKnobsSize ? Math.Min(
                Constants.KnobSize,
                Constants.KnobSize * Settings.Computed.UIScale * Constants.MaxLimitedKnobSize / game.Track.Zoom
            ) : Constants.KnobSize;
            GameLine[] ends = LineEndsInRadius(trk, position, (float)relativeKnobSize - 1);

            if (ends.Length > 0)
            {
                knob = true;
                return ends[0];
            }

            return SelectLines(trk, position).FirstOrDefault();
        }
        protected IEnumerable<GameLine> SelectLines(TrackReader trk, Vector2d position)
        {
            GameLine[] ends = LineEndsInRadius(trk, position, SnapRadius);
            foreach (GameLine line in ends)
                yield return line;
            _ = game.Track.Zoom;
            IEnumerable<GameLine> lines =
                trk.GetLinesInRect(
                    new DoubleRect(position - new Vector2d(24, 24),
                    new Vector2d(24 * 2, 24 * 2)),
                    false);
            foreach (GameLine line in lines)
            {
                if (ends.Contains(line))
                    continue;
                double lnradius = line.Width;
                Angle angle = Angle.FromLine(line.Position1, line.Position2);
                Vector2d[] rect = Utility.GetThickLine(
                    line.Position1,
                    line.Position2,
                    angle,
                    lnradius * 2);
                if (Utility.PointInRectangle(rect, position))
                {
                    yield return line;
                }
            }
            yield break;
        }
        protected GameLine CreateLine(
            TrackWriter trk,
            Vector2d start,
            Vector2d end,
            bool inv,
            bool snapstart,
            bool snapend,
            LineType type,
            double multiplier = 1,
            float width = 1f)
        {
            GameLine added;
            switch (type)
            {
                case LineType.Standard:
                    added = new StandardLine(start, end, inv);
                    break;

                case LineType.Acceleration:
                    RedLine red = new RedLine(start, end, inv)
                    { Multiplier = multiplier };
                    red.CalculateConstants(); // Multiplier needs to be recalculated
                    added = red;
                    break;

                case LineType.Scenery:
                    added = new SceneryLine(start, end)
                    { Width = width };
                    break;

                default: // In case no swatch is chosen select blue and make a blue line
                    added = new StandardLine(start, end, inv);
                    Swatch.Selected = LineType.Standard;
                    type = LineType.Standard;
                    break;
            }
            trk.AddLine(added);
            if (type != LineType.Scenery)
            {
                if (snapstart)
                    SnapLineEnd(trk, added, added.Position1);
                if (snapend)
                    SnapLineEnd(trk, added, added.Position2);
            }
            game.Track.Invalidate();
            return added;
        }
        /// <summary>
        /// Gets lines near the point by radius.
        /// does not support large distances as it only gets a small number of grid cells
        /// </summary>
        /// <returns>a sorted array of lines where 0 is the closest point</returns>
        public GameLine[] LinesInRadius(TrackWriter trk, Vector2d position, double rad)
        {
            SortedList<int, GameLine> lines = new SortedList<int, GameLine>();
            IEnumerable<GameLine> inrect =
                trk.GetLinesInRect(new DoubleRect(position - new Vector2d(24, 24), new Vector2d(24 * 2, 24 * 2)),
                    false);
            Vector2d[] circle = Rendering.StaticRenderer.GenerateCircle(position.X, position.Y, rad, 8);

            GameLine[] ends = LineEndsInRadius(trk, position, rad);
            foreach (GameLine line in ends)
            {
                lines[line.ID] = line;
            }
            foreach (GameLine line in inrect)
            {
                if (lines.ContainsKey(line.ID))
                    continue;
                Angle angle = Angle.FromLine(line.Position1, line.Position2);
                Vector2d[] rect = Utility.GetThickLine(
                    line.Position1,
                    line.Position2,
                    angle,
                    line.Width * 2);
                if (Utility.PointInRectangle(rect, position))
                {
                    lines.Add(line.ID, line);
                    continue;
                }
                else
                {
                    for (int i = 0; i < circle.Length; i++)
                    {
                        if (Utility.PointInRectangle(rect, circle[i]))
                        {
                            lines.Add(line.ID, line);
                            break;
                        }
                    }
                }
            }
            GameLine[] ret = new GameLine[lines.Count];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = lines.Values[lines.Count - 1 - i];
            }
            return lines.Values.ToArray();
        }
        /// <summary>
        /// Gets line ends near the point by radius.
        /// does not support large distances as it only gets a small number of grid cells
        /// </summary>
        /// <returns>a sorted array of lines where 0 is the closest point within the radius</returns>
        protected GameLine[] LineEndsInRadius(TrackReader trk, Vector2d point, double rad)
        {
            IEnumerable<GameLine> lines =
                trk.GetLinesInRect(new DoubleRect(point - new Vector2d(24, 24), new Vector2d(24 * 2, 24 * 2)),
                    false);
            SortedList<double, List<GameLine>> ret = new SortedList<double, List<GameLine>>();
            foreach (GameLine line in lines)
            {
                double p1 = (point - line.Position1).Length;
                double p2 = (point - line.Position2).Length;
                double closer = Math.Min(p1, p2);
                if (closer - line.Width < rad)
                {
                    if (ret.ContainsKey(closer))
                    {
                        ret[closer].Add(line);
                    }
                    else
                    {
                        List<GameLine> l = new List<GameLine>
                        {
                            line
                        };
                        ret[closer] = l;
                    }
                }
            }
            List<GameLine> retn = new List<GameLine>();
            for (int i = 0; i < ret.Values.Count; i++)
            {
                retn.AddRange(ret.Values[i]);
            }
            return retn.ToArray();
        }
        protected bool LifeLock(TrackReader track, Timeline timeline, StandardLine line)
        {
            int offset = game.Track.Offset;
            int iteration = game.Track.IterationsOffset;
            if (offset == 0)
                return false;
            Rider frame = timeline.GetFrame(offset, iteration);
            if (!frame.Crashed)
            {
                List<int> diagnosis = null;
                if (Settings.Editor.LifeLockNoFakie)
                {
                    diagnosis = timeline.DiagnoseFrame(offset, iteration);
                    foreach (int v in diagnosis)
                    {
                        // The next frame dies on something that isnt a fakie, so we cant stop here
                        if (v < 0)
                            return false;
                    }
                }
                if (Settings.Editor.LifeLockNoOrange)
                {
                    if (diagnosis == null)
                        diagnosis = timeline.DiagnoseFrame(offset, iteration);
                    foreach (int v in diagnosis)
                    {
                        // The next frame dies on something that isnt a fakie, so we cant stop here
                        if (v >= 0)
                            return false;
                    }
                }
                if (timeline.IsLineHit(line.ID, game.Track.Offset))
                    return true;
            }
            return false;
        }

        protected GameLine FixNoFakie2(Timeline timeline, GameLine line, Vector2d movejoint)
        {
            const double shiftDist = 1.0e1;
            const int attemptCount = 500;
            int offset = game.Track.Offset;
            Rider lastFrame = timeline.GetFrame(offset - 1);
            bool usingjoint1 = line.Position1 == movejoint;
            Random rnd = new Random();
            _ = rnd.NextDouble(); // Call this once to make sure the RNG is fully shuffled (just in case initial seeds correlate a bit)

            if (!lastFrame.Crashed)
            {
                Vector2d jointPos = movejoint; // The position of the joint which will be moved
                GameLine currentLine = line;
                GameLine newLine = currentLine; // The new line position once the modifier has been applied

                for (int i = 0; i < attemptCount; i++)
                {
                    jointPos.X = movejoint.X + shiftDist * rnd.NextDouble();
                    jointPos.Y = movejoint.Y + shiftDist * rnd.NextDouble();

                    if (usingjoint1)
                        newLine.Position1 = jointPos;
                    else
                        newLine.Position2 = jointPos;

                    using (TrackWriter trk = game.Track.CreateTrackWriter())
                    {
                        trk.DisableUndo();
                        trk.MoveLine(currentLine, newLine.Position1, newLine.Position2, false);
                        currentLine = newLine;
                    }

                    Rider thisFrame = lastFrame.Simulate(game.Track.GetTrack());

                    if (!thisFrame.SledBroken)
                    {
                        using (TrackWriter trk = game.Track.CreateTrackWriter())
                            trk.MoveLine(currentLine, line.Position1, line.Position2, false);
                        return newLine;
                    }
                }

                using (TrackWriter trk = game.Track.CreateTrackWriter())
                    trk.MoveLine(currentLine, line.Position1, line.Position2, false);
            }
            return line;
        }

        protected GameLine FixNoFakie(Timeline timeline, GameLine line, Vector2d movejoint)
        {
            const double shiftDist = 1.0e1;
            const int attemptCount = 500;
            int offset = game.Track.Offset;
            Rider lastFrame = timeline.GetFrame(offset - 1);
            bool usingjoint1 = line.Position1 == movejoint;
            //double lastPos = 0.0;
            if (!lastFrame.Crashed)
            {
                Vector2d jointPos = movejoint;
                GameLine newLine = line;
                GameLine currentLine = line;
                Random rnd = new Random();
                for (int i = 0; i < attemptCount; i++)
                {
                    GameLine oldLine = newLine;
                    jointPos.X = movejoint.X + shiftDist * rnd.NextDouble();
                    jointPos.Y = movejoint.Y + shiftDist * rnd.NextDouble();
                    if (usingjoint1)
                        newLine.Position1 = jointPos;
                    else
                        newLine.Position2 = jointPos;

                    using (TrackWriter trk = game.Track.CreateTrackWriter())
                    {
                        trk.DisableUndo();
                        Console.WriteLine("MOVING LINE " + oldLine.Position1.ToString() + " TO " + newLine.Position1.ToString());
                        trk.MoveLine(currentLine, newLine.Position1, newLine.Position2, false);
                        currentLine = newLine;
                    }

                    Rider thisFrame = lastFrame.Simulate(game.Track.GetTrack());

                    //if (lastPos != 0.0 && lastPos == thisFrame.Body[0].Location.Y)
                    //{
                    //    Console.WriteLine("NO CHANGE");
                    //    Console.WriteLine(jointPos.Y.ToString()+","+);
                    //}
                    //else if (lastPos != 0.0)
                    //{
                    //    Console.WriteLine("SOME CHANGE");
                    //}

                    //lastPos = thisFrame.Body[0].Location.Y;
                    if (!thisFrame.SledBroken)
                    {
                        if (i > 500)
                            Console.WriteLine("SLED SURVIVED! " + i.ToString());
                        using (TrackWriter trk = game.Track.CreateTrackWriter())
                            trk.MoveLine(currentLine, line.Position1, line.Position2, false);
                        return newLine;
                    }
                    if (usingjoint1)
                        line.Position1 = movejoint;
                    else
                        line.Position2 = movejoint;
                    Console.WriteLine("ABOUT TO DIE?");
                    using (TrackWriter trk = game.Track.CreateTrackWriter())
                        trk.MoveLine(currentLine, line.Position1, line.Position2, false);
                    currentLine = line;
                    Console.WriteLine("DEAD?");
                }
            }
            //Console.WriteLine("FAILED!");
            return line;
        }
        protected Vector2d TrySnapPoint(TrackReader track, Vector2d point, out bool snapped)
        {
            Vector2d notpoint = point; // This is somewhat lazy code used in GetSnapPoint_Grid to represent a point not equal to the actual point (TODO make something better)
            notpoint.X += 1.0;
            GameLine[] lines = LineEndsInRadius(track, point, SnapRadius);
            if (lines.Length == 0)
            {
                snapped = GetSnapPoint_Grid(point, notpoint, point, out Vector2d gsnappos);

                return snapped ? gsnappos : point;
            }
            GameLine snap = lines[0];
            snapped = true;
            return Utility.CloserPoint(point, snap.Position1, snap.Position2);
        }
        /// <summary>
        /// Snaps the point specified in endpoint of line to another line if within snapradius
        /// </summary>
        protected void SnapLineEnd(TrackWriter trk, GameLine line, Vector2d endpoint)
        {
            int[] ignore = new int[] { line.ID };
            bool snapped = GetSnapPoint(
                trk,
                line.Position1,
                line.Position2,
                endpoint,
                ignore,
                out Vector2d snap,
                line is StandardLine);

            if (!snapped)
            {
                _ = GetSnapPoint_Grid(line.Position1, line.Position2, endpoint, out snap);
            }

            if (snap == endpoint)
                return;
            if (line.Position1 == endpoint)
            {
                // Don't snap to the same point.
                if (line.Position2 != snap)
                    trk.MoveLine(line, snap, line.Position2);
            }
            else if (line.Position2 == endpoint)
            {
                // Don't snap to the same point.
                if (line.Position1 != snap)
                    trk.MoveLine(line, line.Position1, snap);
            }
        }
        /// <summary>
        /// Snaps the point specified in endpoint of line to another line if 
        /// within snapradius, ignoring all lines specified.
        /// </summary>
        protected bool GetSnapPoint(
            TrackReader trk,
            Vector2d position1,
            Vector2d position2,
            Vector2d endpoint,
            ICollection<int> ignorelines,
            out Vector2d snappoint,
            bool ignorescenery)
        {
            GameLine[] lines = LineEndsInRadius(trk, endpoint, SnapRadius);
            for (int i = 0; i < lines.Length; i++)
            {
                GameLine curr = lines[i];
                if (!ignorelines.Contains(curr.ID))
                {
                    if (ignorescenery && curr is SceneryLine)
                        continue; // Phys lines dont wanna snap to scenery

                    Vector2d snap = Utility.CloserPoint(endpoint, curr.Position1, curr.Position2);
                    if (position1 == endpoint)
                    {
                        // Don't snap to the same point.
                        if (position2 != snap)
                        {
                            snappoint = snap;
                            return true;
                        }
                    }
                    else if (position2 == endpoint)
                    {
                        // Don't snap to the same point.
                        if (position1 != snap)
                        {
                            snappoint = snap;
                            return true;
                        }
                    }
                    else
                    {
                        throw new Exception("Endpoint does not match line position in snap. It's not one of the ends of the line.");
                    }
                    break;
                }
            }
            snappoint = endpoint;
            return false;
        }

        /// <summary>
        /// Snaps the point specified in endpoint of line to any displayed
        /// grid if within snapradius
        /// </summary>
        protected bool GetSnapPoint_Grid(
            Vector2d position1,
            Vector2d position2,
            Vector2d endpoint,
            out Vector2d snappoint)
        {
            Vector2d closestCollisionGridPoint = endpoint; // Just set these to endpoint for now, they will be reset in the function
            Vector2d closestFloatGridPoint = endpoint;
            Vector2d closestGridPoint = endpoint;
            if (!Settings.Editor.SnapToGrid || (!Settings.DrawCollisionGrid && !Settings.DrawFloatGrid)) // If SnapToGrid is disabled or there is no grid to snap to...
            {
                snappoint = endpoint;
                return false;
            }

            if (Settings.DrawCollisionGrid)
            {
                double gridsize = SimulationGrid.CellSize;
                double lowGridX = gridsize * Math.Floor(endpoint.X / gridsize);
                double lowGridY = gridsize * Math.Floor(endpoint.Y / gridsize);

                closestCollisionGridPoint.X = endpoint.X - lowGridX < gridsize / 2.0 ? lowGridX : lowGridX + gridsize;
                closestCollisionGridPoint.Y = endpoint.Y - lowGridY < gridsize / 2.0 ? lowGridY : lowGridY + gridsize;

                closestGridPoint = closestCollisionGridPoint;
            }

            if (Settings.DrawFloatGrid)
            {
                double lowGridX = Math.Sign(endpoint.X) * Math.Pow(2.0, Math.Floor(Math.Log(Math.Abs(endpoint.X)) / Math.Log(2.0))); //2^floor(log_2{|x|}), i.e. float grid cell below (multiplied by sign to handle negatives)
                double lowGridY = Math.Sign(endpoint.Y) * Math.Pow(2.0, Math.Floor(Math.Log(Math.Abs(endpoint.Y)) / Math.Log(2.0))); //2^floor(log_2{|y|}), i.e. float grid cell below (multiplied by sign to handle negatives)

                closestFloatGridPoint.X = Math.Abs(endpoint.X - lowGridX) < Math.Abs(2.0 * lowGridX - endpoint.X) ? lowGridX : 2.0 * lowGridX;
                closestFloatGridPoint.Y = Math.Abs(endpoint.Y - lowGridY) < Math.Abs(2.0 * lowGridY - endpoint.Y) ? lowGridY : 2.0 * lowGridY;

                closestGridPoint = closestFloatGridPoint;
            }

            if (Settings.DrawCollisionGrid && Settings.DrawFloatGrid)
            {
                closestGridPoint = Utility.CloserPoint(endpoint, closestCollisionGridPoint, closestFloatGridPoint);
            }

            if ((closestGridPoint - endpoint).Length < SnapRadius + 1.0) // The extra 1.0 gives it the same radius as a standard line would have
            {
                if ((position1 == endpoint && position2 != closestGridPoint) || // Make sure both lines don't snap to the same point!
                    (position2 == endpoint && position1 != closestGridPoint))
                {
                    snappoint = closestGridPoint;
                    return true;
                }
            }

            snappoint = endpoint;
            return false;
        }
    }
}