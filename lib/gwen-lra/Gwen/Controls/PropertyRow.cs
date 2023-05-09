using Gwen.ControlInternal;
using System;

namespace Gwen.Controls
{
    /// <summary>
    /// Single property row.
    /// </summary>
    public class PropertyRow : ControlBase
    {
        #region Events

        /// <summary>
        /// Invoked when the property value has changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> ValueChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public bool IsEditing { get { return m_Property != null && m_Property.IsEditing; } }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered
        {
            get
            {
                return base.IsHovered || (m_Property != null && m_Property.IsHovered);
            }
        }

        /// <summary>
        /// Property name.
        /// </summary>
        public string Label { get { return m_Label.Text; } set { m_Label.Text = value; } }

        public System.Drawing.Color LabelColor
        {
            get
            {
                return m_Label.TextColorOverride;
            }
            set
            {
                m_Label.TextColorOverride = value;
            }
        }

        /// <summary>
        /// Property value.
        /// </summary>
        public string Value { get { return m_Property.Value; } set { m_Property.Value = value; } }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRow"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="prop">Property control associated with this row.</param>
        public PropertyRow(ControlBase parent, PropertyBase prop)
            : base(parent)
        {
            m_Label = new PropertyRowLabel(this);
            m_Label.Dock = Dock.Left;
            m_Label.Alignment = Pos.Left | Pos.Top;
            m_Label.Margin = new Margin(2, 2, 0, 2);
            m_Label.AutoSizeToContents = false;

            m_Property = prop;
            m_Property.Dock = Dock.Fill;
            m_Property.Parent = this;
            m_Property.Margin = new Margin(1, 0, 0, 1);
            m_Property.ValueChanged += OnValueChanged;
            m_Property.ToolTipProvider = false;
            SizeToChildren(false, true);
            AutoSizeToContents = true;
        }

        #endregion Constructors

        #region Methods

        protected override void ProcessLayout(System.Drawing.Size size)
        {
            PropertyTable parent = Parent as PropertyTable;
            if (parent != null)
            {
                m_Label.Width = parent.SplitWidth;
            }
            base.ProcessLayout(size);
        }

        protected virtual void OnValueChanged(ControlBase control, EventArgs args)
        {
            if (ValueChanged != null)
                ValueChanged.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            /* SORRY */
            if (IsEditing != m_LastEditing)
            {
                OnEditingChanged();
                m_LastEditing = IsEditing;
            }

            if (IsHovered != m_LastHover)
            {
                OnHoverChanged();
                m_LastHover = IsHovered;
            }
            /* SORRY */

            skin.DrawPropertyRow(this, m_Label.Right + m_Label.Margin.Right, IsEditing, IsHovered | m_Property.IsHovered);
        }

        #endregion Methods

        #region Fields

        private readonly Label m_Label;
        private readonly PropertyBase m_Property;
        private bool m_LastEditing;
        private bool m_LastHover;

        #endregion Fields

        private void OnEditingChanged()
        {
            m_Label.Redraw();
        }

        private void OnHoverChanged()
        {
            m_Label.Redraw();
        }
        public override void SetToolTipText(string text)
        {
            base.SetToolTipText(text);
            m_Label.SetToolTipText(text);
        }
    }
}