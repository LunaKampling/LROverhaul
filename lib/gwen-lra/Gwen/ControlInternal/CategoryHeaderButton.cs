using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Header of CollapsibleCategory.
    /// </summary>
    public class CategoryHeaderButton : Button
    {
        protected override System.Drawing.Color CurrentColor => IsDepressed || ToggleState ? Skin.Colors.Text.Highlight : Skin.Colors.Text.Highlight;
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryHeaderButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CategoryHeaderButton(ControlBase parent)
            : base(parent)
        {
            ShouldDrawBackground = false;
            IsToggle = true;
            Alignment = Pos.Center;
            TextPadding = new Padding(3, 2, 3, 2);
            Margin = new Margin(0, 0, 0, 1);
        }
    }
}
