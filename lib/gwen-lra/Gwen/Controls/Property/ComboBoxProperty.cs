namespace Gwen.Controls
{
    /// <summary>
    /// Text property.
    /// </summary>
    public class ComboBoxProperty : PropertyBase
    {
        protected readonly ComboBox m_ComboBox;
        /// <summary>
        /// Property value.
        /// </summary>
        public override string Value
        {
            get => m_ComboBox.Text;
            set => base.Value = value;
        }

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public override bool IsEditing => m_ComboBox.HasFocus;

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered => base.IsHovered | m_ComboBox.IsHovered;
        public MenuItem SelectedItem
        {
            get => m_ComboBox.SelectedItem;
            set => m_ComboBox.SelectedItem = value;
        }

        public ComboBoxProperty(ControlBase parent) : base(parent)
        {
            m_ComboBox = new ComboBox(this);
            Height = m_ComboBox.Height;
            m_ComboBox.Dock = Dock.Fill;
            m_ComboBox.ShouldDrawBackground = false;
            m_ComboBox.ItemSelected += OnValueChanged;
            m_ComboBox.AutoSizeToContents = true;
            AutoSizeToContents = false;
        }
        public MenuItem AddItem(string text, string name = "", object userdata = null) => m_ComboBox.AddItem(text, name, userdata);
        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
        public override void SetValue(string value, bool fireEvents = false) => m_ComboBox.SelectByText(value);
    }
}
