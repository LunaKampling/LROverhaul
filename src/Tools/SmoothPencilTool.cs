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
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using SkiaSharp;

namespace linerider.Tools
{
    public class SmoothPencilTool : Tool
    {
        public override string Name => "Smooth Pencil Tool";
        public override SKBitmap Icon => GameResources.icon_tool_smooth_pencil.Bitmap;

        private bool SmoothMoved = false;

        public override bool RequestsMousePrecision => Active;
        public override Swatch Swatch => SharedSwatches.DrawingToolsSwatch;
        public override bool ShowSwatch => true;
        public override bool NeedsRender => Active;

        private float MINIMUM_LINE => 12f / game.Track.Zoom;

        public bool Snapped = false;
        private Vector2d _diff;
        private Vector2d _start;
        private Vector2d _end;
        private Vector2d _dragto;
        private readonly System.Diagnostics.Stopwatch stopWatch = new();

        private bool _addflip = false;
        private Vector2d _mouseshadow;
        public override MouseCursor Cursor => game.Cursors.List[CursorsHandler.Type.Pencil];
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
                Vector2d gamepos = ScreenToGameCoords(pos);
                using TrackReader trk = game.Track.CreateTrackReader();
                Vector2d snap = TrySnapPoint(trk, gamepos, out bool snapped);
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
            else
            {
                _start = ScreenToGameCoords(pos);
                Snapped = false;
            }
            _addflip = InputUtils.Check(Hotkey.LineToolFlipLine);
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
                using (TrackWriter trk = game.Track.CreateTrackWriter())
                {
                    _end = _start + _diff / Settings.SmoothPencil.smoothStabilizer;
                    GameLine added = CreateLine(trk, _start, _end, _addflip, Snapped, false,
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
            if (Active && !SmoothMoved && _diff.Length > MINIMUM_LINE * 5)
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
                GameRenderer.RenderRoundedLine(_mouseshadow, _mouseshadow, Settings.NightMode ? Color.FromArgb(100, 255, 255, 255) : Color.FromArgb(100, 0, 0, 0), SharedSwatches.DrawingToolsSwatch.Selected == LineType.Scenery ? 2f * Swatch.GreenMultiplier : 2f, false, false);
            }
        }
        public override void Cancel() => Stop();
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
                Snapped = true; // We are now connected to the newest line
            }
        }
    }
}