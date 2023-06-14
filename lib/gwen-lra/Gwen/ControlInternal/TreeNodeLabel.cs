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

                return Parent is TreeNode node && node.IsSelected
                    ? Skin.Colors.Text.AccentForeground
                    : IsDepressed
                    ? Skin.Colors.Text.Contrast
                    : ToggleState ? Skin.Colors.Text.Highlight : IsHovered ? Skin.Colors.Text.ContrastLow : Skin.Colors.Text.Foreground;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeLabel"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TreeNodeLabel(ControlBase parent)
            : base(parent)
        {
            Alignment = Pos.Left | Pos.CenterV;
            ShouldDrawBackground = false;
            AutoSizeToContents = true;
            TextPadding = new Padding(3, 0, 3, 0);
        }
    }
}
