using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Item in CollapsibleCategory.
    /// </summary>
    public class CategoryButton : Button
    {
        internal bool m_Alt; // For alternate coloring

        protected override System.Drawing.Color CurrentColor => m_Alt
                    ? IsDepressed || ToggleState ? Skin.Colors.Text.Contrast : IsHovered ? Skin.Colors.Text.Foreground : Skin.Colors.Text.Foreground
                    : IsDepressed || ToggleState ? Skin.Colors.Text.Contrast : IsHovered ? Skin.Colors.Text.Foreground : Skin.Colors.Text.Foreground;
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CategoryButton(ControlBase parent) : base(parent)
        {
            Alignment = Pos.Left | Pos.CenterV;
            m_Alt = false;
            IsToggle = true;
            TextPadding = new Padding(3, 2, 3, 2);
            ShouldDrawBackground = false;
            Dock = Dock.Top;
            Margin = new Margin(1, 0, 1, 0);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            Skin.Renderer.DrawColor = m_Alt
                ? IsDepressed || ToggleState ? skin.Colors.AccentHigh : IsHovered ? skin.Colors.AccentHigh : skin.Colors.BackgroundHighlight
                : IsDepressed || ToggleState ? skin.Colors.AccentHigh : IsHovered ? skin.Colors.AccentHigh : skin.Colors.Background;

            skin.Renderer.DrawFilledRect(RenderBounds);
            base.Render(Skin);
        }
    }
}
