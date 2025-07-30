using Gwen.ControlInternal;
using System;

namespace Gwen.Controls
{
    /// <summary>
    /// Control with multiple tabs that can be reordered and dragged.
    /// </summary>
    public class TabControl : Container
    {
        private readonly ScrollBarButton[] m_Scroll;
        private int m_ScrollOffset;

        /// <summary>
        /// Invoked when a tab has been added.
        /// </summary>
		public event GwenEventHandler<EventArgs> TabAdded;

        /// <summary>
        /// Invoked when a tab has been removed.
        /// </summary>
		public event GwenEventHandler<EventArgs> TabRemoved;

        /// <summary>
        /// Determines if tabs can be reordered by dragging.
        /// </summary>
        public bool AllowReorder { get => TabStrip.AllowReorder; set => TabStrip.AllowReorder = value; }

        /// <summary>
        /// Currently active tab button.
        /// </summary>
        internal TabButton CurrentButton { get; private set; }

        /// <summary>
        /// Currently active tab page.
        /// </summary>
        public TabPage CurrentPage => CurrentButton.Page;

        /// <summary>
        /// Current tab strip position.
        /// </summary>
        public Dock TabStripPosition { get => TabStrip.StripPosition; set => TabStrip.StripPosition = value; }

        /// <summary>
        /// Tab strip.
        /// </summary>
        public TabStrip TabStrip { get; }
        public int SelectedTab => Children.IndexOf(CurrentButton.Page);

        /// <summary>
        /// Number of tabs in the control.
        /// </summary>
        public int TabCount => TabStrip.Children.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TabControl(ControlBase parent)
            : base(parent)
        {
            m_Scroll = new ScrollBarButton[2];
            m_ScrollOffset = 0;

            TabStrip = new TabStrip(null)
            {
                StripPosition = Dock.Top
            };

            // Make this some special control?
            m_Scroll[0] = new ScrollBarButton(null);
            m_Scroll[0].SetDirectionLeft();
            m_Scroll[0].Clicked += ScrollPressedLeft;
            _ = m_Scroll[0].SetSize(14, 16);

            m_Scroll[1] = new ScrollBarButton(null);
            m_Scroll[1].SetDirectionRight();
            m_Scroll[1].Clicked += ScrollPressedRight;
            _ = m_Scroll[1].SetSize(14, 16);

            PrivateChildren.Add(TabStrip);
            PrivateChildren.Add(m_Scroll[0]);
            PrivateChildren.Add(m_Scroll[1]);

            IsTabable = false;
        }
        protected override void RenderPanel(Skin.SkinBase skin)
        {
            base.RenderPanel(skin);
            skin.DrawTabControl(m_Panel);
        }
        /// <summary>
        /// Adds a new page/tab.
        /// </summary>
        /// <param name="label">Tab label.</param>
        /// <param name="page">Page contents.</param>
        /// <returns>Newly created control.</returns>
        public TabPage AddPage(string label, ControlBase page = null)
        {
            TabButton button = new(TabStrip);
            TabPage tabpage = new(this, button)
            {
                Name = label
            };
            button.SetText(label);
            button.Page = tabpage;
            button.IsTabable = false;
            if (page != null)
            {
                tabpage.AddChild(page);
            }
            AddPage(button);
            return tabpage;
        }

        /// <summary>
        /// Adds a page/tab.
        /// </summary>
        /// <param name="button">Page to add. (well, it's a TabButton which is a parent to the page).</param>
        public void AddPage(TabButton button)
        {
            TabPage page = button.Page;
            page.Parent = this;
            page.IsHidden = true;
            page.Margin = new Margin(6, 6, 6, 6);
            page.Dock = Dock.Fill;

            button.Parent = TabStrip;
            button.Dock = Dock.Left;
            button.TabControl?.UnsubscribeTabEvent(button);
            button.TabControl = this;
            button.Clicked += OnTabPressed;

            if (CurrentButton == null)
            {
                button.Press();
            }

            TabAdded?.Invoke(this, EventArgs.Empty);

            Invalidate();
        }

