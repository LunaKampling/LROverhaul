using System;
using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Item in CollapsibleCategory.
    /// </summary>
    public class CategoryButton : Button
    {
        internal bool m_Alt; // for alternate coloring

        protected override System.Drawing.Color CurrentColor
        {
            get
            {
                if (m_Alt)
                {
                    if (IsDepressed || ToggleState)
                    {
                        return Skin.Colors.Text.Contrast;
                    }
                    if (IsHovered)
                    {
                        return Skin.Colors.Text.Foreground;
                    }
                    return Skin.Colors.Text.Foreground;
                }

                if (IsDepressed || ToggleState)
                {
                    return Skin.Colors.Text.Contrast;
                }
                if (IsHovered)
                {
                    return Skin.Colors.Text.Foreground;
                }
                return Skin.Colors.Text.Foreground;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CategoryButton(Controls.ControlBase parent) : base(parent)
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
            if (m_Alt)
            {
                if (IsDepressed || ToggleState)
                    Skin.Renderer.DrawColor = skin.Colors.AccentHigh;
                else if (IsHovered)
                    Skin.Renderer.DrawColor = skin.Colors.AccentHigh;
                else
                    Skin.Renderer.DrawColor = skin.Colors.BackgroundHighlight;
            }
            else
            {
                if (IsDepressed || ToggleState)
                    Skin.Renderer.DrawColor = skin.Colors.AccentHigh;
                else if (IsHovered)
                    Skin.Renderer.DrawColor = skin.Colors.AccentHigh;
                else
                    Skin.Renderer.DrawColor = skin.Colors.Background;
            }

            skin.Renderer.DrawFilledRect(RenderBounds);
            base.Render(Skin);
        }
    }
}
