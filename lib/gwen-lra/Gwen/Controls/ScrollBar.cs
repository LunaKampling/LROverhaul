using Gwen.ControlInternal;
using System;

namespace Gwen.Controls
{
    /// <summary>
    /// Base class for scrollbars.
    /// </summary>
    public class ScrollBar : ControlBase
    {
        protected readonly ScrollBarButton[] m_ScrollButton;
        protected readonly ScrollBarBar m_Bar;

        protected bool m_Depressed;
        protected float m_ScrollAmount;
        protected float m_ContentSize;
        protected float m_ViewableContentSize;
        protected float m_NudgeAmount;

        /// <summary>
        /// Invoked when the bar is moved.
        /// </summary>
		public event GwenEventHandler<EventArgs> BarMoved;

        /// <summary>
        /// Bar size (in pixels).
        /// </summary>
        public virtual int BarSize { get; set; }

        /// <summary>
        /// Bar position (in pixels).
        /// </summary>
        public virtual int BarPos => 0;

        /// <summary>
        /// Button size (in pixels).
        /// </summary>
        public virtual int ButtonSize => 0;

        public virtual float NudgeAmount { set => m_NudgeAmount = value; }
        public virtual float NudgePercent => m_NudgeAmount / (ContentSize - ViewableContentSize);
        public float ScrollAmount => m_ScrollAmount;
        public float ContentSize
        {
            get => m_ContentSize;
            set
            {
                if (m_ContentSize != value)
                {
                    if (value == ViewableContentSize)
                        m_ScrollAmount = 0;
                    else
                    {
                        float unscaled = m_ScrollAmount * (m_ContentSize - ViewableContentSize);
                        m_ScrollAmount = Util.Clamp(unscaled / (value - ViewableContentSize), 0, 1);
                    }
                    Invalidate();
                }
                m_ContentSize = value;
            }
        }
        public float ViewableContentSize
        {
            get => m_ViewableContentSize; set
            {
                if (m_ViewableContentSize != value)
                    Invalidate();
                m_ViewableContentSize = value;
            }
        }

        /// <summary>
        /// Indicates whether the bar is horizontal.
        /// </summary>
        public virtual bool IsHorizontal => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollBar"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        protected ScrollBar(ControlBase parent) : base(parent)
        {
            m_ScrollButton = new ScrollBarButton[2];
            m_ScrollButton[0] = new ScrollBarButton(this);
            m_ScrollButton[1] = new ScrollBarButton(this);

            m_Bar = new ScrollBarBar(this);

            _ = SetBounds(0, 0, 15, 15);
            m_Depressed = false;

            m_ScrollAmount = 0;
            m_ContentSize = 0;
            m_ViewableContentSize = 0;

            NudgeAmount = 20;
        }
        /// <summary>
        /// Sets the scroll amount (0-1).
        /// </summary>
        /// <param name="value">Scroll amount.</param>
        /// <param name="forceUpdate">Determines whether the control should be updated.</param>
        /// <returns>True if control state changed.</returns>
        public virtual bool SetScrollAmount(float value, bool forceUpdate = false)
        {
            if (m_ScrollAmount == value)
                return false;
            m_ScrollAmount = value;
            Invalidate();
            OnBarMoved(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {

        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawScrollBar(this, IsHorizontal, m_Depressed);

        /// <summary>
        /// Handler for the BarMoved event.
        /// </summary>
        /// <param name="control">The control.</param>
		protected virtual void OnBarMoved(ControlBase control, EventArgs args) => BarMoved?.Invoke(this, EventArgs.Empty);

        protected virtual float CalculateScrolledAmount() => 0;

        protected virtual int CalculateBarSize() => 0;

        public virtual void ScrollToLeft() { }
        public virtual void ScrollToRight() { }
        public virtual void ScrollToTop() { }
        public virtual void ScrollToBottom() { }
    }
}
