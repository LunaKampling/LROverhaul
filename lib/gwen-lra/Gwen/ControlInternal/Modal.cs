using System;
using System.Drawing;
using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Modal control for windows.
    /// </summary>
    public class Modal : Controls.ControlBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Modal"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Modal(Controls.ControlBase parent)
            : base(parent)
        {
            KeyboardInputEnabled = true;
            MouseInputEnabled = true;
            ShouldDrawBackground = true;
            Invalidate();
        }
        /// <summary>
        /// Recursively lays out the control's interior according to alignment, margin, padding, dock etc.
        /// If AutoSizeToContents is enabled, sizes the control before layout.
        /// </summary>
        protected override bool Layout(bool force, bool recursioncheck = false)
        {
            var canvas = GetCanvas();
            if (canvas != null)
            {
                SetBounds(0, 0, GetCanvas().Width, GetCanvas().Height);
            }
            return base.Layout(force,recursioncheck);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawModalControl(this);
        }
    }
}
