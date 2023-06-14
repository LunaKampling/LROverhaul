using Gwen.ControlInternal;
using Gwen.Input;
using System;
using System.Drawing;

namespace Gwen.Controls
{
    /// <summary>
    /// RadioButton with label.
    /// </summary>
    public class RadioButton : ControlBase
    {
        private readonly RadioButtonButton m_RadioButton;
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
        public bool IsChecked { get => m_RadioButton.IsChecked; set => m_RadioButton.IsChecked = value; }

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
        /// Initializes a new instance of the <see cref="RadioButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public RadioButton(ControlBase parent)
            : base(parent)
        {
            MinimumSize = new Size(15, 15);
            AutoSizeToContents = true;

            m_RadioButton = new RadioButtonButton(this)
            {
                IsTabable = false
            };
            m_RadioButton.CheckChanged += OnCheckChanged;
            m_RadioButton.ToolTipProvider = false;

            m_Label = new Label(this)
            {
                ToolTipProvider = false,
                TextPadding = Padding.Two,
                Margin = new Margin(17, 0, 0, 0),
                Dock = Dock.Fill
            };
            m_Label.Clicked += delegate (ControlBase Control, ClickedEventArgs args) { m_RadioButton.Press(Control); };
            m_Label.IsTabable = false;
            AutoSizeToContents = true;
            Checked += (o, e) =>
            {
                if (Parent != null)
                {
                    foreach (ControlBase child in Parent.Children)
                    {
                        if (child is RadioButton rb)
                        {
                            if (rb != this)
                                rb.IsChecked = false;
                        }
                    }
                }
            };
        }

        protected override void ProcessLayout(Size size)
        {
            m_RadioButton?.AlignToEdge(Pos.Left | Pos.CenterV);
            base.ProcessLayout(size);
        }

        /// <summary>
        /// Renders the focus overlay.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void RenderFocus(Skin.SkinBase skin)
        {
            if (InputHandler.KeyboardFocus != this)
                return;
            if (!IsTabable)
                return;

            skin.DrawKeyboardHighlight(this, RenderBounds, 0);
        }

        public void Press(ControlBase control = null) => m_RadioButton.Press(control);

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
                m_RadioButton.IsChecked = !m_RadioButton.IsChecked;
            return true;
        }

        /// <summary>
        /// Selects the radio button.
        /// </summary>
        public virtual void Select() => m_RadioButton.IsChecked = true;

        /// <summary>
        /// Handler for CheckChanged event.
        /// </summary>
        protected virtual void OnCheckChanged(ControlBase control, EventArgs Args)
        {
            if (m_RadioButton.IsChecked)
            {
                Checked?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                UnChecked?.Invoke(this, EventArgs.Empty);
            }

            CheckChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
