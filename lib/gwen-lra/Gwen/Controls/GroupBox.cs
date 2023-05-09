using System;
using System.Drawing;

namespace Gwen.Controls
{
    /// <summary>
    /// Group box (container).
    /// </summary>
    /// <remarks>Don't use autosize with docking.</remarks>
    public class GroupBox : Container
    {
        protected Label m_Label;
        /// <summary>
        /// Font.
        /// </summary>
        public Font Font
        {
            get { return m_Label.Font; }
            set
            {
                m_Label.Font = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Text.
        /// </summary>
        public virtual string Text
        {
            get
            {
                return m_Label.Text;
            }
            set
            {
                m_Label.Text = value;
            }
        }

        protected override Margin PanelMargin
        {
            get
            {
                return new Margin(5, m_Label.TextHeight + 5, 5, 5);
            }
        }
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public GroupBox(ControlBase parent) : base(parent)
        {
            m_Label = new Label(null);
            m_Label.TextPadding = new Padding(10, 0, 10, 0);
            m_Label.Alignment = Pos.Top | Pos.Left;
            m_Label.AutoSizeToContents = true;
            PrivateChildren.Add(m_Label);
            m_Panel.AutoSizeToContents = true;
            Invalidate();
            //Margin = new Margin(5, 5, 5, 5);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            if (ShouldDrawBackground)
            {
                var end = m_Label.TextX + m_Label.TextWidth;
                int txwidth = m_Label.TextWidth - (end - Math.Min(end, Width - Padding.Right));
                skin.DrawGroupBox(this, m_Label.TextX, m_Label.TextHeight, txwidth);
            }
        }

        #endregion Methods
    }
}