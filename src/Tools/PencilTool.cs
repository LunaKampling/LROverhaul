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
using OpenTK;
using System.Drawing;
using Color = System.Drawing.Color;

namespace linerider.Tools
{
    public class PencilTool : Tool
    {
        public override string Name => "Pencil Tool";
        public override Bitmap Icon => GameResources.icon_tool_pencil.Bitmap;
        public override bool RequestsMousePrecision => DrawingScenery;
        public override Swatch Swatch => SharedSwatches.DrawingToolsSwatch;
        public override bool ShowSwatch => true;
        public override bool NeedsRender => DrawingScenery || Active;
        public bool Snapped = false;
        private bool _drawn;
        private Vector2d _start;
        private Vector2d _end;
        private float MINIMUM_LINE => 6f / game.Track.Zoom;
        private bool _addflip = false;
        private Vector2d _mouseshadow;
        public bool DrawingScenery => Swatch.Selected == LineType.Scenery;
        public override MouseCursor Cursor => game.Cursors.List[CursorsHandler.Type.Pencil];
        public PencilTool() : base()
        {
            Swatch.Selected = LineType.Standard;
        }
        public override void OnMouseDown(Vector2d pos, bool nodraw)
        {
            Stop();
            if (!nodraw)
            {
                Active = true;

                if (EnableSnap)
                {
                    Vector2d gamepos = ScreenToGameCoords(pos);
                    using (TrackReader trk = game.Track.CreateTrackReader())
                    {
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
            }
            base.OnMouseDown(pos, nodraw);
        }
        public override void OnChangingTool()
        {
            Stop();
            _mouseshadow = Vector2d.Zero;
        }
        private void AddLine()
        {
            _drawn = true;
            using (TrackWriter trk = game.Track.CreateTrackWriter())
            {
                GameLine added = CreateLine(trk, _start, _end, _addflip, Snapped, false,
                    Swatch.Selected, trk.Track._layers.currentLayer, Swatch.RedMultiplier, Swatch.GreenMultiplier);
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
                Vector2d diff = _end - _start;
                double len = diff.Length;

                if ((DrawingScenery && len >= MINIMUM_LINE) ||
                    (len >= MINIMUM_LINE * 2))
                {
                    AddLine();
                    _start = _end;
                    Snapped = true; // We are now connected to the newest line
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
        public override void Render(Layer layer)
        {
            base.Render(layer);
            if (DrawingScenery && _mouseshadow != Vector2d.Zero && !game.Track.Playing)
            {
                GameRenderer.RenderRoundedLine(_mouseshadow, _mouseshadow, Color.FromArgb(100, Settings.Colors.SceneryLine), 2f * Swatch.GreenMultiplier, false, false);
            }
        }
        public override void Cancel() => Stop();
        public override void Stop()
        {
            if (Active)
            {
                Active = false;

                if (_drawn)
                {
                    game.Track.UndoManager.EndAction();
                    _drawn = false;
                }
                else
                {
                    game.Track.UndoManager.CancelAction();
                }
            }
            _mouseshadow = Vector2d.Zero;
        }
    }
}