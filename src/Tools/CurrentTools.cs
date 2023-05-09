using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace linerider.Tools
{
    public static class CurrentTools
    {
        public static PencilTool PencilTool { get; private set; }
        public static SmoothPencilTool SmoothPencilTool { get; private set; }
        public static EraserTool EraserTool { get; private set; }
        public static LineTool LineTool { get; private set; }
        public static BezierTool BezierTool { get; private set; } 
        public static MoveTool MoveTool{ get; private set; }
        public static SelectTool SelectTool{ get; private set; }
        public static HandTool HandTool { get; private set; }
        public static Tool _selected;
        public static Tool SelectedTool
        {
            get
            {
                if (_quickpan)
                {
                    return HandTool;
                }
                return _selected;
            }
        }
        private static bool _quickpan = false;
        public static bool QuickPan
        {
            get
            {
                return _quickpan;
            }
            set
            {
                if (value != _quickpan)
                {
                    if (value == false)
                    {
                        HandTool.Stop();
                    }
                    else
                    {
                        // SelectedTool.Stop();
                    }
                    _quickpan = value;
                }
            }
        }
        public static void Init()
        {
            PencilTool = new PencilTool();
            SmoothPencilTool = new SmoothPencilTool();
            EraserTool = new EraserTool();
            LineTool = new LineTool();
            HandTool = new HandTool();
            SelectTool = new SelectTool();
            MoveTool = new MoveTool();
            BezierTool = new BezierTool();
            _selected = PencilTool;
        }
        public static void SetTool(Tool tool)
        {
            if (SelectedTool != null && tool != SelectedTool)
            {
                SelectedTool.Stop();
                SelectedTool.OnChangingTool();
            }

            if (tool == CurrentTools.HandTool)
            {
                _selected = HandTool;
                HandTool.Stop();
                _quickpan = false;
            }
            else if (tool == CurrentTools.LineTool)
            {
                _selected = LineTool;
            }
            else if (tool == CurrentTools.BezierTool)
            {
                _selected = BezierTool;
            }
            else if (tool == CurrentTools.PencilTool)
            {
                _selected = PencilTool;
            }
            else if (tool == CurrentTools.SmoothPencilTool)
            {
                _selected = SmoothPencilTool;
            }
            else if (tool == CurrentTools.EraserTool)
            {
                if (SelectedTool == EraserTool)
                    EraserTool.Swatch.Selected = LineType.All;
                _selected = EraserTool;
            }
            else if (tool == CurrentTools.MoveTool)
            {
                if (SelectedTool == MoveTool)
                    MoveTool.Swatch.Selected = LineType.All;
                _selected = MoveTool;
            }
            else if (tool == CurrentTools.SelectTool)
            {
                if (SelectedTool == SelectTool)
                    SelectTool.Swatch.Selected = LineType.All;
                _selected = SelectTool;
            }
        }
    }
}
