using Gwen;
using Gwen.Controls;
using linerider.Tools;

namespace linerider.UI.Components
{
    public class ToolButton : WidgetButton
    {
        public Tool Tool;
        public Tool Subtool;
        private readonly Texture ActiveToolBackground;

        public ToolButton(ControlBase parent, Tool tool) : base(parent)
        {
            Texture tx = new Texture(parent.Skin.Renderer);
            Gwen.Renderer.OpenTK.LoadTextureInternal(tx, GameResources.ux_tool_background.Bitmap);
            ActiveToolBackground = tx;

            Tool = tool;
            Name = tool.Name;
            Action = (o, e) => CurrentTools.SetTool(Tool);

            SetImage(tool.Icon);
            _ = SetSize(tool.Icon.Width, tool.Icon.Height);
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

        public override void Dispose()
        {
            m_texture?.Dispose();
            base.Dispose();
        }
    }
}
