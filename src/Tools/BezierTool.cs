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

using linerider.Rendering;
using OpenTK;
using System;
using Color = System.Drawing.Color;
using OpenTK.Input;
using linerider.Game;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;
using linerider.Utils;
using linerider.UI;
using System.Drawing;

namespace linerider.Tools
{
    public class BezierTool : Tool
    {
        public override Bitmap Icon => GameResources.icon_tool_bezier.Bitmap;
        public override Hotkey Hotkey => Hotkey.EditorLineTool;
        public override string Name => "Bezier Tool";
        public override MouseCursor Cursor
        {
            get { return game.Cursors.List[CursorsHandler.Type.Line]; }
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
        public bool Snapped = false;
        private bool done = false;
        private const float MINIMUM_LINE = 0.01f;
        private bool _addflip;
        private List<Vector2d> controlPoints = new List<Vector2d> {};
        private List<GameLine> workingLines = new List<GameLine> {};
        private Vector2d _end;
        private Vector2d _start;
        private bool moving = false;
        private int pointToMove = -1;
        private float nodeSize => Settings.Bezier.NodeSize / game.Track.Zoom;
        private float nodeThickness => 2f / game.Track.Zoom;

        public BezierTool()
            : base()
        {
            Swatch.Selected = LineType.Blue;
        }

        public override void OnChangingTool()
        {
            Stop();
        }
        public override void OnMouseDown(Vector2d pos)
        {
            Active = true;
            var gamepos = ScreenToGameCoords(pos);
            if (EnableSnap)
            {
                using (var trk = game.Track.CreateTrackReader())
                {
                    var snap = TrySnapPoint(trk, gamepos, out bool success);
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


            _addflip = UI.InputUtils.Check(UI.Hotkey.LineToolFlipLine);
            _end = _start;

            int closestIndex = -1;
            double closestDist = 100000;
            for (int i = 0; i < controlPoints.Count; i++)
            {
                var dist = GameRenderer.Distance(ScreenToGameCoords(pos), controlPoints[i]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                }
            }

            if (closestIndex >= 0 && closestDist < nodeSize)
            {
                moving = true;
                pointToMove = closestIndex;
            }
            else
            {
                moving = false;
                pointToMove = -1;
                if (controlPoints.Count < 20)
                {
                    controlPoints.Add(_end);
                }
            }

            game.Invalidate();
            base.OnMouseDown(pos);
        }

        public override void OnMouseRightDown(Vector2d pos)
        {
            int closestIndex = -1;
            double closestDist = 100000;
            for (int i = 0; i < controlPoints.Count; i++)
            {
                var dist = GameRenderer.Distance(ScreenToGameCoords(pos), controlPoints[i]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                }
            }

            if (closestIndex >= 0 && closestDist < nodeSize)
            {
                controlPoints.RemoveAt(closestIndex);
            }
            base.OnMouseRightDown(pos);
        }

        public override bool OnKeyDown(Key k)
        {
            switch (k)
            {
                case OpenTK.Input.Key.Space:
                case OpenTK.Input.Key.KeypadEnter:
                case OpenTK.Input.Key.Enter:
                    done = true;
                    FinalizePlacement();
                    break;
            }
            return base.OnKeyDown(k);
        }

        public override void OnMouseMoved(Vector2d pos)
        {
            if (Active)
            {
                if (pointToMove >= 0 && moving)
                {
                    controlPoints[pointToMove] = ScreenToGameCoords(pos);
                }

                if (game.ShouldXySnap())
                {
                    _end = Utility.SnapToDegrees(_start, _end);
                }
                else if (EnableSnap)
                {
                    using (var trk = game.Track.CreateTrackReader())
                    {
                        var snap = TrySnapPoint(trk, _end, out bool snapped);
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
                moving = false;
                pointToMove = -1;
                var diff = _end - _start;
                var x = diff.X;
                var y = diff.Y;
                if (game.ShouldXySnap())
                {
                    _end = Utility.SnapToDegrees(_start, _end);
                }
                else if (EnableSnap)
                {
                    using (var trk = game.Track.CreateTrackWriter())
                    {
                        var snap = TrySnapPoint(trk, _end, out bool snapped);
                        if (snapped && snap != _start)
                        {
                            _end = snap;
                        }
                    }
                }
            }
            Snapped = false;
            base.OnMouseUp(pos);
        }
        public override void Render()
        {
            base.Render();
            if (Active)
            {
                GeneratePreview();
            }

        }
        private void GeneratePreview()
        {
            DeleteLines();
            using (var trk = game.Track.CreateTrackWriter())
            {
                trk.DisableUndo();
                PlaceLines(trk, true);
            }
            if(Settings.Bezier.Mode == (int)Settings.BezierMode.Direct)
            {
                RenderDirect();
            }
            else if (Settings.Bezier.Mode == (int)Settings.BezierMode.Trace)
            {
                RenderTrace();
            }
        }
        private void RenderDirect()
        {
            BezierCurve curve;
            GameRenderer.GenerateBezierCurve2d(controlPoints.ToArray(), Settings.Bezier.Resolution, out curve);
            switch (Swatch.Selected)
            {
                case LineType.Blue:
                    GameRenderer.RenderPoints(controlPoints, curve, Settings.Colors.StandardLine, nodeSize, nodeThickness);
                    break;
                case LineType.Scenery:
                    GameRenderer.RenderPoints(controlPoints, curve, Settings.Colors.SceneryLine, nodeSize, nodeThickness);
                    break;
                case LineType.Red:
                    GameRenderer.RenderPoints(controlPoints, curve, Settings.Colors.AccelerationLine, nodeSize, nodeThickness);
                    break;
            }
        }
        private void RenderTrace()
        {
            switch (Swatch.Selected)
            {
                case LineType.Blue:
                    GameRenderer.RenderPoints(controlPoints, Settings.Colors.StandardLine, nodeSize, nodeThickness);
                    break;
                case LineType.Scenery:
                    GameRenderer.RenderPoints(controlPoints, Settings.Colors.SceneryLine, nodeSize, nodeThickness);
                    break;
                case LineType.Red:
                    GameRenderer.RenderPoints(controlPoints, Settings.Colors.AccelerationLine, nodeSize, nodeThickness);
                    break;
            }
        }
        private void FinalizePlacement()
        {
            Active = false;
            DeleteLines();
            using (var trk = game.Track.CreateTrackWriter())
            {
                PlaceLines(trk, false);
            }
            game.Invalidate();
            controlPoints.Clear();
            workingLines.Clear();
        }
        private void PlaceLines(TrackWriter trk, bool preview)
        {
            if (controlPoints.Count > 1)
            {
                List<Vector2> curvePoints = GameRenderer.GenerateBezierCurve(controlPoints.ToArray(), Settings.Bezier.Resolution).ToList();
                if(!preview) game.Track.UndoManager.BeginAction();
                for (int i = 1; i < curvePoints.Count; i++)
                {
                    Vector2d _start = (Vector2d)curvePoints[i - 1];
                    Vector2d _end = (Vector2d)curvePoints[i];
                    if ((_end - _start).Length >= MINIMUM_LINE)
                    {
                        var added = CreateLine(trk, _start, _end, _addflip, Snapped, EnableSnap,
                            Swatch.Selected, Swatch.RedMultiplier, Swatch.GreenMultiplier);
                        workingLines.Add(added);
                    }
                }
                game.Track.NotifyTrackChanged();
                if (!preview) game.Track.UndoManager.EndAction();
            }
            game.Invalidate();
        }
        private void DeleteLines()
        {
            using (var trk = game.Track.CreateTrackWriter())
            {
                trk.DisableUndo();
                if (workingLines.Count() == 0)
                    return;
                foreach (GameLine line in workingLines)
                {
                    trk.RemoveLine(line);
                }
                workingLines.Clear();
                game.Track.Invalidate();
                game.Track.NotifyTrackChanged();
            }
        }
        public override void Cancel()
        {
            Stop();
        }
        public override void Stop()
        {
            Active = false;
            controlPoints.Clear();
            DeleteLines();
        }
    }
}