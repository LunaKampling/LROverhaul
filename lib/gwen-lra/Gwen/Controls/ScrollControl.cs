using System;
using System.Linq;
using System.Drawing;

namespace Gwen.Controls
{
    /// <summary>
    /// Base for controls whose interior can be scrolled.
    /// </summary>
    public class ScrollControl : Container
    {
        private bool m_CanScrollH;
        private bool m_CanScrollV;
        private bool m_AutoHideBars;

        public int VerticalScroll
        {
            get
            {
                return m_Panel.Y;
            }
        }

        public int HorizontalScroll
        {
            get
            {
                return m_Panel.X;
            }
        }

        public ControlBase InnerPanel
        {
            get
            {
                return m_Panel;
            }
        }

        private readonly ScrollBar m_VerticalScrollBar;
        private readonly ScrollBar m_HorizontalScrollBar;

        /// <summary>
        /// Indicates whether the control can be scrolled horizontally.
        /// </summary>
        public bool CanScrollH { get { return m_CanScrollH; } }

        /// <summary>
        /// Indicates whether the control can be scrolled vertically.
        /// </summary>
        public bool CanScrollV { get { return m_CanScrollV; } }

        /// <summary>
        /// Determines whether the scroll bars should be hidden if not needed.
        /// </summary>
        public bool AutoHideBars { get { return m_AutoHideBars; } set { m_AutoHideBars = value; } }

        protected bool HScrollRequired
        {
            set
            {
                bool disabled = value;
                bool hidden = m_AutoHideBars && value;
                if (m_HorizontalScrollBar.IsDisabled != disabled ||
                    m_HorizontalScrollBar.IsHidden != hidden)
                {
                    if (value)
                        m_HorizontalScrollBar.SetScrollAmount(0, true);
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
                bool hidden = m_AutoHideBars && value;
                if (m_VerticalScrollBar.IsDisabled != disabled ||
                    m_VerticalScrollBar.IsHidden != hidden)
                {
                    if (value)
                        m_VerticalScrollBar.SetScrollAmount(0, true);
                    m_VerticalScrollBar.IsDisabled = disabled;
                    m_VerticalScrollBar.IsHidden = hidden;
                }
            }
        }
        protected int VScrollWidth
        {
            get
            {
                return m_VerticalScrollBar.Width;
            }
        }
        protected int HScrollHeight
        {
            get
            {
                return m_HorizontalScrollBar.Height;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ScrollControl(ControlBase parent)
            : base(parent)
        {
            //todo scrollbars currently hide borders of our owner
            m_VerticalScrollBar = new VerticalScrollBar(null)
            {
                Dock = Dock.Right,
                NudgeAmount = 30,
            };
            m_VerticalScrollBar.BarMoved += VBarMoved;
            m_CanScrollV = true;

            m_HorizontalScrollBar = new HorizontalScrollBar(null)
            {
                Dock = Dock.Bottom,
                NudgeAmount = 30,
            };
            m_HorizontalScrollBar.BarMoved += HBarMoved;
            m_CanScrollH = true;
            PrivateChildren.Add(m_VerticalScrollBar);
            PrivateChildren.Add(m_HorizontalScrollBar);
            m_Panel.Dock = Dock.None;
            SendChildToBack(m_Panel);
            m_Panel.AutoSizeToContents = true;
            m_AutoHideBars = true;
            m_HorizontalScrollBar.Hide();
            m_VerticalScrollBar.Hide();
        }

        protected override void ProcessLayout(Size size)
        {
            UpdateScrollBars();
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
        private void UpdatePanel()
        {
            m_Panel.MinimumSize = new Size(Width - (m_VerticalScrollBar.IsHidden ? 0 : m_VerticalScrollBar.Width), m_Panel.MinimumSize.Height);
        }
        /// <summary>
        /// Enables or disables inner scrollbars.
        /// </summary>
        /// <param name="horizontal">Determines whether the horizontal scrollbar should be enabled.</param>
        /// <param name="vertical">Determines whether the vertical scrollbar should be enabled.</param>
        public virtual void EnableScroll(bool horizontal, bool vertical)
        {
            m_CanScrollV = vertical;
            m_CanScrollH = horizontal;
            m_VerticalScrollBar.IsHidden = !m_CanScrollV;
            m_HorizontalScrollBar.IsHidden = !m_CanScrollH;
        }

        public virtual void SetInnerSize(int width, int height)
        {
            m_Panel.SetSize(width, height);
        }

        public virtual bool UpdateScrollBars()
        {
            var sz = m_Panel.AutoSizeToContents ? m_Panel.GetSizeToFitContents() : m_Panel.Size;
            bool vchange = UpdateBar(sz, true);
            bool hchange = UpdateBar(sz, false);
            if (hchange)
            {
                // scenario: horizontal scroll appearing necessitates vscroll
                UpdateBar(sz, true);
            }
            UpdatePanel();
            return vchange || hchange;
        }
        private bool UpdateBar(Size panelsize, bool vscroll)
        {
            var bar = vscroll ? m_VerticalScrollBar : m_HorizontalScrollBar;
            var altbar = vscroll ? m_HorizontalScrollBar : m_VerticalScrollBar;
            float percent;
            if (vscroll)
            {
                percent =
                Height / (float)(panelsize.Height + (altbar.IsHidden ? 0 : altbar.Height));
            }
            else
            {
                percent =
                Width / (float)(panelsize.Width + (altbar.IsHidden ? 0 : altbar.Width));
            }
            bool vvis = bar.IsVisible;
            if (vscroll)
            {
                if (m_CanScrollV)
                    VScrollRequired = percent >= 1;
                else
                    bar.IsHidden = true;
            }
            else
            {
                if (m_CanScrollH)
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

        protected virtual void VBarMoved(ControlBase control, EventArgs args)
        {
            Invalidate();
        }

        protected virtual void HBarMoved(ControlBase control, EventArgs args)
        {
            Invalidate();
        }

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

            UpdateScrollBars();
            m_VerticalScrollBar.ScrollToBottom();
        }

        public virtual void ScrollToTop()
        {
            m_VerticalScrollBar.ScrollToTop();
        }

        public virtual void ScrollToLeft()
        {
            m_HorizontalScrollBar.ScrollToLeft();
        }

        public virtual void ScrollToRight()
        {
            if (CanScrollH)
            {
                UpdateScrollBars();
                m_HorizontalScrollBar.ScrollToRight();
            }
        }

        public virtual void DeleteAll()
        {
            m_Panel.DeleteAllChildren();
        }
    }
}
