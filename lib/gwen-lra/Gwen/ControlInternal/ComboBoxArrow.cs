using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// ComboBox arrow.
    /// </summary>
    public class ComboBoxArrow : ControlBase
    {
        private readonly ComboBox m_ComboBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComboBoxArrow"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ComboBoxArrow(ComboBox parent)
            : base(parent) // Or Base?
        {
            MouseInputEnabled = false;
            _ = SetSize(15, 15);

            m_ComboBox = parent;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawDropDownArrow(this, m_ComboBox.IsHovered, m_ComboBox.IsDepressed, m_ComboBox.IsOpen, m_ComboBox.IsDisabled);
    }
}
