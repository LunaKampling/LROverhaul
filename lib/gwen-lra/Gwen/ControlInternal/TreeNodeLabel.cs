using System;
using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Tree node label.
    /// </summary>
    public class TreeNodeLabel : Button
    {
        protected override System.Drawing.Color CurrentColor
        {
            get
            {
                if (IsDisabled)
                {
                    return Skin.Colors.Text.Disabled;
                }

                if (Parent is TreeNode node && node.IsSelected)
                {
                    return Skin.Colors.Text.AccentForeground;
                }
                if (IsDepressed)
                {
                    return Skin.Colors.Text.Contrast;
                }
                if (ToggleState)
                {
                    return Skin.Colors.Text.Highlight;
                }

                if (IsHovered)
                {
                    return Skin.Colors.Text.ContrastLow;
                }
                return Skin.Colors.Text.Foreground;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeLabel"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TreeNodeLabel(Controls.ControlBase parent)
            : base(parent)
        {
            Alignment = Pos.Left | Pos.CenterV;
            ShouldDrawBackground = false;
            AutoSizeToContents = true;
            TextPadding = new Padding(3, 0, 3, 0);
        }
    }
}
