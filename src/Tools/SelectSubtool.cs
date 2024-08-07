using linerider.Game;
using linerider.Rendering;
using linerider.UI;
using linerider.Utils;
using Newtonsoft.Json.Linq;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace linerider.Tools
{
    // TODO the node attachment gets wonky when inverting scale/rotate.
    // Can be demonstrated by showing the hover while clicking.
    // Ideally, the node/square shuold be rotated
    public class SelectSubtool : Tool
    {
        private Layer selectLayer = new Layer();
        public override Swatch Swatch => SharedSwatches.EraserAndSelectToolSwatch;
        public override bool ShowSwatch => true;
        public SelectSubtool() : base()
        {
            Swatch.Selected = LineType.All;
        }
        public override MouseCursor Cursor => _hoverscale
                    ? _nodetop == _nodeleft ? game.Cursors.List[CursorsHandler.Type.SizeNWSE] : game.Cursors.List[CursorsHandler.Type.SizeSWNE]
                    : game.Cursors.List[CursorsHandler.Type.Select];
        private bool _hoverscale = false;
        private DoubleRect _boxstart;
        private DoubleRect _selectionbox = DoubleRect.Empty;
        private DoubleRect _startselectionedges = DoubleRect.Empty;
        private DoubleRect _drawbox = DoubleRect.Empty;
        private Vector2d _clickstart;
        private bool _movingselection = false;
        private readonly List<LineSelection> _selection = new List<LineSelection>();
        private readonly List<LineSelection> _boxselection = new List<LineSelection>();
        private readonly HashSet<int> _selectedlines = new HashSet<int>();
        private bool _drawingbox = false;
        private bool _movemade = false;
        private readonly List<GameLine> _copybuffer = new List<GameLine>();
        private GameLine _snapline = null;
        private bool _snapknob2 = false;
        private bool _snapknob1 = false;
        private GameLine _hoverline = null;
        private Vector2d _copyorigin;
        private bool _scaling = false;
        private bool _rotating = false;
        private bool _nodetop = false;
        private bool _nodeleft = false;

        public List<LineSelection> GetLineSelections() => _selection;
        public List<LineSelection> GetLineSelectionsInBox() => _boxselection;
        public void CancelSelection()
        {
            if (Active)
            {
                if (_drawingbox)
                {
                    Cancel();
                }
                else
                {
                    Stop();
                    DeferToMoveTool();
                }
            }
        }
        public override void OnUndoRedo(bool isundo, object undohint)
        {
            if (Active && (_selection.Count != 0 || _boxselection.Count != 0) &&
                (undohint is int[] lineids))
            {
                if (lineids.Length != 0)
                {
                    Stop(false);
                    _hoverline = null;
                    Active = true;
                    using (TrackWriter trk = game.Track.CreateTrackWriter())
                    {
                        foreach (int lineid in lineids)
                        {
                            GameLine line = trk.Track.LineLookup[lineid];
                            _ = _selectedlines.Add(line.ID);
                            LineSelection selection = new LineSelection(line, true, null);
                            _selection.Add(selection);
                            line.SelectionState = SelectionState.Selected;
                            game.Track.RedrawLine(line);
                        }
                    }
                    _selectionbox = GetBoxFromSelected(_selection);
                    return;
                }
            }
            Stop(true);
        }
        public override void OnMouseRightDown(Vector2d pos)
        {
            if (Active && _selection.Count != 0 && _movingselection == false)
            {
                Vector2d gamepos = ScreenToGameCoords(pos);
                if (!StartTransformSelection(gamepos, true))
                    Stop(true);
            }
            base.OnMouseRightDown(pos);
        }
        public override void OnMouseRightUp(Vector2d pos)
        {
            if (_movingselection & _rotating)
            {
                ReleaseSelection();
            }
            base.OnMouseRightUp(pos);
        }
        public override void OnMouseDown(Vector2d pos, bool nodraw)
        {
            Vector2d gamepos = ScreenToGameCoords(pos);
            if (Active && _selection.Count != 0)
            {
                if (StartTransformSelection(gamepos, false))
                {
                    return;
                }
                bool axis = InputUtils.CheckPressed(Hotkey.ToolAxisLock);
                bool perpendicularaxis = InputUtils.CheckPressed(Hotkey.ToolPerpendicularAxisLock);
                if ((axis || perpendicularaxis) && StartMoveSelection(gamepos))
                {
                    return;
                }
                else if (InputUtils.CheckPressed(Hotkey.ToolAddSelection))
                {
                    StartAddSelection(gamepos);
                    return;
                }
                else if (InputUtils.CheckPressed(Hotkey.ToolToggleSelection))
                {
                    _ = ToggleSelection(gamepos);
                    return;
                }
                else if (!(axis || perpendicularaxis) && StartMoveSelection(gamepos))
                {
                    return;
                }
            }
            Unselect();
            _selectionbox = DoubleRect.Empty;
            _drawbox = new DoubleRect(gamepos, Vector2d.Zero);
            Active = true;
            _drawingbox = true;
            _movingselection = false;
            base.OnMouseDown(pos, nodraw);
        }
        public override void OnMouseMoved(Vector2d pos)
        {
            Vector2d gamepos = ScreenToGameCoords(pos);
            _hoverscale = false;
            if (Active && _drawingbox)
            {
                UpdateDrawingBox(gamepos);
            }
            else if (Active && _movingselection)
            {
                if (_scaling)
                    ScaleSelection(gamepos);
                else if (_rotating)
                    RotateSelection(gamepos);
                else
                    MoveSelection(gamepos);
            }
            else if (Active)
            {
                UpdateHover(gamepos);
            }
            base.OnMouseMoved(pos);
        }
        public override void OnMouseUp(Vector2d pos)
        {
            if (_drawingbox)
            {
                _drawingbox = false;
                if (_selection.Count == 0 && _boxselection.Count == 0)
                {
                    Stop(true);
                }
                else
                {
                    TrackWriter trk = game.Track.CreateTrackWriter();
                    foreach (LineSelection v in _boxselection)
                    {
                        if (Settings.Editor.NoHitSelect && game.Track.Timeline.IsLineHit(v.line.ID)) continue;
                        
                        LineType linetypeSelected = v.GetLineType();
                        if (v.line.layer.GetVisibility() && !v.line.layer.GetLock() &&
                            (v.line.Type == Swatch.Selected || Swatch.Selected == LineType.All ||
                            (Swatch.Selected == LineType.Layer && v.line.layer == trk.Track._layers.currentLayer)))
                        {
                            _ = _selectedlines.Add(v.line.ID);
                            _selection.Add(v);
                        }
                    }
                    _selectionbox = GetBoxFromSelected(_selection);
                    _boxselection.Clear();
                }
                _drawbox = DoubleRect.Empty;
            }
            else
            {
                ReleaseSelection();
            }
            base.OnMouseUp(pos);
        }
        private void UpdateDrawingBox(Vector2d gamepos)
        {
            Vector2d size = gamepos - _drawbox.Vector;
            _drawbox.Size = size;
            using (TrackWriter trk = game.Track.CreateTrackWriter())
            {
                IEnumerable<GameLine> lines = trk.GetLinesInRect(_drawbox.MakeLRTB(), true);
                UnselectOnUpdate(lines);
                foreach (GameLine line in lines)
                {
                    if (!_selectedlines.Contains(line.ID))
                    {
                        if (Settings.Editor.NoHitSelect && game.Track.Timeline.IsLineHit(line.ID)) continue;

                        LineSelection selection = new LineSelection(line, true, null);
                        if (line.layer.GetVisibility() && !line.layer.GetLock() &&
                            (line.Type == Swatch.Selected || Swatch.Selected == LineType.All || 
                            (Swatch.Selected == LineType.Layer && line.layer == trk.Track._layers.currentLayer)) && line.SelectionState == SelectionState.None)
                        {
                            line.SelectionState = SelectionState.Selected;
                            _boxselection.Add(selection);
                            game.Track.RedrawLine(line);

                        }
                    }
                }
            }
        }
        private void UpdateHover(Vector2d gamepos)
        {
            _hoverline = null;
            using (TrackReader trk = game.Track.CreateTrackReader())
            {
                GameLine selected = InputUtils.CheckPressed(Hotkey.ToolToggleSelection)
                    ? SelectLine(trk, gamepos, out _)
                    : (SelectInSelection(trk, gamepos)?.line);
                if (selected != null)
                {
                    _hoverline = selected;
                }
            }
            if (CornerContains(gamepos, _nodetop = true, _nodeleft = true) ||
                CornerContains(gamepos, _nodetop = true, _nodeleft = false) ||
                CornerContains(gamepos, _nodetop = false, _nodeleft = false) ||
                CornerContains(gamepos, _nodetop = false, _nodeleft = true))
            {
                _hoverscale = true;
            }
        }
        private void ReleaseSelection()
        {
            if (_movingselection)
            {
                _snapline = null;
                _movingselection = false;
                _snapknob1 = false;
                _snapknob2 = false;
                _rotating = false;
                _scaling = false;
                SaveMovedSelection();
                foreach (LineSelection selected in _selection)
                {
                    selected.clone = selected.line.Clone();
                }
            }
        }
        public override void OnChangingTool() => Stop(false);

        public override void Cancel()
        {
            _hoverscale = false;
            _ = CancelDrawBox();
            UnselectBox();
            if (Active)
            {
                ReleaseSelection();
            }
        }
        public override void Stop()
        {
            if (Active)
            {
                Stop(true);
            }
        }
        private void Stop(bool defertomovetool)
        {
            if (Active)
            {
                SaveMovedSelection();
            }
            Active = false;
            UnselectBox();
            Unselect();
            _ = CancelDrawBox();
            _selectionbox = DoubleRect.Empty;
            _movingselection = false;
            _movemade = false;
            _snapline = null;
            _hoverline = null;
            _snapknob1 = false;
            _snapknob2 = false;
            _scaling = false;
            _rotating = false;
            _hoverscale = false;
            if (defertomovetool)
                DeferToMoveTool();
        }
        private DoubleRect GetCorner(DoubleRect box, bool top, bool left, int boxsize = 15)
        {
            DoubleRect corner = box;
            corner.Width = boxsize / game.Track.Zoom;
            corner.Height = boxsize / game.Track.Zoom;
            if (!left)
            {
                corner.Left = box.Right;
            }
            else
            {
                corner.Left -= corner.Width;
            }
            if (!top)
            {
                corner.Top = box.Bottom;
            }
            else
            {
                corner.Top -= corner.Height;
            }
            return corner;
        }
        private void RenderFilledCorner(DoubleRect box, bool top, bool left)
        {
            DoubleRect corner = GetCorner(box, top, left);
            DoubleRect rect = new DoubleRect(
                GameDrawingMatrix.ScreenCoordD(corner.Vector),
                new Vector2d(corner.Width * game.Track.Zoom,
                corner.Height * game.Track.Zoom));
            GameRenderer.RenderRoundedRectangle(rect, Color.CornflowerBlue, 5, false);
        }
        private void RenderCorner(DoubleRect box, bool top, bool left)
        {
            DoubleRect corner = GetCorner(box, top, left);
            DoubleRect rect = new DoubleRect(
                GameDrawingMatrix.ScreenCoordD(corner.Vector),
                new Vector2d(corner.Width * game.Track.Zoom,
                corner.Height * game.Track.Zoom));
            GameRenderer.RenderRoundedRectangle(rect, Color.CornflowerBlue, 2, false);
        }
        public override void Render(Layer layer)
        {
            if (Active)
            {
                Color color = Color.FromArgb(255, 0x00, 0x77, 0xcc);
                if (_selectionbox != DoubleRect.Empty)
                {
                    GameRenderer.RenderRoundedRectangle(_selectionbox, color, 2f / game.Track.Zoom);
                    RenderCorner(_selectionbox, true, true);
                    RenderCorner(_selectionbox, false, false);
                    RenderCorner(_selectionbox, true, false);
                    RenderCorner(_selectionbox, false, true);
                    if (_hoverscale)
                        RenderFilledCorner(_selectionbox, _nodetop, _nodeleft);
                }
                if (_drawbox != DoubleRect.Empty)
                    GameRenderer.RenderRoundedRectangle(_drawbox, color, 2f / game.Track.Zoom);
                if (_hoverline != null)
                {
                    GameRenderer.RenderRoundedLine(_hoverline.Position1, _hoverline.Position2, Color.FromArgb(127, Settings.Computed.BGColor), _hoverline.Width * 2 * 0.8f);

                    GameRenderer.DrawKnob(_hoverline.Position1, _snapknob1, false, _hoverline.Width, _snapknob1 && !_snapknob2 ? 1 : 0);
                    GameRenderer.DrawKnob(_hoverline.Position2, _snapknob2, false, _hoverline.Width, _snapknob2 && !_snapknob1 ? 1 : 0);
                }
            }
            base.Render(layer);
        }
        public bool CancelDrawBox()
        {
            if (_drawingbox)
            {
                UnselectBox();
                _drawingbox = false;
                _drawbox = DoubleRect.Empty;
                return true;
            }
            return false;
        }
        public void Delete()
        {
            if (!Active || _drawingbox || _selection.Count == 0)
                return;

            using (TrackWriter trk = game.Track.CreateTrackWriter())
            {
                game.Track.UndoManager.BeginAction();
                foreach (int selected in _selectedlines)
                {
                    GameLine line = trk.Track.LineLookup[selected];
                    line.SelectionState = SelectionState.None;
                    trk.RemoveLine(line);
                }
                game.Track.UndoManager.EndAction();
                trk.NotifyTrackChanged();
            }
            _selection.Clear();
            _selectedlines.Clear();
            Stop();
        }
        public void Cut()
        {
            if (!Active || _drawingbox || _selection.Count == 0)
                return;

            Copy();
            Delete();
        }
        public void Copy()
        {
            if (!Active || _drawingbox || _selection.Count == 0)
                return;
            _copybuffer.Clear();

            foreach (LineSelection selected in _selection)
            {
                _copybuffer.Add(selected.line.Clone());
            }
            _copyorigin = GetCopyOrigin();
        }
        public void Paste() => PasteFromBuffer(_copybuffer);
        private void PasteFromBuffer(List<GameLine> buffer)
        {
            if (buffer.Count == 0)
                return;

            Stop(false);
            Vector2d pasteorigin = GetCopyOrigin();
            Vector2d diff = pasteorigin - _copyorigin;
            Unselect();
            Active = true;
            if (CurrentTools.CurrentTool != this)
            {
                CurrentTools.SetTool(this);
            }
            using (TrackWriter trk = game.Track.CreateTrackWriter())
            {
                game.Track.UndoManager.BeginAction();
                foreach (GameLine line in buffer)
                {
                    GameLine add = line.Clone();
                    add.ID = GameLine.UninitializedID;
                    add.Position1 += diff;
                    add.Position2 += diff;
                    add.layer = trk.Track._layers.currentLayer;
                    if (add is StandardLine stl)
                        stl.CalculateConstants();
                    add.SelectionState = SelectionState.Selected;
                    trk.AddLine(add, trk.Track._layers.currentLayer); //Adds lines on the current layer. .com does it the same way.
                    LineSelection selectinfo = new LineSelection(add, true, null);
                    _selection.Add(selectinfo);
                    _ = _selectedlines.Add(add.ID);
                }
                game.Track.UndoManager.EndAction();
            }
            _selectionbox = GetBoxFromSelected(_selection);
            game.Track.NotifyTrackChanged();
        }
        public void CopyValues()
        {
            if (!Active || _drawingbox || _selection.Count == 0)
                return;

            StringBuilder lineArray = new StringBuilder("[");

            Vector2d offset = GetCopyOrigin();

            for (int i = 0; i < _selection.Count; i++)
            {
                GameLine line = _selection[i].line.Clone();
                line.Position1 -= offset;
                line.Position2 -= offset;
                _ = lineArray.Append(line.ToString());

                if (i < _selection.Count - 1)
                {
                    _ = lineArray.Append(",");
                }
            }

            Clipboard.SetText(lineArray.ToString() + "]");

            game.Track.Notify("Copied!");
        }
        public void PasteValues(Layer layer)
        {
            string lineArray = Clipboard.GetText();
            List<GameLine> lineList = ParseJson(lineArray, layer);

            if (lineList is null)
                return;

            PasteFromBuffer(lineList);
        }
        private List<GameLine> ParseJson(string lineArray, Layer layer)
        {
            try
            {
                JArray parsedLineArray = JArray.Parse(lineArray);

                List<GameLine> deserializedLines = new List<GameLine>();

                foreach (JToken line in parsedLineArray)
                {
                    JObject inner = line.Value<JObject>();

                    GameLine newLine = null;

                    Vector2d pos1 = new Vector2d((double)inner["x1"], (double)inner["y1"]);
                    Vector2d pos2 = new Vector2d((double)inner["x2"], (double)inner["y2"]);
                    bool flipped = inner.ContainsKey("flipped") && (bool)inner["flipped"];
                    bool left = inner.ContainsKey("leftExtended") && (bool)inner["leftExtended"];
                    bool right = inner.ContainsKey("rightExtended") && (bool)inner["rightExtended"];
                    StandardLine.Ext extension =
                        left && right ? StandardLine.Ext.Both :
                        left ? StandardLine.Ext.Left :
                        right ? StandardLine.Ext.Right :
                        StandardLine.Ext.None;
                    switch ((int)inner["type"])
                    {
                        case 0:
                        {
                            newLine = new StandardLine(layer, pos1, pos2, flipped)
                            { Extension = extension };
                            break;
                        }
                        case 1:
                        {
                            int multiplier = inner.ContainsKey("multiplier") ? (int)inner["multiplier"] : 1;
                            newLine = new RedLine(layer, pos1, pos2)
                            { Multiplier = multiplier };
                            break;
                        }
                        case 2:
                        {
                            float width = inner.ContainsKey("width") ? (float)inner["width"] : 1f;
                            newLine = new SceneryLine(layer, pos1, pos2)
                            { Width = width };
                            break;
                        }
                        default:
                            throw new Exception("Unknown line type");
                    }

                    deserializedLines.Add(newLine);
                }

                return deserializedLines;
            }
            catch
            {
                game.Track.Notify("Failed to Paste");
                return null;
            }
        }
        public void SwitchLineType(LineType type)
        {
            if (!Active || _drawingbox || _selection.Count == 0)
                return;

            using (TrackWriter trk = game.Track.CreateTrackWriter())
            {
                game.Track.UndoManager.BeginAction();

                List<GameLine> buffer = new List<GameLine>();

                foreach (int selected in _selectedlines)
                {
                    GameLine line = trk.Track.LineLookup[selected];
                    line.SelectionState = SelectionState.None;
                    trk.RemoveLine(line);

                    buffer.Add(CreateLine(trk, line.Start, line.End, false, false, false, type, line.layer));
                }

                _selection.Clear();
                _selectedlines.Clear();
                Stop();

                game.Track.UndoManager.EndAction();
                trk.NotifyTrackChanged();
            }
        }
        private void StartAddSelection(Vector2d gamepos)
        {
            _movingselection = false;
            _drawbox = new DoubleRect(gamepos, Vector2d.Zero);
            _drawingbox = true;
        }
        private bool ToggleSelection(Vector2d gamepos)
        {
            using (TrackWriter trk = game.Track.CreateTrackWriter())
            {
                GameLine line = SelectLine(trk, gamepos, out bool knob);
                if (line != null)
                {
                    if (_selectedlines.Contains(line.ID))
                    {
                        _ = _selectedlines.Remove(line.ID);
                        _selection.RemoveAt(
                            _selection.FindIndex(
                                x => x.line.ID == line.ID));
                        line.SelectionState = SelectionState.None;
                    }
                    else
                    {
                        _ = _selectedlines.Add(line.ID);
                        LineSelection selection = new LineSelection(line, true, null);
                        _selection.Add(selection);
                        line.SelectionState = SelectionState.Selected;
                    }
                    _selectionbox = GetBoxFromSelected(_selection);
                    game.Track.RedrawLine(line);
                    return true;
                }
            }
            return false;
        }
        /// <param name="rotate">
        /// If true rotates selection, if false scales selection
        /// </param>
        private bool StartTransformSelection(Vector2d gamepos, bool rotate)
        {
            if (CornerContains(gamepos, _nodetop = true, _nodeleft = true) ||
                CornerContains(gamepos, _nodetop = true, _nodeleft = false) ||
                CornerContains(gamepos, _nodetop = false, _nodeleft = false) ||
                CornerContains(gamepos, _nodetop = false, _nodeleft = true))
            {
                if (rotate)
                {
                    _rotating = true;
                }
                else
                {
                    _scaling = true;
                }
                _movingselection = true;
                _clickstart = gamepos;
                _boxstart = _selectionbox;
                _startselectionedges = GetBoxLineEdges(_selection);
                return true;
            }
            return false;
        }
        private bool StartMoveSelection(Vector2d gamepos)
        {
            if (_selectionbox.Contains(gamepos.X, gamepos.Y))
            {
                using (TrackReader trk = game.Track.CreateTrackReader())
                {
                    LineSelection selected = SelectInSelection(trk, gamepos);
                    if (selected != null)
                    {
                        bool snapped = IsLineSnappedByKnob(trk, gamepos, selected.clone, out bool knob1);

                        _snapknob1 = !snapped || knob1;
                        _snapknob2 = !snapped || !knob1;

                        _snapline = selected.clone;
                        _clickstart = gamepos;
                        _boxstart = _selectionbox;
                        _movingselection = true;
                        return true;
                    }
                }
            }
            return false;
        }
        private bool CornerContains(Vector2d gamepos, bool top, bool left)
        {
            DoubleRect corner = GetCorner(_selectionbox, top, left);
            bool ret = corner.Contains(gamepos.X, gamepos.Y);
            return ret;
        }
        /// <summary>
        /// Returns selection box without calculating line widths
        /// </summary>
        private DoubleRect GetBoxLineEdges(List<LineSelection> selected)
        {
            if (selected == null || selected.Count == 0)
                return DoubleRect.Empty;
            Vector2d tl = selected[0].line.Position1;
            Vector2d br = selected[0].line.Position1;
            for (int i = 0; i < selected.Count; i++)
            {
                GameLine sel = selected[i].line;
                Vector2d p1 = sel.Position1;
                Vector2d p2 = sel.Position2;
                tl.X = Math.Min(tl.X, Math.Min(p1.X, p2.X));
                tl.Y = Math.Min(tl.Y, Math.Min(p1.Y, p2.Y));
                br.X = Math.Max(br.X, Math.Max(p1.X, p2.X));
                br.Y = Math.Max(br.Y, Math.Max(p1.Y, p2.Y));
            }
            return new DoubleRect(tl, br - tl);
        }
        private DoubleRect GetBoxFromSelected(List<LineSelection> selected)
        {
            if (selected == null || selected.Count == 0)
                return DoubleRect.Empty;
            Vector2d tl = selected[0].line.Position1;
            Vector2d br = selected[0].line.Position1;
            for (int i = 0; i < selected.Count; i++)
            {
                GameLine sel = selected[i].line;
                Vector2d p1 = sel.Position1;
                Vector2d p2 = sel.Position2;
                tl.X = Math.Min(tl.X, Math.Min(p1.X, p2.X) - sel.Width);
                tl.Y = Math.Min(tl.Y, Math.Min(p1.Y, p2.Y) - sel.Width);

                br.X = Math.Max(br.X, Math.Max(p1.X, p2.X) + sel.Width);
                br.Y = Math.Max(br.Y, Math.Max(p1.Y, p2.Y) + sel.Width);
            }
            return new DoubleRect(tl, br - tl);
        }
        private Vector2d GetSnapOffset(Vector2d movediff, TrackReader trk)
        {
            Vector2d snapoffset = Vector2d.Zero;
            double distance = -1;
            void checklines(GameLine[] lines, Vector2d snap)
            {
                foreach (GameLine line in lines)
                {
                    if (!_selectedlines.Contains(line.ID))
                    {
                        Vector2d closer = Utility.CloserPoint(snap, line.Position1, line.Position2);
                        Vector2d diff = closer - snap;
                        double dist = diff.Length;
                        if (distance == -1 || dist < distance)
                        {
                            snapoffset = diff;
                            distance = dist;
                        }
                    }
                }
            }
            if (_snapknob1)
            {
                Vector2d snap1 = _snapline.Position1 + movediff;
                GameLine[] lines1 = LineEndsInRadius(trk, snap1, SnapRadius);
                checklines(lines1, snap1);
            }
            if (_snapknob2)
            {
                Vector2d snap2 = _snapline.Position2 + movediff;
                GameLine[] lines2 = LineEndsInRadius(trk, snap2, SnapRadius);
                checklines(lines2, snap2);
            }

            return snapoffset;
        }
        private void ScaleSelection(Vector2d pos)
        {
            if (_selection.Count > 0)
            {
                _movemade = true;
                Vector2d movediff = pos - _clickstart;
                if (InputUtils.CheckPressed(Hotkey.ToolScaleAspectRatio))
                {
                    movediff.Y = _nodeleft == _nodetop
                        ? movediff.X * (_boxstart.Height / _boxstart.Width)
                        : -(movediff.X * (_boxstart.Height / _boxstart.Width));
                }
                using (TrackWriter trk = game.Track.CreateTrackWriter())
                {
                    trk.DisableUndo();

                    foreach (LineSelection selected in _selection)
                    {
                        Vector2d scalar1 = Vector2d.Divide(
                            selected.clone.Position1 - _startselectionedges.Vector,
                            _startselectionedges.Size);

                        Vector2d scalar2 = Vector2d.Divide(
                            selected.clone.Position2 - _startselectionedges.Vector,
                             _startselectionedges.Size);
                        if (_nodetop)
                        {
                            scalar1.Y = 1 - scalar1.Y;
                            scalar2.Y = 1 - scalar2.Y;
                        }
                        if (_nodeleft)
                        {
                            scalar1.X = 1 - scalar1.X;
                            scalar2.X = 1 - scalar2.X;
                        }
                        if (_startselectionedges.Size.X == 0)
                        {
                            scalar1.X = 0;
                            scalar2.X = 0;
                        }
                        if (_startselectionedges.Size.Y == 0)
                        {
                            scalar1.Y = 0;
                            scalar2.Y = 0;
                        }
                        Vector2d p1 = Vector2d.Multiply(scalar1, movediff);
                        Vector2d p2 = Vector2d.Multiply(scalar2, movediff);

                        trk.MoveLine(
                            selected.line,
                            selected.clone.Position1 + p1,
                            selected.clone.Position2 + p2);
                    }
                    _selectionbox = GetBoxFromSelected(_selection);
                    game.Track.NotifyTrackChanged();
                }
            }
            game.Invalidate();
        }
        private void RotateSelection(Vector2d pos)
        {
            if (_selection.Count > 0)
            {
                _movemade = true;
                _ = pos - _clickstart;
                using (TrackWriter trk = game.Track.CreateTrackWriter())
                {
                    trk.DisableUndo();
                    Vector2d center = _startselectionedges.Vector +
                        _startselectionedges.Size / 2;
                    Vector2d edge = new Vector2d(_nodeleft
                        ? _startselectionedges.Left
                        : _startselectionedges.Right,
                        _nodetop
                        ? _startselectionedges.Top
                        : _startselectionedges.Bottom);

                    Angle angle = Angle.FromVector(edge - center);
                    Angle newangle = Angle.FromVector(pos - center);
                    Angle anglediff = newangle - angle;
                    if (game.ShouldXySnap())
                        anglediff = new Angle(Math.Round(anglediff.Degrees / Settings.Editor.XySnapDegrees) * Settings.Editor.XySnapDegrees);
                    foreach (LineSelection selected in _selection)
                    {
                        Vector2d p1 = Utility.Rotate(selected.clone.Position1, center, anglediff);
                        Vector2d p2 = Utility.Rotate(selected.clone.Position2, center, anglediff);
                        trk.MoveLine(selected.line, p1, p2);
                    }
                    _selectionbox = GetBoxFromSelected(_selection);
                    game.Track.NotifyTrackChanged();
                }
            }
            game.Invalidate();
        }

        private bool ApplyModifiers(Vector2d joint1, ref Vector2d joint2)
        {
            bool axis = InputUtils.CheckPressed(Hotkey.ToolAxisLock);
            bool perpendicularaxis = InputUtils.CheckPressed(Hotkey.ToolPerpendicularAxisLock);
            bool modified = false;
            if (axis || perpendicularaxis)
            {
                Angle angle = Angle.FromVector(_snapline.GetVector());
                if (perpendicularaxis)
                {
                    angle.Degrees -= 90;
                }
                joint2 = Utility.AngleLock(joint1, joint2, angle);
                modified = true;
            }
            return modified;
        }
        private void MoveSelection(Vector2d pos)
        {
            if (_selection.Count > 0)
            {
                _movemade = true;
                Vector2d movediff = pos - _clickstart;
                using (TrackWriter trk = game.Track.CreateTrackWriter())
                {
                    Vector2d pos2 = pos;
                    if (ApplyModifiers(_clickstart, ref pos2))
                    {
                        movediff = pos2 - _clickstart;
                    }
                    else if (_snapline != null && EnableSnap)
                    {
                        movediff += GetSnapOffset(movediff, trk);
                    }
                    _selectionbox.Vector = _boxstart.Vector + movediff;
                    foreach (LineSelection selected in _selection)
                    {
                        trk.DisableUndo();
                        trk.MoveLine(
                            selected.line,
                            selected.clone.Position1 + movediff,
                            selected.clone.Position2 + movediff);
                    }
                    game.Track.NotifyTrackChanged();
                }
            }
            game.Invalidate();
        }
        public void UnselectBox()
        {
            if (_boxselection.Count != 0)
            {
                using (TrackWriter trk = game.Track.CreateTrackWriter())
                {
                    foreach (LineSelection sel in _boxselection)
                    {
                        if (!_selectedlines.Contains(sel.line.ID))
                        {
                            if (sel.line.SelectionState != SelectionState.None)
                            {
                                sel.line.SelectionState = SelectionState.None;
                                game.Track.RedrawLine(sel.line);
                            }
                        }
                    }
                }
                _boxselection.Clear();
            }
        }
        /// <summary>
        /// Unselects every line that isn't in the current selection
        /// Only gets called when the drawing box is updated
        /// </summary>
        /// <param name="lines">list of lines in the current selection</param>
        public void UnselectOnUpdate(IEnumerable<GameLine> selection)
        {
            if (_boxselection.Count != 0)
            {
                using (TrackWriter trk = game.Track.CreateTrackWriter())
                {
                    HashSet<int> newSelection = new HashSet<int>();
                    foreach (GameLine line in selection) 
                    {
                        newSelection.Add(line.ID);
                    }
                    HashSet<LineSelection> unselected = new HashSet<LineSelection>();
                    foreach (LineSelection sel in _boxselection)
                    {
                        if (!newSelection.Contains(sel.line.ID))
                        {
                            if (!_selectedlines.Contains(sel.line.ID))
                            {
                                if (sel.line.SelectionState != SelectionState.None)
                                {
                                    sel.line.SelectionState = SelectionState.None;
                                    game.Track.RedrawLine(sel.line);
                                    unselected.Add(sel);
                                }
                            }
                        }
                    }
                    foreach (LineSelection sel in unselected)
                    {
                        _boxselection.Remove(sel);
                    }
                }
            }
        }
        public void Unselect()
        {
            if (_selection.Count != 0)
            {
                using (TrackWriter trk = game.Track.CreateTrackWriter())
                {
                    Dictionary<int, GameLine> lookup = trk.Track.LineLookup;
                    foreach (LineSelection sel in _selection)
                    {
                        // Prefer the 'real' line, if the track state changed
                        // our sel.line could be out of sync
                        if (lookup.TryGetValue(sel.line.ID, out GameLine line))
                        {
                            if (line.SelectionState != SelectionState.None)
                            {
                                line.SelectionState = SelectionState.None; 
                                game.Track.RedrawLine(line);
                            }
                        }
                    }
                    _selection.Clear();
                    _selectedlines.Clear();
                }
            }
            _movemade = false;
        }

        public List<LineSelection> SelectLines(List<GameLine> lines, bool additive = false)
        {
            if (!additive)
            {
                Unselect();
                UnselectBox();
            }
            foreach (GameLine line in lines)
            {
                LineSelection selection = new LineSelection(line, true, null);
                _ = _selectedlines.Add(line.ID);
                _selection.Add(selection);
                _boxselection.Add(selection);

                line.SelectionState = SelectionState.Selected;
                game.Track.RedrawLine(line);
            }
            _selectionbox = GetBoxFromSelected(_selection);

            Render(selectLayer);

            return _selection;
        }

        private void SaveMovedSelection()
        {
            if (Active)
            {
                if (_selection.Count != 0 && _movemade)
                {
                    game.Track.UndoManager.BeginAction();
                    game.Track.UndoManager.SetActionUserHint(_selectedlines.ToArray());
                    foreach (LineSelection selected in _selection)
                    {
                        game.Track.UndoManager.AddChange(selected.clone, selected.line);
                    }
                    game.Track.UndoManager.EndAction();
                    _movemade = false;
                }
                game.Invalidate();
            }
        }
        private void DeferToMoveTool() => CurrentTools.SetTool(CurrentTools.SelectTool);
        private Vector2d GetCopyOrigin() => game.Track.Camera.GetCenter();
        /// <summary>
        /// Return the line (if any) in the point that we've selected
        /// </summary>
        private LineSelection SelectInSelection(TrackReader trk, Vector2d gamepos)
        {
            foreach (GameLine line in SelectLines(trk, gamepos))
            {
                if (_selectedlines.Contains(line.ID))
                {
                    foreach (LineSelection s in _selection)
                    {
                        if (s.line.ID == line.ID)
                        {
                            return s;
                        }
                    }
                }
            }
            return null;
        }
    }
}