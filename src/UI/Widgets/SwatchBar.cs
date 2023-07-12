using Gwen;
using Gwen.Controls;
using linerider.Tools;
using linerider.UI.Components;
using System.Drawing;

namespace linerider.UI.Widgets
{
    public class SwatchBar : WidgetContainer
    {
        private readonly Editor _editor;

        private readonly Bitmap _normalTexture = GameResources.ux_swatch.Bitmap;
        private readonly Bitmap _activeTexture = GameResources.ux_swatch_active.Bitmap;
        private readonly WidgetButton _standardBtn;
        private readonly WidgetButton _accelerationBtn;
        private readonly WidgetButton _sceneryBtn;
        private readonly WidgetButton _allBtn;

        public SwatchBar(ControlBase parent, Editor editor) : base(parent)
        {
            GameCanvas canvas = (GameCanvas)GetCanvas();
            _editor = editor;
            ShouldDrawBackground = false;
            Margin = new Margin(0, WidgetMargin, 0, 0);
            Padding = Padding.Zero;

            _standardBtn = new WidgetButton(this)
            {
                Name = "Standard Line",
                Action = (o, e) => SwatchSelect(LineType.Standard),
                Hotkey = Hotkey.EditorToolColor1,
                HotkeyCondition = () => !_editor.Playing,
            };
            _accelerationBtn = new WidgetButton(this)
            {
                Name = "Acceleration Line",
                Font = canvas.Fonts.DefaultBold,
                TextRequest = (o, e) => TextRequest(LineType.Acceleration),
                Action = (o, e) => SwatchSelect(LineType.Acceleration),
                Hotkey = Hotkey.EditorToolColor2,
                HotkeyCondition = () => !_editor.Playing,
            };
            _sceneryBtn = new WidgetButton(this)
            {
                Name = "Scenery Line",
                Font = canvas.Fonts.DefaultBold,
                TextRequest = (o, e) => TextRequest(LineType.Scenery),
                Action = (o, e) => SwatchSelect(LineType.Scenery),
                Hotkey = Hotkey.EditorToolColor3,
                HotkeyCondition = () => !_editor.Playing,
            };
            _allBtn = new WidgetButton(this)
            {
                Name = "All Lines",
                Font = canvas.Fonts.DefaultBold,
                TextRequest = (o, e) => TextRequest(LineType.All),
                Action = (o, e) => SwatchSelect(LineType.All),
                Hotkey = Hotkey.EditorToolColor4,
                HotkeyCondition = () => !_editor.Playing,
            };

            _accelerationBtn.RightClicked += (o, e) => IncrementMultiplier(LineType.Acceleration);
            _sceneryBtn.RightClicked += (o, e) => IncrementMultiplier(LineType.Scenery);
        }
        public override void Think()
        {
            base.Think();

            bool colorsChanged = _allBtn.Color != Settings.Computed.LineColor
                || _standardBtn.Color != Settings.Colors.StandardLine
                || _accelerationBtn.Color != Settings.Colors.AccelerationLine
                || _sceneryBtn.Color != Settings.Colors.SceneryLine;
            if (colorsChanged)
            {
                _standardBtn.Color = Settings.Colors.StandardLine;
                _accelerationBtn.Color = Settings.Colors.AccelerationLine;
                _sceneryBtn.Color = Settings.Colors.SceneryLine;
                _allBtn.Color = Settings.Computed.LineColor;

                _accelerationBtn.TextColor = Utility.IsColorDark(Settings.Colors.AccelerationLine) ? Color.White : Color.Black;
                _sceneryBtn.TextColor = Utility.IsColorDark(Settings.Colors.SceneryLine) ? Color.White : Color.Black;
                _allBtn.TextColor = Utility.IsColorDark(Settings.Computed.LineColor) ? Color.White : Color.Black;
            }

            LineType currSwatch = CurrentTools.CurrentTool.Swatch.Selected;
            _standardBtn.Icon = currSwatch == LineType.Standard ? _activeTexture : _normalTexture;
            _accelerationBtn.Icon = currSwatch == LineType.Acceleration ? _activeTexture : _normalTexture;
            _sceneryBtn.Icon = currSwatch == LineType.Scenery ? _activeTexture : _normalTexture;
            _allBtn.Icon = currSwatch == LineType.All ? _activeTexture : _normalTexture;

            Tool tool = CurrentTools.CurrentTool;
            bool showAllButton = tool == CurrentTools.EraserTool || tool == CurrentTools.SelectTool || tool == CurrentTools.SelectSubtool;
            _allBtn.IsHidden = !showAllButton;
        }
        private string TextRequest(LineType linetype)
        {
            Tool tool = CurrentTools.CurrentTool;
            Swatch swatch = tool.Swatch;
            bool isEmpty = swatch.Selected != linetype;
            if (isEmpty)
                return "";

            string label = "";

            switch (linetype)
            {
                case LineType.Acceleration:
                    label = "\u00A0" + swatch.RedMultiplier.ToString(Program.Culture) + "\u00D7";
                    break;
                case LineType.Scenery:
                    label = "\u00A0" + swatch.GreenMultiplier.ToString(Program.Culture) + "\u00D7";
                    break;
                case LineType.All:
                    label = "All";
                    break;
            }

            return label;
        }
        private void SwatchSelect(LineType type)
        {
            Tool tool = CurrentTools.CurrentTool;
            Swatch swatch = tool.Swatch;
            bool canSwitch = type != LineType.All || (type == LineType.All && (tool == CurrentTools.EraserTool || tool == CurrentTools.SelectTool || tool == CurrentTools.SelectSubtool));
            if (!canSwitch)
                return;

            if (swatch != null)
                swatch.Selected = type;

            Invalidate();
        }
        public void IncrementMultiplier(LineType linetype)
        {
            Tool tool = CurrentTools.CurrentTool;
            Swatch swatch = tool.Swatch;
            bool canIncrement = swatch.Selected == linetype && tool != CurrentTools.EraserTool && tool != CurrentTools.SelectTool && tool != CurrentTools.SelectSubtool;

            if (canIncrement)
            {
                swatch.IncrementSelectedMultiplier();
                Invalidate();
            }
        }
    }
}
