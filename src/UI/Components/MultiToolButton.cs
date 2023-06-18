using Gwen;
using Gwen.Controls;
using linerider.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace linerider.UI.Components
{
    public class MultiToolButton : WidgetButton
    {
        private int _currentidx = 0;
        private readonly List<ToolButton> _buttons = new List<ToolButton>();
        private readonly Texture _indicatortexture;
        private ToolButton CurrentButton => _buttons[_currentidx];

        public new Hotkey Hotkey
        {
            get => base.Hotkey;
            set
            {
                base.Hotkey = value;
                foreach (ToolButton toolButton in _buttons)
                    toolButton.TooltipHotkey = value;
            }
        }

        public MultiToolButton(ControlBase parent, Tool[] tools) : base(parent)
        {
            Texture tx = new Texture(parent.Skin.Renderer);
            Gwen.Renderer.OpenTK.LoadTextureInternal(tx, GameResources.ux_multitool_indicator.Bitmap);
            _indicatortexture = tx;

            foreach (Tool tool in tools)
            {
                ToolButton toolButton = new ToolButton(this, tool)
                {
                    Action = (o, e) => SelectNextTool(),
                    IsHidden = true,
                };
                _buttons.Add(toolButton);
            }

            CurrentButton.IsHidden = false;
            Action = (o, e) => SelectNextTool();

            Width = _buttons[0].Width;
            Height = _buttons[0].Height;
        }
        private void SelectNextTool()
        {
            bool shouldSetNextTool = CurrentTools._current == CurrentButton.Tool;

            if (shouldSetNextTool)
            {
                int nextIdx = _currentidx + 1;
                if (nextIdx == _buttons.Count)
                    nextIdx = 0;

                ToolButton nextButton = _buttons[nextIdx];

                CurrentButton.IsHidden = true;
                nextButton.IsHidden = false;

                CurrentTools.SetTool(nextButton.Tool);
                _currentidx = nextIdx;
            }
            else
            {
                CurrentTools.SetTool(CurrentButton.Tool);
            }
        }
        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            bool drawIndicator = CurrentTools._current == CurrentButton.Tool && !CurrentTools.QuickPan;
            if (!drawIndicator)
                return;

            Color color = Settings.Computed.LineColor;
            if (CurrentButton.Tool.ShowSwatch)
            {
                switch (CurrentButton.Tool.Swatch.Selected)
                {
                    case LineType.Scenery:
                        color = Settings.Colors.SceneryLine;
                        break;
                    case LineType.Standard:
                        color = Settings.Colors.StandardLine;
                        break;
                    case LineType.Acceleration:
                        color = Settings.Colors.AccelerationLine;
                        break;
                }
            }

            Rectangle rect = new Rectangle(0, 0, _indicatortexture.Width, _indicatortexture.Height);
            int buttonWidth = RenderBounds.Width;
            int indicatorWidth = rect.Width;
            int indicatorsCount = _buttons.Count;
            int allIndicatorsWidth = indicatorWidth * indicatorsCount;
            int startPos = buttonWidth / 2 - allIndicatorsWidth / 2;

            for (int i = 0; i < indicatorsCount; i++)
            {
                int alpha = _currentidx == i ? 255 : 96;
                rect.X = startPos + indicatorWidth * i;
                skin.Renderer.DrawColor = Color.FromArgb(alpha, color);
                skin.Renderer.DrawTexturedRect(_indicatortexture, rect);
            }
        }
    }
}
