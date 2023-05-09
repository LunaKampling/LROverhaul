using System;
using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// ComboBox arrow.
    /// </summary>
    public class DropDownArrow : Button
    {
        public bool IsOpen = false;
        public DropDownArrow(ControlBase parent)
            : base(parent)
        {
            AutoSizeToContents = false;
            SetSize(16, 16);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            base.Render(skin);
            skin.DrawDropDownArrow(this, IsHovered, IsDepressed, IsOpen, IsDisabled);
        }
    }
}
