using Gwen.Controls;
using Gwen;
using linerider.Tools;
using System.Drawing;

namespace linerider.UI.Components
{
    public class ToolButton : WidgetButton
    {
        public Tool Tool;
        public Tool Subtool;
        private Texture ActiveToolBackground;

        public ToolButton(ControlBase parent, Tool tool) : this(parent, tool, true)
        { }
        public ToolButton(ControlBase parent, Tool tool, bool registerEvents) : base(parent)
        {
            Texture tx = new Texture(parent.Skin.Renderer);
            Gwen.Renderer.OpenTK.LoadTextureInternal(tx, GameResources.ux_tool_background.Bitmap);
            ActiveToolBackground = tx;

            Tool = tool;
            Name = tool.Name;

            if (registerEvents)
            {
                Action = (o, e) => CurrentTools.SetTool(Tool);
                Hotkey = tool.Hotkey;
            }

            SetImage(tool.Icon);
            SetSize(tool.Icon.Width, tool.Icon.Height);
        }
        public override string Tooltip
        {
            get
            {
                bool addHotkeyBrackets = !string.IsNullOrEmpty(Tool.Name);
                string tooltip = Tool.Name;

                if (Tool.Hotkey != Hotkey.None)
                    tooltip += Settings.HotkeyToString(Tool.Hotkey, addHotkeyBrackets);

                return tooltip;
            }
        }
        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            Color toolColor = Settings.Computed.LineColor;

            if (CurrentTools.CurrentTool == Tool || CurrentTools.CurrentTool == Subtool)
            {
                Color backgroundColor = toolColor;
                toolColor = Settings.NightMode ? Color.Black : Color.White;

                if (Tool.ShowSwatch)
                {
                    switch (Tool.Swatch.Selected)
                    {
                        case LineType.Scenery:
                            backgroundColor = Settings.Colors.SceneryLine;
                            break;
                        case LineType.Standard:
                            backgroundColor = Settings.Colors.StandardLine;
                            break;
                        case LineType.Acceleration:
                            backgroundColor = Settings.Colors.AccelerationLine;
                            break;
                    }
                    toolColor = Utility.IsColorDark(backgroundColor) ? Color.White : Color.Black;
                }

                skin.Renderer.DrawColor = Color.FromArgb(255, backgroundColor);
                skin.Renderer.DrawTexturedRect(ActiveToolBackground, RenderBounds);
                skin.Renderer.DrawColor = Color.FromArgb(255, toolColor);
                skin.Renderer.DrawTexturedRect(m_texture, RenderBounds);
            }
            else
            {
                base.Render(skin);
            }
        }
    }
}
