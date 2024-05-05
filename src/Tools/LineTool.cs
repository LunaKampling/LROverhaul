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
using System;
using System.Drawing;
using Color = System.Drawing.Color;

namespace linerider.Tools
{
    public class LineTool : Tool
    {
        public override string Name => "Line Tool";
        public override Bitmap Icon => GameResources.icon_tool_line.Bitmap;
        public override MouseCursor Cursor => game.Cursors.List[CursorsHandler.Type.Line];
        public override Swatch Swatch => SharedSwatches.DrawingToolsSwatch;
        public override bool ShowSwatch => true;
        public bool Snapped = false;
        private const float MINIMUM_LINE = 0.01f;
        private bool _addflip;
        private Vector2d _end;
        private Vector2d _start;

        public LineTool() : base()
        {
            Swatch.Selected = LineType.Standard;
        }

        public override void OnChangingTool() => Stop();
        public override void OnMouseDown(Vector2d pos)
        {
            Active = true;
            Vector2d gamepos = ScreenToGameCoords(pos);
            if (EnableSnap)
            {
                using (TrackReader trk = game.Track.CreateTrackReader())
                {
                    Vector2d snap = TrySnapPoint(trk, gamepos, out bool success);
                    if (success)
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
                _start = gamepos;
                Snapped = false;
            }

            _addflip = InputUtils.Check(Hotkey.LineToolFlipLine);
            _end = _start;
            game.Invalidate();
            base.OnMouseDown(pos);
        }

        public override void OnMouseMoved(Vector2d pos)
        {
            if (Active)
            {
                _end = ScreenToGameCoords(pos);
                if (game.ShouldXySnap())
                {
                    _end = Utility.SnapToDegrees(_start, _end);
                }
                else if (EnableSnap)
                {
                    using (TrackReader trk = game.Track.CreateTrackReader())
                    {
                        Vector2d snap = TrySnapPoint(trk, _end, out bool snapped);
                        if (snapped && snap != _start)
                        {
                            _end = snap;
                        }
                    }
                }
                game.Invalidate();
            }
            base.OnMouseMoved(pos);
        }

        public override void OnMouseUp(Vector2d pos)
        {
            game.Invalidate();
            if (Active)
            {
                Active = false;
                Vector2d diff = _end - _start;
                double x = diff.X;
                double y = diff.Y;
                if (Math.Abs(x) + Math.Abs(y) < MINIMUM_LINE)
                    return;
                if (game.ShouldXySnap())
                {
                    _end = Utility.SnapToDegrees(_start, _end);
                }
                else if (EnableSnap)
                {
                    using (TrackWriter trk = game.Track.CreateTrackWriter())
                    {
                        Vector2d snap = TrySnapPoint(trk, _end, out bool snapped);
                        if (snapped && snap != _start)
                        {
                            _end = snap;
                        }
                    }
                }
                if ((_end - _start).Length >= MINIMUM_LINE)
                {
                    using (TrackWriter trk = game.Track.CreateTrackWriter())
                    {
                        game.Track.UndoManager.BeginAction();
                        GameLine added = CreateLine(trk, _start, _end, _addflip, Snapped, EnableSnap,
                            Swatch.Selected, trk.Track._layers.currentLayer, Swatch.RedMultiplier, Swatch.GreenMultiplier);
                        game.Track.UndoManager.EndAction();
                        if (added is StandardLine)
                        {
                            game.Track.NotifyTrackChanged();
                        }
                    }
                    game.Invalidate();
                }
            }
            Snapped = false;
            base.OnMouseUp(pos);
        }
        public override void Render(Layer layer)
        {
            base.Render(layer);
            if (Active)
            {
                Vector2d diff = _end - _start;
                double x = diff.X;
                double y = diff.Y;
                Color c = Color.FromArgb(200, 150, 150, 150);
                if (Math.Abs(x) + Math.Abs(y) < MINIMUM_LINE)
                {
                    c = Color.Red;
                    float sz = 2f;
                    if (Swatch.Selected == LineType.Scenery)
                        sz *= Swatch.GreenMultiplier;
                    GameRenderer.RenderRoundedLine(_start, _end, c, sz);
                }
                else
                {
                    switch (Swatch.Selected)
                    {
                        case LineType.Standard:
                            StandardLine sl = new StandardLine(layer, _start, _end, _addflip);
                            sl.CalculateConstants();
                            GameRenderer.DrawTrackLine(sl, c, Settings.Editor.RenderGravityWells, true);
                            break;

                        case LineType.Acceleration:
                            RedLine rl = new RedLine(layer, _start, _end, _addflip)
                            {
                                Multiplier = Swatch.RedMultiplier
                            };
                            rl.CalculateConstants();
                            GameRenderer.DrawTrackLine(rl, c, Settings.Editor.RenderGravityWells, true);
                            break;

                        case LineType.Scenery:
                            GameRenderer.RenderRoundedLine(_start, _end, c, 2 * Swatch.GreenMultiplier);
                            break;
                    }
                }
            }
        }

        public override void Cancel() => Stop();
        public override void Stop() => Active = false;
    }
}