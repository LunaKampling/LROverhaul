using System;
using System.Drawing;
using Gwen.Input;
using Gwen.ControlInternal;
namespace Gwen.Controls
{
    /// <summary>
    /// CheckBox with label.
    /// </summary>
    public class Checkbox : ControlBase
    {
        private readonly CheckBoxButton m_CheckBox;
        private readonly Label m_Label;

        /// <summary>
        /// Invoked when the control has been checked.
        /// </summary>
        public event GwenEventHandler<EventArgs> Checked;

        /// <summary>
        /// Invoked when the control has been unchecked.
        /// </summary>
        public event GwenEventHandler<EventArgs> UnChecked;

        /// <summary>
        /// Invoked when the control's check has been changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> CheckChanged;

        /// <summary>
        /// Indicates whether the control is checked.
        /// </summary>
        public bool IsChecked { get { return m_CheckBox.IsChecked; } set { m_CheckBox.IsChecked = value; } }

        /// <summary>
        /// Label text.
        /// </summary>
        public string Text
        {
            get { return m_Label.Text; }
            set
            {
                m_Label.Text = value;
                m_Label.IsHidden = (string.IsNullOrEmpty(value));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Checkbox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Checkbox(ControlBase parent)
            : base(parent)
        {
            this.MinimumSize = new System.Drawing.Size(15, 15);
            AutoSizeToContents = true;
            m_CheckBox = new CheckBoxButton(this);
            m_CheckBox.ToolTipProvider = false;
            m_CheckBox.IsTabable = false;
            m_CheckBox.CheckChanged += OnCheckChanged;
            m_Label = new Label(this);
            m_Label.ToolTipProvider = false;
            m_Label.TextPadding = Padding.Two;
            m_Label.Margin = new Margin(17, 0, 0, 0);
            m_Label.Dock = Dock.Fill;
            m_Label.Clicked += delegate (ControlBase Control, ClickedEventArgs args) { m_CheckBox.Press(Control); };
            m_Label.IsTabable = false;
            Text = "";

            KeyboardInputEnabled = true;
            IsTabable = true;
        }
        protected override void RenderFocus(Skin.SkinBase skin)
        {
            if (InputHandler.KeyboardFocus != this)
                return;
            if (!IsTabable)
                return;

            if (m_Label.IsVisible)
                skin.DrawKeyboardHighlight(this, m_Label.Bounds, 0);
            else
                base.RenderFocus(skin);
        }
        protected override void ProcessLayout(Size size)
        {
            if (m_CheckBox != null)
            {
                m_CheckBox.AlignToEdge(Pos.Left | Pos.CenterV);
            }
            base.ProcessLayout(size);
        }

        /// <summary>
        /// Handler for CheckChanged event.
        /// </summary>
        protected virtual void OnCheckChanged(ControlBase control, EventArgs Args)
        {
            if (m_CheckBox.IsChecked)
            {
                if (Checked != null)
                    Checked.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (UnChecked != null)
                    UnChecked.Invoke(this, EventArgs.Empty);
            }

            if (CheckChanged != null)
                CheckChanged.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handler for Space keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeySpace(bool down)
        {
            base.OnKeySpace(down);
            if (!down)
                m_CheckBox.IsChecked = !m_CheckBox.IsChecked;
            return true;
        }
        public override void SetToolTipText(string text)
        {
            //  base.SetToolTipText(text);
            m_Label.SetToolTipText(text);
        }
    }
}
