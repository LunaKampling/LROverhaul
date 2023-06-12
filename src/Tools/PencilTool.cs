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
using linerider.Utils;
using linerider.UI;
using System.Drawing;

namespace linerider.Tools
{
    public class PencilTool : Tool
    {
        public override Bitmap Icon => GameResources.icon_tool_pencil.Bitmap;
        public override Hotkey Hotkey => Hotkey.EditorPencilTool;
        public override string Name => "Pencil Tool";
        public override bool RequestsMousePrecision
        {
            get => DrawingScenery;
        }
        public override Swatch Swatch
        {
            get => SharedSwatches.DrawingToolsSwatch;
        }
        public override bool ShowSwatch
        {
            get => true;
        }
        public override bool NeedsRender
        {
            get => DrawingScenery || Active;
        }
        public bool Snapped = false;
        private bool _drawn;
        private Vector2d _start;
        private Vector2d _end;
        private float MINIMUM_LINE
        {
            get => 6f / game.Track.Zoom;
        }
        private bool _addflip = false;
        private Vector2d _mouseshadow;
        public bool DrawingScenery
        {
            get => Swatch.Selected == LineType.Scenery;
        }
        public override MouseCursor Cursor
        {
            get => game.Cursors.List[CursorsHandler.Type.Pencil];
        }
        public PencilTool() : base()
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
            _drawn = true;
            using (var trk = game.Track.CreateTrackWriter())
            {
                var added = CreateLine(trk, _start, _end, _addflip, Snapped, false,
                    Swatch.Selected, Swatch.RedMultiplier, Swatch.GreenMultiplier);
                if (added is StandardLine)
                {
                    game.Track.NotifyTrackChanged();
                }
            }
            game.Invalidate();
        }
        public override void OnMouseMoved(Vector2d pos)
        {
            if (Active)
            {
                _end = ScreenToGameCoords(pos);
                var diff = _end - _start;
                var len = diff.Length;

                if ((DrawingScenery && len >= MINIMUM_LINE) ||
                    (len >= MINIMUM_LINE * 2))
                {
                    AddLine();
                    _start = _end;
                    Snapped = true;//we are now connected to the newest line
                }
                game.Invalidate();
            }

            _mouseshadow = ScreenToGameCoords(pos);
            base.OnMouseMoved(pos);
        }
        public override void OnMouseUp(Vector2d pos)
        {
            game.Invalidate();
            if (Active)
            {
                if (DrawingScenery && ScreenToGameCoords(pos) == _start)
                {
                    AddLine();
                    _mouseshadow = ScreenToGameCoords(pos);
                }
                else
                {
                    OnMouseMoved(pos);
                }
                Stop();
            }
            base.OnMouseUp(pos);
        }
        public override void Render()
        {
            base.Render();
            if (DrawingScenery && _mouseshadow != Vector2d.Zero && !game.Track.Playing)
            {
                GameRenderer.RenderRoundedLine(_mouseshadow, _mouseshadow, Color.FromArgb(100, Settings.Colors.SceneryLine), 2f * Swatch.GreenMultiplier, false, false);
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

                if (_drawn)
                {
                    game.Track.UndoManager.EndAction();
                }
                else
                {
                    game.Track.UndoManager.CancelAction();
                }
                _drawn = false;
            }
            _mouseshadow = Vector2d.Zero;
        }
    }
}