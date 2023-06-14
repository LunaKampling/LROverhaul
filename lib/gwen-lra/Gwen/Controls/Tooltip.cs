namespace Gwen.Controls
{
    public class Tooltip : Label
    {
        public Tooltip(ControlBase parent) : base(parent)
        {
            TextPadding = new Padding(5, 3, 5, 3);
            TextColorOverride = Skin.Colors.Text.Foreground;
            TextColor = Skin.Colors.Text.Foreground;
        }
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawToolTip(this);
            base.Render(skin);
        }
    }
}