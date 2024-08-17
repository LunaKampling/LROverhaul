using System;

namespace Gwen.Controls
{
    /// <summary>
    /// Base for controls whose interior can be scrolled.
    /// </summary>
    public class ScrollControl : Container
    {
        public int VerticalScroll => m_Panel.Y;

        public int HorizontalScroll => m_Panel.X;

        public ControlBase InnerPanel => m_Panel;

        private readonly ScrollBar m_VerticalScrollBar;
        private readonly ScrollBar m_HorizontalScrollBar;

        /// <summary>
        /// Indicates whether the control can be scrolled horizontally.
        /// </summary>
        public bool CanScrollH { get; private set; }

        /// <summary>
        /// Indicates whether the control can be scrolled vertically.
        /// </summary>
        public bool CanScrollV { get; private set; }

        /// <summary>
        /// Determines whether the scroll bars should be hidden if not needed.
        /// </summary>
        public bool AutoHideBars { get; set; }

        protected bool HScrollRequired
        {
            set
            {
                bool disabled = value;
                bool hidden = AutoHideBars && value;
                if (m_HorizontalScrollBar.IsDisabled != disabled ||
                    m_HorizontalScrollBar.IsHidden != hidden)
                {
                    if (value)
                        _ = m_HorizontalScrollBar.SetScrollAmount(0, true);
                    m_HorizontalScrollBar.IsDisabled = disabled;
                    m_HorizontalScrollBar.IsHidden = hidden;
                }
            }
        }

        protected bool VScrollRequired
        {
            set
            {
                bool disabled = value;
                bool hidden = AutoHideBars && value;
                if (m_VerticalScrollBar.IsDisabled != disabled ||
                    m_VerticalScrollBar.IsHidden != hidden)
                {
                    if (value)
                        _ = m_VerticalScrollBar.SetScrollAmount(0, true);
                    m_VerticalScrollBar.IsDisabled = disabled;
                    m_VerticalScrollBar.IsHidden = hidden;
                }
            }
        }
        protected int VScrollWidth => m_VerticalScrollBar.Width;
        protected int HScrollHeight => m_HorizontalScrollBar.Height;
        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ScrollControl(ControlBase parent)
            : base(parent)
        {
            // Todo scrollbars currently hide borders of our owner
            m_VerticalScrollBar = new VerticalScrollBar(null)
            {
                Dock = Dock.Right,
                NudgeAmount = 30,
            };
            m_VerticalScrollBar.BarMoved += VBarMoved;
            CanScrollV = true;

            m_HorizontalScrollBar = new HorizontalScrollBar(null)
            {
                Dock = Dock.Bottom,
                NudgeAmount = 30,
            };
            m_HorizontalScrollBar.BarMoved += HBarMoved;
            CanScrollH = true;
            PrivateChildren.Add(m_VerticalScrollBar);
            PrivateChildren.Add(m_HorizontalScrollBar);
            m_Panel.Dock = Dock.None;
            SendChildToBack(m_Panel);
            m_Panel.AutoSizeToContents = true;
            AutoHideBars = true;
            m_HorizontalScrollBar.Hide();
            m_VerticalScrollBar.Hide();
        }

        protected override void ProcessLayout(Size size)
        {
            _ = UpdateScrollBars();
            base.ProcessLayout(size);
            if (UpdateScrollBars())
            {
                if (m_Panel.NeedsLayout)
                    m_Panel.Layout(false);
            }
            UpdateScrollPosition();
        }
        /// <summary>
        /// Maintain the panel to be at least the maximum width without scrolling
        /// </summary>
        private void UpdatePanel() => m_Panel.MinimumSize = new Size(Width - (m_VerticalScrollBar.IsHidden ? 0 : m_VerticalScrollBar.Width), m_Panel.MinimumSize.Height);
        /// <summary>
        /// Enables or disables inner scrollbars.
        /// </summary>
        /// <param name="horizontal">Determines whether the horizontal scrollbar should be enabled.</param>
        /// <param name="vertical">Determines whether the vertical scrollbar should be enabled.</param>
        public virtual void EnableScroll(bool horizontal, bool vertical)
        {
            CanScrollV = vertical;
            CanScrollH = horizontal;
            m_VerticalScrollBar.IsHidden = !CanScrollV;
            m_HorizontalScrollBar.IsHidden = !CanScrollH;
        }

        public virtual void SetInnerSize(int width, int height) => m_Panel.SetSize(width, height);

