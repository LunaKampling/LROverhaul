using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Label for PropertyRow.
    /// </summary>
    public class PropertyRowLabel : Label
    {
        private readonly PropertyRow m_PropertyRow;
        protected override System.Drawing.Color CurrentColor => IsDisabled
                    ? Skin.Colors.Text.Disabled
                    : m_PropertyRow != null && m_PropertyRow.IsEditing
                        ? Skin.Colors.Text.Highlight
                        : m_PropertyRow != null && m_PropertyRow.IsHovered ? Skin.Colors.Text.Contrast : Skin.Colors.Text.Foreground;
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRowLabel"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public PropertyRowLabel(PropertyRow parent) : base(parent)
        {
            AutoSizeToContents = false;
            Alignment = Pos.Left | Pos.CenterV;
            m_PropertyRow = parent;
        }
    }
}
