using Gwen.ControlInternal;
using Gwen.Input;
using System;
using System.Drawing;
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
        public bool IsChecked { get => m_CheckBox.IsChecked; set => m_CheckBox.IsChecked = value; }

        /// <summary>
        /// Label text.
        /// </summary>
        public string Text
        {
            get => m_Label.Text;
            set
            {
                m_Label.Text = value;
                m_Label.IsHidden = string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Checkbox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Checkbox(ControlBase parent)
            : base(parent)
        {
            MinimumSize = new Size(15, 15);
            AutoSizeToContents = true;
            m_CheckBox = new CheckBoxButton(this)
            {
                ToolTipProvider = false,
                IsTabable = false
            };
            m_CheckBox.CheckChanged += OnCheckChanged;
            m_Label = new Label(this)
            {
                ToolTipProvider = false,
                TextPadding = Padding.Two,
                Margin = new Margin(17, 0, 0, 0),
                Dock = Dock.Fill
            };
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
            m_CheckBox?.AlignToEdge(Pos.Left | Pos.CenterV);
            base.ProcessLayout(size);
        }

        /// <summary>
        /// Handler for CheckChanged event.
        /// </summary>
        protected virtual void OnCheckChanged(ControlBase control, EventArgs Args)
        {
            if (m_CheckBox.IsChecked)
            {
                Checked?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                UnChecked?.Invoke(this, EventArgs.Empty);
            }

            CheckChanged?.Invoke(this, EventArgs.Empty);
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
            _ = base.OnKeySpace(down);
            if (!down)
                m_CheckBox.IsChecked = !m_CheckBox.IsChecked;
            return true;
        }
        public override void SetToolTipText(string text) =>
            //  base.SetToolTipText(text);
            m_Label.SetToolTipText(text);
    }
}
