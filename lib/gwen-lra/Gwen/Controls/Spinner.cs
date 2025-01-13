using Gwen.ControlInternal;
using System;

namespace Gwen.Controls
{
    /// <summary>
    /// Numeric up/down.
    /// </summary>
    public class Spinner : ControlBase
    {
        public class NumericUpDownTextbox : TextBoxNumeric
        {
            private readonly Spinner m_parent;
            public NumericUpDownTextbox(Spinner parent) : base(parent)
            {
                m_parent = parent;
            }
            public override void SetValue(double v) => m_parent.Value = v;
            protected override void UpdateAfterTextChanged()
            {
            }
            public void UpdateTextValue() => UpdateValue();
        }
        private readonly UpDownButton_Down m_Down;
        private readonly UpDownButton_Up m_Up;
        private readonly NumericUpDownTextbox m_Textbox;
        private readonly ControlBase m_BtnContainer;
        private double m_Max;
        private double m_Min;
        private double m_Value;
        /// <summary>
        /// Invoked when the value has been changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Maximum value.
        /// </summary>
        public double Max
        {
            get => m_Max;
            set
            {
                m_Max = value;
                if (Value > m_Max)
                    Value = m_Max;
            }
        }

        /// <summary>
        /// Minimum value.
        /// </summary>
        public double Min
        {
            get => m_Min;
            set
            {
                m_Min = value;
                if (Value < m_Min)
                    Value = m_Min;
            }
        }
        public double IncrementSize { get; set; } = 1;

        /// <summary>
        /// Numeric value of the control.
        /// </summary>
        public double Value
        {
            get => m_Value;
            set
            {
                if (IncrementSize > 1)
                    value = value - value % IncrementSize + (m_Min % 2 == 0 ? 0 : 1);
                if (value < m_Min)
                    value = m_Min;
                if (value > m_Max)
                    value = m_Max;
                if (value != m_Value)
                {
                    m_Value = value;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
                if (value != m_Textbox.Value)
                {
                    m_Textbox.Value = value;
                    m_Textbox.CursorPos = 0;
                    m_Textbox.CursorEnd = 0;
                }
                if (value.ToString() != m_Textbox.Text)
                {
                    m_Textbox.Text = value.ToString();
                }
            }
        }
        public bool OnlyWholeNumbers
        {
            get => m_Textbox.OnlyWholeNumbers;
            set
            {
                m_Textbox.OnlyWholeNumbers = value;
                Value = Math.Round(Value);
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Spinner"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Spinner(ControlBase parent)
            : base(parent)
        {
            //   Padding = Padding.One;
            m_Textbox = new NumericUpDownTextbox(this)
            {
                ShouldDrawBackground = false
            };
            // m_Textbox.TextPadding = new Padding(m_Textbox.TextPadding.Left, m_Textbox.TextPadding.Top, m_BtnContainer.Width, m_Textbox.TextPadding.Bottom);
            _ = SetSize(m_Textbox.Height * 3, m_Textbox.Height);
            m_Textbox.Dock = Dock.Fill;
            m_BtnContainer = new Panel(this)
            {
                Padding = Padding.Zero,
                Margin = new Margin(0, 1, 1, 1)
            };
            ;
            m_BtnContainer.Dock = Dock.Right;
            m_BtnContainer.AutoSizeToContents = true;
            // m_BtnContainer.DrawDebugOutlines = true;
            m_Up = new UpDownButton_Up(m_BtnContainer);
            m_Up.Clicked += OnButtonUp;
            m_Up.IsTabable = false;
            m_Up.Dock = Dock.Top;

            m_Down = new UpDownButton_Down(m_BtnContainer);
            m_Down.Clicked += OnButtonDown;
            m_Down.IsTabable = false;
            m_Down.Dock = Dock.Bottom;

            m_Max = 100;
            m_Min = 0;
            m_Value = 0;
            m_Textbox.Value = 0;
        }

        #endregion Constructors

        #region Methods
        protected override void ProcessLayout(Size size)
        {
            int ctrlsize = size.Height - m_BtnContainer.Margin.Height;
            _ = m_Up.SetSize(ctrlsize / 2 + ctrlsize / 4, ctrlsize / 2);
            _ = m_Down.SetSize(ctrlsize / 2 + ctrlsize / 4, ctrlsize / 2);
            base.ProcessLayout(size);
        }

        /// <summary>
        /// Handler for the button down event.
        /// </summary>
        /// <param name="control">Event source.</param>
        protected virtual void OnButtonDown(ControlBase control, ClickedEventArgs args) => Decrement();

        /// <summary>
        /// Handler for the button up event.
        /// </summary>
        /// <param name="control">Event source.</param>
        protected virtual void OnButtonUp(ControlBase control, EventArgs args) => Increment();
        protected override bool OnMouseWheeled(int delta)
        {
            m_Textbox.UpdateTextValue();
            Value = m_Value + IncrementSize * Math.Sign(delta);
            return true;
        }
        public void Increment() => Value = m_Value + IncrementSize;
        public void Decrement() => Value = m_Value - IncrementSize;
        /// <summary>
        /// Handler for Down Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyDown(bool down)
        {
            if (down)
                Decrement();
            return true;
        }

        /// <summary>
        /// Handler for Up Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyUp(bool down)
        {
            if (down)
                Increment();
            return true;
        }
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawTextBox(this, m_Textbox.HasFocus || m_BtnContainer.HasFocus);
            base.Render(skin);
        }
        #endregion Methods
    }
}