using System;
using System.Drawing;

namespace Gwen.Controls
{
    /// <summary>
    /// Horizontal slider.
    /// </summary>
    public class HorizontalSlider : Slider
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="HorizontalSlider"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public HorizontalSlider(ControlBase parent)
            : base(parent)
        {
            m_SliderBar.IsHorizontal = true;
            Height = 15;
        }

        #endregion Constructors

        #region Methods

        protected override double CalculateValue() => m_SliderBar.X / (double)(Width - m_SliderBar.Width);

        protected override void ProcessLayout(Size size)
        {
            base.ProcessLayout(size);
            _ = m_SliderBar.SetSize(15, Height);
            UpdateBarFromValue();
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
            base.OnMouseClickedLeft(x, y, down);
            if (down)
            {
                IsMouseDepressed = false; // We forfeit our mouse down event and right to a mouse up event

                m_SliderBar.MoveTo((int)(CanvasPosToLocal(new Point(x, y)).X - m_SliderBar.Width * 0.5), m_SliderBar.Y);
                m_SliderBar.InputMouseClickedLeft(x, y, down);
                OnMoved(m_SliderBar, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawSlider(this, true, m_SnapToNotches && DrawNotches ? m_NotchCount : 0, m_SliderBar.Width, m_Value);

        protected override void UpdateBarFromValue() => m_SliderBar.MoveTo((int)((Width - m_SliderBar.Width) * m_Value), m_SliderBar.Y);

        #endregion Methods
    }
}