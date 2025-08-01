﻿namespace Gwen.ControlInternal
{
    /// <summary>
    /// Drag&drop highlight.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Highlight"/> class.
    /// </remarks>
    /// <param name="parent">Parent control.</param>
    public class Highlight(Controls.ControlBase parent) : Controls.ControlBase(parent)
    {

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawHighlight(this);
    }
}
