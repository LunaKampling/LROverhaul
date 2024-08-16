using Gwen;
using Gwen.Controls;
using Gwen.Skin.Texturing;
using linerider.IO;
using linerider.Utils;
using System.Drawing;

namespace linerider.UI.Components
{
    public class Separator : Panel
    {
        private Color _color => Utility.MixColors(Settings.Computed.BGColor, Settings.Computed.LineColor, 0.25f);
        public Separator(ControlBase parent, bool vertical = false) : base(parent)
        {
            MouseInputEnabled = false;

            if (vertical)
            {
                Margin = new Margin(WidgetContainer.WidgetItemSpacing, WidgetContainer.WidgetItemSpacing * 2, WidgetContainer.WidgetItemSpacing, WidgetContainer.WidgetItemSpacing * 2);
                Dock = Dock.Left;
                Width = 1;
            }
            else
            {
                Margin = new Margin(WidgetContainer.WidgetItemSpacing * 2, WidgetContainer.WidgetItemSpacing, WidgetContainer.WidgetItemSpacing * 2, WidgetContainer.WidgetItemSpacing);
                Dock = Dock.Top;
                Height = 1;
            }
        }
        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            skin.Renderer.DrawColor = _color;
            skin.Renderer.DrawFilledRect(new Rectangle(0, 0, Width, Height));
        }
    }
}