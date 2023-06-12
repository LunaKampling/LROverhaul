using Gwen.Controls;
using linerider.Tools;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using Gwen;

namespace linerider.UI.Components
{
    public class MultiToolButton : WidgetButton
    {
        private int _currentidx = 0;
        private List<ToolButton> _buttons = new List<ToolButton>();
        private Texture _indicatortexture;
        private ToolButton _currentbutton
        {
            get => _buttons[_currentidx];
        }

        public MultiToolButton(ControlBase parent, Tool[] tools) : this(parent, tools, Hotkey.None)
        { }
        public MultiToolButton(ControlBase parent, Tool[] tools, Hotkey hotkey) : base(parent)
        {
            Texture tx = new Texture(parent.Skin.Renderer);
            Gwen.Renderer.OpenTK.LoadTextureInternal(tx, GameResources.icon_multitool_indicator.Bitmap);
            _indicatortexture = tx;

            foreach (Tool tool in tools)
            {
                ToolButton toolButton = new ToolButton(this, tool, false)
                {
                    Action = (o, e) => SelectNextTool(),
                    IsHidden = true,
                };
                _buttons.Add(toolButton);
            }

            _currentbutton.IsHidden = false;

            if (hotkey != Hotkey.None)
            {
                Action = (o, e) => SelectNextTool();
                Hotkey = hotkey;
            }

            Width = _buttons[0].Width;
            Height = _buttons[0].Height;
        }
        private void SelectNextTool()
        {
            bool shouldSetNextTool = CurrentTools._current == _currentbutton.Tool;

            if (shouldSetNextTool)
            {
                int nextIdx = _currentidx + 1;
                if (nextIdx == _buttons.Count)
                    nextIdx = 0;

                ToolButton nextbutton = _buttons[nextIdx];

                _currentbutton.IsHidden = true;
                nextbutton.IsHidden = false;

                CurrentTools.SetTool(nextbutton.Tool);
                _currentidx = nextIdx;
            }
            else
            {
                CurrentTools.SetTool(_currentbutton.Tool);
            }
        }
        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            bool drawIndicator = CurrentTools._current == _currentbutton.Tool && !CurrentTools.QuickPan;
            if (!drawIndicator)
                return;

            Color color = Settings.Computed.LineColor;
            if (_currentbutton.Tool.ShowSwatch)
            {
                switch (_currentbutton.Tool.Swatch.Selected)
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
