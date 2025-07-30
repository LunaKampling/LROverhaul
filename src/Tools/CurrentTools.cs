namespace linerider.Tools
{
    public static class CurrentTools
    {
        public static PencilTool PencilTool { get; private set; }
        public static SmoothPencilTool SmoothPencilTool { get; private set; }
        public static EraserTool EraserTool { get; private set; }
        public static LineTool LineTool { get; private set; }
        public static BezierTool BezierTool { get; private set; }
        public static SelectTool SelectTool { get; private set; }
        public static SelectSubtool SelectSubtool { get; private set; }
        public static PanTool PanTool { get; private set; }
        public static Tool _current;
        public static Tool CurrentTool => _quickpan ? PanTool : _current;
        private static bool _quickpan = false;
        public static bool QuickPan
        {
            get => _quickpan;
            set
            {
                if (value != _quickpan)
                {
                    if (value == false)
                    {
                        PanTool.Stop();
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
            LineTool = new LineTool();
            BezierTool = new BezierTool();
            EraserTool = new EraserTool();
            PanTool = new PanTool();
            SelectSubtool = new SelectSubtool();
            SelectTool = new SelectTool();
            _current = PencilTool;
        }
        public static void SetTool(Tool tool)
        {
            if (CurrentTool != null && tool != CurrentTool)
            {
                CurrentTool.Stop();
                CurrentTool.OnChangingTool();
            }

            if (tool == PanTool)
            {
                _current = PanTool;
                PanTool.Stop();
                _quickpan = false;
            }
            else if (tool == LineTool)
            {
                _current = LineTool;
            }
            else if (tool == BezierTool)
            {
                _current = BezierTool;
            }
            else if (tool == PencilTool)
            {
                _current = PencilTool;
            }
            else if (tool == SmoothPencilTool)
            {
                _current = SmoothPencilTool;
            }
            else if (tool == EraserTool)
            {
                if (CurrentTool == EraserTool)
                    EraserTool.Swatch.Selected = LineType.All;
                _current = EraserTool;
            }
            else if (tool == SelectTool)
            {
                if (CurrentTool == SelectTool)
                    SelectTool.Swatch.Selected = LineType.All;
                _current = SelectTool;
            }
            else if (tool == SelectSubtool)
            {
                if (CurrentTool == SelectSubtool)
                    SelectSubtool.Swatch.Selected = LineType.All;
                _current = SelectSubtool;
            }
        }
    }
}