        public virtual bool UpdateScrollBars()
        {
            Size sz = m_Panel.AutoSizeToContents ? m_Panel.GetSizeToFitContents() : m_Panel.Size;
            bool vchange = UpdateBar(sz, true);
            bool hchange = UpdateBar(sz, false);
            if (hchange)
            {
                // Scenario: horizontal scroll appearing necessitates vscroll
                _ = UpdateBar(sz, true);
            }
            UpdatePanel();
            return vchange || hchange;
        }
        private bool UpdateBar(Size panelsize, bool vscroll)
        {
            ScrollBar bar = vscroll ? m_VerticalScrollBar : m_HorizontalScrollBar;
            ScrollBar altbar = vscroll ? m_HorizontalScrollBar : m_VerticalScrollBar;
            float percent = vscroll
                ? Height / (float)(panelsize.Height + (altbar.IsHidden ? 0 : altbar.Height))
                : Width / (float)(panelsize.Width + (altbar.IsHidden ? 0 : altbar.Width));
            bool vvis = bar.IsVisible;
            if (vscroll)
            {
                if (CanScrollV)
                    VScrollRequired = percent >= 1;
                else
                    bar.IsHidden = true;
            }
            else
            {
                if (CanScrollH)
                    HScrollRequired = percent >= 1;
                else
                    bar.IsHidden = true;
            }

            if (vscroll)
            {
                bar.ContentSize = panelsize.Height;
                bar.ViewableContentSize = Height - (altbar.IsHidden ? 0 : altbar.Height);
            }
            else
            {
                bar.ContentSize = panelsize.Width;
                bar.ViewableContentSize = Width - (altbar.IsHidden ? 0 : altbar.Width);
            }
            return bar.IsVisible != vvis;
        }
        private void UpdateScrollPosition()
        {
            int newInnerPanelPosX = 0;
            int newInnerPanelPosY = 0;

            if (CanScrollV && !m_VerticalScrollBar.IsHidden)
            {
                newInnerPanelPosY =
                    (int)Math.Round(
                        -(m_Panel.Height - Height + (m_HorizontalScrollBar.IsHidden ? 0 : m_HorizontalScrollBar.Height)) *
                        m_VerticalScrollBar.ScrollAmount);
            }
            if (CanScrollH && !m_HorizontalScrollBar.IsHidden)
            {
                newInnerPanelPosX =
                    (int)Math.Round(
                        -(m_Panel.Width - Width + (m_VerticalScrollBar.IsHidden ? 0 : m_VerticalScrollBar.Width)) *
                        m_HorizontalScrollBar.ScrollAmount);
            }

            m_Panel.SetPosition(newInnerPanelPosX, newInnerPanelPosY);
        }

        protected virtual void VBarMoved(ControlBase control, EventArgs args) => Invalidate();

        protected virtual void HBarMoved(ControlBase control, EventArgs args) => Invalidate();

        protected override void Render(Skin.SkinBase skin)
        {
        }

        /// <summary>
        /// Handler invoked on mouse wheel event.
        /// </summary>
        /// <param name="delta">Scroll delta.</param>
        /// <returns></returns>
        protected override bool OnMouseWheeled(int delta)
        {
            if (CanScrollV && m_VerticalScrollBar.IsVisible)
            {
                if (m_VerticalScrollBar.SetScrollAmount(
                    m_VerticalScrollBar.ScrollAmount - m_VerticalScrollBar.NudgePercent * (delta / 60.0f), false))
                    return true;
            }

            else if (CanScrollH && m_HorizontalScrollBar.IsVisible)
            {
                if (m_HorizontalScrollBar.SetScrollAmount(
                    m_HorizontalScrollBar.ScrollAmount - m_HorizontalScrollBar.NudgePercent * (delta / 60.0f), false))
                    return true;
            }

            return false;
        }

        public virtual void ScrollToBottom()
        {
            if (!CanScrollV)
                return;

            _ = UpdateScrollBars();
            m_VerticalScrollBar.ScrollToBottom();
        }

        public virtual void ScrollToTop() => m_VerticalScrollBar.ScrollToTop();

        public virtual void ScrollToLeft() => m_HorizontalScrollBar.ScrollToLeft();

        public virtual void ScrollToRight()
        {
            if (CanScrollH)
            {
                _ = UpdateScrollBars();
                m_HorizontalScrollBar.ScrollToRight();
            }
        }

        public virtual void DeleteAll() => m_Panel.DeleteAllChildren();
    }
}