        public TabPage GetPage(int index) => ((TabButton)TabStrip.Children[index]).Page;
        public TabPage GetPage(string title)
        {
            for (int i = 0; i < TabStrip.Children.Count; i++)
            {
                if (TabStrip.Children[i] is TabButton btn)
                {
                    if (btn.Text == title)
                        return btn.Page;
                }
            }
            return null;
        }
        public void SetTabIndex(TabPage page, int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            TabStrip.Children.Move(page.TabButton, index);
            TabStrip.Invalidate();
        }
        public void RemoveTab(TabPage page)
        {
            int idx = -1;
            if (CurrentButton == page.TabButton)
            {
                idx = TabStrip.Children.IndexOf(page.TabButton);
                if (idx == TabStrip.Children.Count - 1)
                    idx--;
                CurrentButton = null;
            }
            TabStrip.RemoveChild(page.TabButton, true);
            RemoveChild(page, true);
            if (idx != -1)
            {
                GetPage(idx).FocusTab();
            }
            OnLoseTab(page.TabButton);
            Invalidate();
        }
        private void UnsubscribeTabEvent(TabButton button) => button.Clicked -= OnTabPressed;

        /// <summary>
        /// Handler for tab selection.
        /// </summary>
        /// <param name="control">Event source (TabButton).</param>
		internal virtual void OnTabPressed(ControlBase control, EventArgs args)
        {
            if (!(control is TabButton button))
                return;

            ControlBase page = button.Page;
            if (null == page)
                return;

            if (CurrentButton == button)
                return;

            if (null != CurrentButton)
            {
                ControlBase page2 = CurrentButton.Page;
                if (page2 != null)
                {
                    page2.IsHidden = true;
                }
                CurrentButton.Redraw();
                CurrentButton = null;
            }

            CurrentButton = button;

            page.IsHidden = false;

            TabStrip.Invalidate();
            Invalidate();
        }

        protected override void ProcessLayout(Size size)
        {
            HandleOverflow();
            base.ProcessLayout(size);
        }

        /// <summary>
        /// Handler for tab removing.
        /// </summary>
        /// <param name="button"></param>
        internal virtual void OnLoseTab(TabButton button)
        {
            TabRemoved?.Invoke(this, EventArgs.Empty);

            Invalidate();
        }

        private void HandleOverflow()
        {
            Size TabsSize = TabStrip.GetSizeToFitContents();

            // Only enable the scrollers if the tabs are at the top.
            // This is a limitation we should explore.
            // Really TabControl should have derivitives for tabs placed elsewhere where we could specialize 
            // some functions like this for each direction.
            bool needed = TabsSize.Width > Width && TabStrip.Dock == Dock.Top;

            m_Scroll[0].IsHidden = !needed;
            m_Scroll[1].IsHidden = !needed;

            if (!needed)
            {
                if (m_ScrollOffset != 0)
                {
                    m_ScrollOffset = 0;
                    TabStrip.Margin = Margin.Zero;
                    Invalidate();
                }
                return;
            }

            m_ScrollOffset = Util.Clamp(m_ScrollOffset, 0, TabsSize.Width - Width + 32);

            TabStrip.Margin = new Margin(m_ScrollOffset * -1, 0, 0, 0);
            m_Scroll[0].SetPosition(Width - 30, 5);
            m_Scroll[1].SetPosition(m_Scroll[0].Right + m_Scroll[0].Margin.Right, 5);
        }

        protected virtual void ScrollPressedLeft(ControlBase control, EventArgs args)
        {
            m_ScrollOffset -= 120;
            Invalidate();
        }

        protected virtual void ScrollPressedRight(ControlBase control, EventArgs args)
        {
            m_ScrollOffset += 120;
            Invalidate();
        }
    }
}
