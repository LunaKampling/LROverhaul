using System;

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
            get { return m_ComboBox.Text; }
            set { base.Value = value; }
        }

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public override bool IsEditing
        {
            get { return m_ComboBox.HasFocus; }
        }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered
        {
            get { return base.IsHovered | m_ComboBox.IsHovered; }
        }
        public MenuItem SelectedItem
        {
            get
            {
                return m_ComboBox.SelectedItem;
            }
            set
            {
                m_ComboBox.SelectedItem = value;
            }
        }

        public ComboBoxProperty(ControlBase parent) : base(parent)
        {
            m_ComboBox = new ComboBox(this);
            this.Height = m_ComboBox.Height;
            m_ComboBox.Dock = Dock.Fill;
            m_ComboBox.ShouldDrawBackground = false;
            m_ComboBox.ItemSelected += OnValueChanged;
            m_ComboBox.AutoSizeToContents = true;
            AutoSizeToContents = false;
        }
        public MenuItem AddItem(string text, string name = "", object userdata = null)
        {
            return m_ComboBox.AddItem(text, name, userdata);
        }
        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
        public override void SetValue(string value, bool fireEvents = false)
        {
            m_ComboBox.SelectByText(value);
        }
    }
}
