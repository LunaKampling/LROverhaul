using System;
using Gwen.ControlInternal;
namespace Gwen.Controls
{
    /// <summary>
    /// Checkable property.
    /// </summary>
    public class CheckProperty : PropertyBase
    {
        #region Properties

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public override bool IsEditing
        {
            get { return m_CheckBox.HasFocus; }
        }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered
        {
            get { return base.IsHovered || m_CheckBox.IsHovered; }
        }

        /// <summary>
        /// Property value.
        /// </summary>
        public override string Value
        {
            get { return m_CheckBox.IsChecked ? "1" : "0"; }
            set { base.Value = value; }
        }
        public bool IsChecked
        {
            get
            {
                return m_CheckBox.IsChecked;
            }
            set
            {
                m_CheckBox.IsChecked = value;
            }
        }
        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckProperty"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CheckProperty(Gwen.Controls.ControlBase parent)
            : base(parent)
        {
            m_CheckBox = new Checkbox(this);
            m_CheckBox.Text = "";
            m_CheckBox.ShouldDrawBackground = false;
            m_CheckBox.CheckChanged += OnValueChanged;
            m_CheckBox.IsTabable = true;
            m_CheckBox.KeyboardInputEnabled = true;
            m_CheckBox.SetPosition(2, 1);

            Height = 18;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
        public override void SetValue(string value, bool fireEvents = false)
        {
            if (value == "1" || value.ToLower() == "true" || value.ToLower() == "yes")
            {
                m_CheckBox.IsChecked = true;
            }
            else
            {
                m_CheckBox.IsChecked = false;
            }
        }

        #endregion Methods

        #region Fields

        private readonly Checkbox m_CheckBox;

        #endregion Fields
    }
}