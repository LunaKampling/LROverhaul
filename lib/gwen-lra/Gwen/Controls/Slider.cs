using Gwen.ControlInternal;
using Gwen.Input;
using System;

namespace Gwen.Controls
{
    /// <summary>
    /// Base slider.
    /// </summary>
    public class Slider : ControlBase
    {
        #region Events

        /// <summary>
        /// Invoked when the value has been changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> ValueChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Maximum value.
        /// </summary>
        public double Max { get => m_Max; set => SetRange(m_Min, value); }

        /// <summary>
        /// Minimum value.
        /// </summary>
        public double Min { get => m_Min; set => SetRange(value, m_Max); }

        /// <summary>
        /// Number of notches on the slider axis.
        /// </summary>
        public int NotchCount
        {
            get => m_NotchCount;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("NotchCount cannot be <= 0");
                m_NotchCount = value;
            }
        }

        /// <summary>
        /// Determines whether the slider should snap to notches.
        /// </summary>
        public bool SnapToNotches { get => m_SnapToNotches; set => m_SnapToNotches = value; }
        public bool DrawNotches { get; set; } = true;

        public bool Held => m_SliderBar.IsHeld;

        public override string Tooltip
        {
            get => base.Tooltip;

            set
            {
                base.Tooltip = value;
                m_SliderBar.Tooltip = value;
            }
        }
        /// <summary>
        /// Current value.
        /// </summary>
        public double Value
        {
            get
            {
                return m_Min + m_Value * (m_Max - m_Min);
                ;
            }
            set
            {
                if (value < m_Min)
                    value = m_Min;
                if (value > m_Max)
                    value = m_Max;
                // Normalize Value
                value = m_Min == m_Max ? 0 : (value - m_Min) / (m_Max - m_Min);
                SetValueInternal(value);
                Redraw();
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Sets the value range.
        /// </summary>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        public void SetRange(double min, double max)
        {
            double denormalized = Value;
            m_Min = min;
            m_Max = max;
            Value = denormalized;
        }

        #endregion Methods

        #region Fields

        protected readonly SliderBar m_SliderBar;
        protected double m_Max;
        protected double m_Min;
        protected int m_NotchCount;
        protected bool m_SnapToNotches;
        protected double m_Value;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Slider"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        protected Slider(ControlBase parent)
            : base(parent)
        {
            _ = SetBounds(new Rectangle(0, 0, 32, 128));

            m_SliderBar = new SliderBar(this);
            m_SliderBar.Dragged += OnMoved;
            m_Min = 0.0f;
            m_Max = 1.0f;

            m_SnapToNotches = false;
            m_NotchCount = 5;
            m_Value = 0.0f;

            KeyboardInputEnabled = true;
            IsTabable = false;
        }

        #endregion Constructors
        protected virtual double CalculateValue() => 0;

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
                Value--;
            return true;
        }

        /// <summary>
        /// Handler for End keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyEnd(bool down)
        {
            if (down)
                Value = m_Max;
            return true;
        }

        /// <summary>
        /// Handler for Home keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyHome(bool down)
        {
            if (down)
                Value = m_Min;
            return true;
        }

        /// <summary>
        /// Handler for Left Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyLeft(bool down)
        {
            if (down)
                Value--;
            return true;
        }

        /// <summary>
        /// Handler for Right Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyRight(bool down)
        {
            if (down)
                Value++;
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
                Value++;
            return true;
        }

        protected virtual void OnMoved(ControlBase control, EventArgs args) => SetValueInternal(CalculateValue());

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

        protected virtual void SetValueInternal(double val)
        {
            if (m_SnapToNotches)
            {
                val = (int)Math.Round(val * m_NotchCount);
                val /= m_NotchCount;

            }

            if (m_Value != val)
            {
                m_Value = val;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }

            UpdateBarFromValue();
        }

        protected virtual void UpdateBarFromValue()
        {
        }
    }
}