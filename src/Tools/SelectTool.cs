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
using linerider.UI;
using linerider.Utils;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace linerider.Tools
{
    public class SelectTool : Tool
    {
        public override string Name => "Select Tool";
        public override Bitmap Icon => GameResources.icon_tool_select.Bitmap;
        public override Swatch Swatch => SharedSwatches.EraserAndSelectToolSwatch;
        public override bool ShowSwatch => true;
        public override string Tooltip
        {
            get
            {
                if (_active && _selection != null && _selection.line != null)
                {
                    Vector2d vec = _selection.line.GetVector();
                    double len = vec.Length;
                    Angle angle = Angle.FromVector(vec);
                    angle.Degrees += 90;
                    string tooltip = "";
                    if (Settings.Editor.ShowLineLength)
                        tooltip += "length: " + Math.Round(len, 2);
                    if (Settings.Editor.ShowLineAngle)
                        tooltip += " \n" +
                        "angle: " + Math.Round(angle.Degrees, 2) + "° ";
                    if (Settings.Editor.ShowLineID &&
                            _selection.line.Type != LineType.Scenery)
                        tooltip += "\n" +
                        "ID: " + _selection.line.ID + " ";
                    return tooltip;
                }
                return "";
            }
        }
        public override MouseCursor Cursor => game.Cursors.List[CursorsHandler.Type.Select];
        protected override bool EnableSnap
        {
            get
            {
                bool toggle = InputUtils.CheckPressed(Hotkey.ToolToggleSnap);
                return Settings.Editor.SnapMoveLine != toggle;
            }
        }
        public bool CanLifelock => InputUtils.Check(Hotkey.ToolLifeLock) &&
        CurrentTools.CurrentTool == this;
        private Vector2d _clickstart;
        private bool _lifelocking = false;
        private LineSelection _selection;
        private bool _active = false;
        private GameLine _hoverline = null;
        private bool _hoverknob = false;
        private bool _hoverknobjoint1;
        private readonly Stopwatch _hovertime = new Stopwatch();
        private bool _hoverclick = false;
        public override bool Active
        {
            get => _active;
            protected set => Debug.Fail($"Cannot set MoveTool.Active, use {nameof(_active)} instead");
        }
        public SelectTool()
        {
        }
        private void UpdatePlayback(GameLine line)
        {
            if (line is StandardLine stdline && CanLifelock)
            {
                game.Track.NotifyTrackChanged();
                using (TrackReader trk = game.Track.CreateTrackReader())
                {
                    if (!LifeLock(trk, game.Track.Timeline, stdline))
                    {
                        _lifelocking = true;
                    }
                    else if (_lifelocking)
                    {
                        DropLine();
                    }
                }
            }
            else
            {
                game.Track.NotifyTrackChanged();
            }
        }
        private bool SelectLine(Vector2d gamepos)
        {
            using (TrackReader trk = game.Track.CreateTrackReader())
            {
                GameLine line = SelectLine(trk, gamepos, out bool knob);
                if (line != null)
                {
                    _clickstart = gamepos;
                    _active = true;
                    // Is it a knob?
                    if (knob)
                    {
                        if (InputUtils.Check(Hotkey.ToolSelectBothJoints))
                        {
                            _selection = new LineSelection(line, bothjoints: true);
                        }
                        else
                        {
                            Vector2d knobpos = Utility.CloserPoint(
                                gamepos,
                                line.Position1,
                                line.Position2);
                            _selection = new LineSelection(line, knobpos);
                            _hoverclick = true;
                            _hovertime.Restart();
                            foreach (GameLine snap in LineEndsInRadius(trk, knobpos, 1))
                            {
                                if ((snap.Position1 == knobpos ||
                                    snap.Position2 == knobpos) &&
                                    snap != line)
                                {
                                    _selection.snapped.Add(new LineSelection(snap, knobpos));
                                }
                            }
                        }
                        return true;
                    }
                    else
                    {
                        _selection = new LineSelection(line, true);
                        return true;
                    }
                }
            }
            return false;
        }
        private void MoveSelection(Vector2d pos)
        {
            if (_selection != null)
            {
                GameLine line = _selection.line;
                using (TrackWriter trk = game.Track.CreateTrackWriter())
                {
                    trk.DisableUndo();
                    Vector2d joint1 = _selection.joint1
                        ? _selection.clone.Position1 + (pos - _clickstart)
                        : line.Position1;
                    Vector2d joint2 = _selection.joint2
                        ? _selection.clone.Position2 + (pos - _clickstart)
                        : line.Position2;
                    bool mod = ApplyModifiers(ref joint1, ref joint2);

                    if (!mod && EnableSnap)
                    {
                        SnapJoints(trk, line, ref joint1, ref joint2);
                    }

                    //GameLine newLine = line;
                    //if (UI.InputUtils.Check(Hotkey.DrawDebugCamera))
                    //{
                    //    GameLine movedLine = line;
                    //    movedLine.Position = joint1;
                    //    movedLine.Position2 = joint2;
                    //    newLine = FixNoFakie2(game.Track.Timeline, line, _selection.joint1 ? line.Position : line.Position2);
                    //    joint1 = newLine.Position;
                    //    joint2 = newLine.Position2;
                    //    //trk.MoveLine(movedLine, newLine.Position, newLine.Position2);
                    //    //line = newLine;
                    //}

                    trk.MoveLine(
                        line,
                        joint1,
                        joint2);
                    //line = newLine;

                    foreach (LineSelection sl in _selection.snapped)
                    {
                        GameLine snap = sl.line;
                        Vector2d snapjoint = _selection.joint1 ? joint1 : joint2;
                        trk.MoveLine(
                            snap,
                            sl.joint1 ? snapjoint : snap.Position1,
                            sl.joint2 ? snapjoint : snap.Position2);
                    }
                }
                UpdatePlayback(_selection.line);
            }
            game.Invalidate();
        }
        private void UpdateHoverline(Vector2d gamepos)
        {
            GameLine oldhover = _hoverline;
            _hoverline = null;
            if (!_active)
            {
                using (TrackReader trk = game.Track.CreateTrackReader())
                {
                    GameLine line = SelectLine(trk, gamepos, out bool knob);
                    if (line != null)
                    {
                        _hoverline = line;
                        if (knob)
                        {
                            Vector2d point = Utility.CloserPoint(
                                gamepos,
                                line.Position1,
                                line.Position2);

                            bool joint1 = point == line.Position1;

                            if (_hoverline != oldhover ||
                            _hoverknobjoint1 != joint1 ||
                            _hoverknob != knob)
                            {
                                _hoverclick = false;
                                _hovertime.Restart();
                            }
                            _hoverknobjoint1 = joint1;
                        }
                        _hoverknob = knob;
                    }
                }
            }
        }

        public override void OnMouseDown(Vector2d mousepos)
        {
            base.OnMouseDown(mousepos);
            Vector2d gamepos = ScreenToGameCoords(mousepos);

            Stop(); // Double check
            if (!SelectLine(gamepos))
            {
                CurrentTools.SetTool(CurrentTools.SelectSubtool);
                CurrentTools.SelectSubtool.OnMouseDown(mousepos);
                IsLeftMouseDown = false;
                _hoverline = null;
            }
            else
            {
                UpdateHoverline(gamepos);
            }
        }
        public override void OnMouseUp(Vector2d pos)
        {
            DropLine();
            UpdateHoverline(ScreenToGameCoords(pos));
            base.OnMouseUp(pos);
        }
        public override void OnMouseMoved(Vector2d pos)
        {
            UpdateHoverline(ScreenToGameCoords(pos));
            if (_active)
            {
                MoveSelection(ScreenToGameCoords(pos));
            }
            base.OnMouseMoved(pos);
        }

        public override void OnMouseRightDown(Vector2d pos)
        {
            Stop(); // Double check
            Vector2d gamepos = ScreenToGameCoords(pos);
            using (TrackWriter trk = game.Track.CreateTrackWriter())
            {
                GameLine line = SelectLine(trk, gamepos, out bool knob);
                if (line != null)
                {
                    game.Canvas.ShowLineWindow(line, (int)pos.X, (int)pos.Y);
                }
            }
            base.OnMouseRightDown(pos);
        }
        public override void OnChangingTool() => Stop();
        public override void Render()
        {
            if (_hoverline != null)
            {
                DrawHover(
                    _hoverline,
                     _hoverknob && _hoverknobjoint1,
                     _hoverknob && !_hoverknobjoint1,
                     false);
            }
            if (_active)
            {
                DrawHover(
                    _selection.line,
                    _selection.joint1,
                    _selection.joint2,
                    true);
            }
            base.Render();
        }
        private void DrawHover(GameLine line,
            bool knob1, bool knob2, bool selected = false)
        {
            Vector2d start = line.Position1;
            Vector2d end = line.Position2;
            float width = line.Width;
            long elapsed = _hovertime.ElapsedMilliseconds;
            int animtime = 250;
            if (_hovertime.IsRunning)
            {
                if (elapsed > animtime * 2)
                {
                    if (_hoverclick)
                        _hovertime.Stop();
                    else
                        _hovertime.Stop();
                }
                game.Track.Invalidate();
            }
            float hoverratio;
            if (_hoverclick)
            {
                animtime = 75;
                elapsed += 75 / 4;
                hoverratio = Math.Min(animtime, elapsed) / (float)animtime;
            }
            else
            {
                hoverratio = Math.Min(Math.Min(animtime, elapsed) / (float)animtime, 0.5f);
            }
            bool both = knob1 == knob2 == true;
            int linealpha = both ? 64 : 48;
            if (selected && both)
                linealpha += 16;
            GameRenderer.RenderRoundedLine(
                start,
                end,
                Color.FromArgb(linealpha, Color.FromArgb(127, 127, 127)),
                width * 2);

            bool canlifelock = CanLifelock && line.Type != LineType.Scenery;
            GameRenderer.DrawKnob(start, knob1, canlifelock, width, hoverratio);
            GameRenderer.DrawKnob(end, knob2, canlifelock, width, hoverratio);

        }
        private void DropLine()
        {
            if (_active)
            {
                _hoverline = _selection.line;
                _hoverknob = !_selection.BothJoints;
                _hoverknobjoint1 = _selection.joint1;
            }
            _lifelocking = false;
            if (_active)
            {
                if (_selection != null)
                {
                    game.Track.UndoManager.BeginAction();
                    game.Track.UndoManager.AddChange(_selection.clone, _selection.line);
                    foreach (LineSelection s in _selection.snapped)
                    {
                        game.Track.UndoManager.AddChange(s.clone, s.line);
                    }
                    game.Track.UndoManager.EndAction();
                }
                game.Invalidate();
            }
            _active = false;
            _selection = null;
        }
        public override void Cancel() => Stop();
        public override void Stop()
        {
            DropLine();
            _hoverline = null;
            _hoverclick = false;
        }
        private bool ApplyModifiers(ref Vector2d joint1, ref Vector2d joint2) // Modifies the movement to account for angle locks & stuff
        {
            bool both = _selection.joint1 && _selection.joint2;
            bool modified = false;
            if (both)
            {
                bool axis = InputUtils.CheckPressed(Hotkey.ToolAxisLock);
                bool perpendicularaxis = InputUtils.CheckPressed(Hotkey.ToolPerpendicularAxisLock);
                if (axis || perpendicularaxis)
                {
                    Angle angle = Angle.FromVector(_selection.clone.GetVector());
                    if (perpendicularaxis)
                    {
                        angle.Degrees -= 90;
                    }
                    joint1 = Utility.AngleLock(_selection.line.Position1, joint1, angle);
                    joint2 = Utility.AngleLock(_selection.line.Position2, joint2, angle);
                    modified = true;
                }
            }
            else
            {
                Vector2d start = _selection.joint1 ? joint2 : joint1;
                Vector2d end = _selection.joint2 ? joint2 : joint1;
                if (InputUtils.Check(Hotkey.ToolAngleLock))
                {
                    end = Utility.AngleLock(start, end, Angle.FromVector(_selection.clone.GetVector()));
                    modified = true;
                }
                if (InputUtils.CheckPressed(Hotkey.ToolXYSnap))
                {
                    end = Utility.SnapToDegrees(start, end);
                    modified = true;
                }
                if (InputUtils.Check(Hotkey.ToolLengthLock))
                {
                    Vector2d currentdelta = _selection.line.Position2 - _selection.line.Position1;
                    end = Utility.LengthLock(start, end, currentdelta.Length);
                    modified = true;
                }

                if (_selection.joint2)
                    joint2 = end;
                else
                    joint1 = end;
            }
            return modified;
        }
        private void SnapJoints(TrackReader trk, GameLine line,
         ref Vector2d joint1, ref Vector2d joint2)
        {
            HashSet<int> ignoreids = new HashSet<int>
            {
                _selection.line.ID
            };
            foreach (LineSelection snapped in _selection.snapped)
            {
                _ = ignoreids.Add(snapped.line.ID);
            }
            Vector2d snapj1 = joint1;
            Vector2d snapj2 = joint2;

            bool j1snapped = false;
            bool j2snapped = false;

            bool ignorescenery = line is StandardLine;
            if (_selection.joint1)
            {
                j1snapped = GetSnapPoint(trk, joint1, line.Position2, joint1,
                    ignoreids, out snapj1, ignorescenery);

                if (!j1snapped)
                    j1snapped = GetSnapPoint_Grid(joint1, line.Position2, joint1, out snapj1);
            }
            if (_selection.joint2)
            {
                j2snapped = GetSnapPoint(trk, line.Position1, joint2, joint2,
                    ignoreids, out snapj2, ignorescenery);

                if (!j2snapped)
                    j2snapped = GetSnapPoint_Grid(line.Position1, joint2, joint2, out snapj2);
            }
            if (_selection.BothJoints)
            {
                Vector2d j1diff = snapj1 - joint1;
                Vector2d j2diff = snapj2 - joint2;
                if (j1snapped && j2snapped)
                {
                    if (j1diff.Length < j2diff.Length)
                        j2snapped = false;
                    else
                        j1snapped = false;
                }
                if (j1snapped)
                    joint2 += j1diff;
                else if (j2snapped)
                    joint1 += j2diff;
            }
            if (j1snapped)
                joint1 = snapj1;
            else if (j2snapped)
                joint2 = snapj2;
        }
    }
}