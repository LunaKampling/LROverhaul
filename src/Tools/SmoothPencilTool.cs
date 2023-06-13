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
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Color = System.Drawing.Color;
using linerider.Rendering;
using linerider.Game;
using linerider.UI;
using System.Drawing;

namespace linerider.Tools
{
    public class SmoothPencilTool : Tool
    {
        public override Bitmap Icon => GameResources.icon_tool_smooth_pencil.Bitmap;
        public override Hotkey Hotkey => Hotkey.EditorPencilTool;
        public override string Name => "Smooth Pencil Tool";

        private bool SmoothMoved = false;

        public override bool RequestsMousePrecision
        {
            get
            {
                return Active;
            }
        }
        public override Swatch Swatch
        {
            get
            {
                return SharedSwatches.DrawingToolsSwatch;
            }
        }
        public override bool ShowSwatch
        {
            get
            {
                return true;
            }
        }
        public override bool NeedsRender
        {
            get
            {
                return Active;
            }
        }

        private float MINIMUM_LINE
        {
            get
            {
                return 12f / game.Track.Zoom;
            }
        }

        public bool Snapped = false;
        private Vector2d _diff;
        private Vector2d _start;
        private Vector2d _end;
        private Vector2d _dragto;
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        private bool _addflip = false;
        private Vector2d _mouseshadow;
        public override MouseCursor Cursor
        {
            get { return game.Cursors.List[CursorsHandler.Type.Pencil]; }
        }
        public SmoothPencilTool() : base()
        {
            Swatch.Selected = LineType.Standard;
        }
        public override void OnMouseDown(Vector2d pos)
        {
            Stop();
            Active = true;

            if (EnableSnap)
            {
                var gamepos = ScreenToGameCoords(pos);
                using (var trk = game.Track.CreateTrackReader())
                {
                    var snap = TrySnapPoint(trk, gamepos, out bool snapped);
                    if (snapped)
                    {
                        _start = snap;
                        Snapped = true;
                    }
                    else
                    {
                        _start = gamepos;
                        Snapped = false;
                    }
                }
            }
            else
            {
                _start = ScreenToGameCoords(pos);
                Snapped = false;
            }
            _addflip = UI.InputUtils.Check(UI.Hotkey.LineToolFlipLine);
            _end = _start;
            game.Invalidate();
            game.Track.UndoManager.BeginAction();
            base.OnMouseDown(pos);
        }
        public override void OnChangingTool()
        {
            Stop();
            _mouseshadow = Vector2d.Zero;
        }
        private void AddLine()
        {
            _diff = _dragto - _start;
            if (_diff.Length > MINIMUM_LINE)
            {
                using (var trk = game.Track.CreateTrackWriter())
                {
                    _end = _start + _diff / Settings.SmoothPencil.smoothStabilizer;
                    var added = CreateLine(trk, _start, _end, _addflip, Snapped, false,
                        Swatch.Selected, Swatch.RedMultiplier, Swatch.GreenMultiplier);
                    _start = _end;
                    if (added is StandardLine)
                    {
                        game.Track.NotifyTrackChanged();
                    }
                }
                game.Invalidate();
            }
        }
        public override void OnMouseMoved(Vector2d pos)
        {
            if (Active)
            {
                _dragto = ScreenToGameCoords(pos);
            }
            if (!SmoothMoved)
            {
                _diff = _dragto - _start;
            }
            if (Active && (!SmoothMoved && _diff.Length > MINIMUM_LINE * 5))
            {
                SmoothMoved = true;
                stopWatch.Start();
            }
            _mouseshadow = ScreenToGameCoords(pos);
            base.OnMouseMoved(pos);
        }
        public override void OnMouseUp(Vector2d pos)
        {
            game.Invalidate();
            Stop();
            Active = false;
            base.OnMouseUp(pos);
        }
        public override void Render()
        {
            base.Render();
            if (_mouseshadow != Vector2d.Zero && !game.Track.Playing)
            {
                GameRenderer.RenderRoundedLine(_mouseshadow, _mouseshadow, (Settings.NightMode ? Color.FromArgb(100, 255, 255, 255) : Color.FromArgb(100, 0, 0, 0)), (SharedSwatches.DrawingToolsSwatch.Selected == LineType.Scenery ? 2f * Swatch.GreenMultiplier : 2f), false, false);
            }
        }
        public override void Cancel()
        {
            Stop();
        }
        public override void Stop()
        {
            if (Active)
            {
                Active = false;

                if (SmoothMoved)
                {
                    game.Track.UndoManager.EndAction();
                    SmoothMoved = false;
                    stopWatch.Reset();
                }
                else
                {
                    game.Track.UndoManager.CancelAction();
                }
            }
            _mouseshadow = Vector2d.Zero;
        }

        public void UpdateSmooth()
        {
            if (SmoothMoved && stopWatch.ElapsedMilliseconds > Settings.SmoothPencil.smoothUpdateSpeed)
            {
                    stopWatch.Restart();
                    AddLine();
                    Snapped = true;//we are now connected to the newest line
            } 
        }
    }
}