using Gwen.ControlInternal;
using System;
using System.Drawing;

namespace Gwen.Controls
{
    /// <summary>
    /// Menu item.
    /// </summary>
    public class MenuItem : Button
    {
        private bool m_Checked;
        private Menu m_Menu;
        private ControlBase m_SubmenuArrow;
        private Label m_Accelerator;

        /// <summary>
        /// Indicates whether the item is on a menu strip.
        /// </summary>
        public bool IsOnStrip { get; set; }

        /// <summary>
        /// Determines if the menu item is checkable.
        /// </summary>
        public bool IsCheckable { get; set; }

        /// <summary>
        /// Indicates if the parent menu is open.
        /// </summary>
        public bool IsMenuOpen => m_Menu != null && !m_Menu.IsHidden;

        /// <summary>
        /// Gets or sets the check value.
        /// </summary>
        public bool IsChecked
        {
            get => m_Checked;
            set
            {
                if (value == m_Checked)
                    return;

                m_Checked = value;

                CheckChanged?.Invoke(this, EventArgs.Empty);

                if (value)
                {
                    Checked?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    UnChecked?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the parent menu.
        /// </summary>
        public Menu Menu
        {
            get
            {
                if (null == m_Menu)
                {
                    m_Menu = new Menu(GetCanvas())
                    {
                        IsHidden = true
                    };

                    if (!IsOnStrip)
                    {
                        m_SubmenuArrow?.Dispose();
                        m_SubmenuArrow = new RightArrow(this);
                        _ = m_SubmenuArrow.SetSize(15, 15);
                    }

                    Invalidate();
                }

                return m_Menu;
            }
        }

        /// <summary>
        /// Invoked when the item is selected.
        /// </summary>
		public event GwenEventHandler<ItemSelectedEventArgs> Selected;

        /// <summary>
        /// Invoked when the item is checked.
        /// </summary>
		public event GwenEventHandler<EventArgs> Checked;

        /// <summary>
        /// Invoked when the item is unchecked.
        /// </summary>
		public event GwenEventHandler<EventArgs> UnChecked;

        /// <summary>
        /// Invoked when the item's check value is changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> CheckChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItem"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public MenuItem(ControlBase parent)
            : base(parent)
        {
            AutoSizeToContents = true;
            IsOnStrip = false;
            IsTabable = false;
            IsCheckable = false;
            IsChecked = false;

            m_Accelerator = new Label(this);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawMenuItem(this, IsMenuOpen, IsCheckable && m_Checked);

        protected override void ProcessLayout(Size size)
        {
            m_SubmenuArrow?.AlignToEdge(Pos.Right | Pos.CenterV, 4, 0);
            base.ProcessLayout(size);
        }

        /// <summary>
        /// Internal OnPressed implementation.
        /// </summary>
        protected override void OnClicked(int x, int y)
        {
            if (m_Menu != null)
            {
                ToggleMenu();
            }
            else if (!IsOnStrip)
            {
                IsChecked = !IsChecked;
                Selected?.Invoke(this, new ItemSelectedEventArgs(this));
                GetCanvas().CloseMenus();
            }
            base.OnClicked(x, y);
        }

        /// <summary>
        /// Toggles the menu open state.
        /// </summary>
        public void ToggleMenu()
        {
            if (IsMenuOpen)
                CloseMenu();
            else
                OpenMenu();
        }

        /// <summary>
        /// Opens the menu.
        /// </summary>
        public void OpenMenu()
        {
            if (null == m_Menu)
                return;

            m_Menu.IsHidden = false;
            m_Menu.BringToFront();

            Point p = LocalPosToCanvas(Point.Empty);

            // Strip menus open downwards
            if (IsOnStrip)
            {
                m_Menu.SetPosition(p.X, p.Y + Height + 1);
            }
            // Submenus open sidewards
            else
            {
                m_Menu.SetPosition(p.X + Width, p.Y);
            }

            // TODO: Option this.
            // TODO: Make sure on screen, open the other side of the 
            // parent if it's better...
        }

        /// <summary>
        /// Closes the menu.
        /// </summary>
        public void CloseMenu()
        {
            if (null == m_Menu)
                return;
            m_Menu.Close();
            m_Menu.CloseAll();
        }
        // TODO: removed accellerator sizing code

        public MenuItem SetAction(GwenEventHandler<EventArgs> handler)
        {
            if (m_Accelerator != null)
            {
                AddAccelerator(m_Accelerator.Text, handler);
            }

            Selected += handler;
            return this;
        }

        public void SetAccelerator(string acc)
        {
            if (m_Accelerator != null)
            {
                //m_Accelerator.DelayedDelete(); // To prevent double disposing
                m_Accelerator = null;
            }

            if (string.IsNullOrEmpty(acc))
                return;

            m_Accelerator = new Label(this)
            {
                Dock = Dock.Right,
                Alignment = Pos.Right | Pos.CenterV,
                Text = acc,
                Margin = new Margin(0, 0, 16, 0)
            };
            // TODO
        }
        public override string ToString() => "[MenuItem: " + Text + "]";
    }
}
