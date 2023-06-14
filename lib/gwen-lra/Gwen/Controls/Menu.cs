﻿using Gwen.ControlInternal;
using System;
using System.Drawing;
using System.Linq;

namespace Gwen.Controls
{
    /// <summary>
    /// Popup menu.
    /// </summary>
    public class Menu : ScrollControl
    {
        #region Properties

        /// <summary>
        /// Determines whether the menu should be disposed on close.
        /// </summary>
        public bool DeleteOnClose { get; set; }

        public bool IconMarginDisabled { get; set; }

        protected override Margin PanelMargin => Margin.Zero;
        public override bool IsHidden
        {
            get => base.IsHidden;
            set
            {
                if (value != base.IsHidden)
                {
                    base.IsHidden = value;
                    UpdateCanvas();
                }
            }
        }
        internal override bool IsMenuComponent => true;

        /// <summary>
        /// Determines whether the menu should open on mouse hover.
        /// </summary>
        protected virtual bool ShouldHoverOpenMenu => true;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Menu"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Menu(ControlBase parent)
            : base(parent)
        {
            _ = SetBounds(0, 0, 10, 10);
            IconMarginDisabled = false;

            //   AutoHideBars = false;

            EnableScroll(false, true);
            DeleteOnClose = false;
            AutoSizeToContents = true;
            UpdateCanvas();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Adds a divider menu item.
        /// </summary>
        public virtual void AddDivider() => _ = new MenuDivider(this)
        {
            Dock = Dock.Top,
            Margin = new Margin(IconMarginDisabled ? 0 : 24, 0, 4, 0)
        };

        /// <summary>
        /// Adds a new menu item.
        /// </summary>
        /// <param name="text">Item text.</param>
        /// <returns>Newly created control.</returns>
        public virtual MenuItem AddItem(string text) => AddItem(text, string.Empty);

        /// <summary>
        /// Adds a new menu item.
        /// </summary>
        /// <param name="text">Item text.</param>
        /// <param name="iconName">Icon texture name.</param>
        /// <param name="accelerator">Accelerator for this item.</param>
        /// <returns>Newly created control.</returns>
        public virtual MenuItem AddItem(string text, string iconName, string accelerator = "")
        {
            MenuItem item = new MenuItem(this)
            {
                Padding = Padding.Four
            };
            item.SetText(text);
            item.SetImage(iconName);
            item.SetAccelerator(accelerator);

            OnAddItem(item);

            return item;
        }

        /// <summary>
        /// Closes the current menu.
        /// </summary>
        public virtual void Close()
        {
            //System.Diagnostics.Debug.Print("Menu.Close: {0}", this);
            if (IsVisible)
            {
                ScrollToTop();
                IsHidden = true;
                if (DeleteOnClose)
                {
                    DelayedDelete();
                }
            }
        }

        /// <summary>
        /// Closes all submenus.
        /// </summary>
        public virtual void CloseAll()
        {
            //System.Diagnostics.Debug.Print("Menu.CloseAll: {0}", this);
            ControlBase[] copy = Children.ToArray();
            foreach (ControlBase child in copy)
            {
                if (child is MenuItem menuitem)
                    menuitem.CloseMenu();
            }
        }

        /// <summary>
        /// Closes all submenus and the current menu.
        /// </summary>
        public void CloseMenus()
        {
            CloseAll();
            Close();
        }

        /// <summary>
        /// Indicates whether any (sub)menu is open.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsMenuOpen() => Children.Any(child => { return child is MenuItem && (child as MenuItem).IsMenuOpen; });

        /// <summary>
        ///  Opens the menu.
        /// </summary>
        /// <param name="pos">Unused.</param>
        public void Open(Pos pos)
        {
            IsHidden = false;
            BringToFront();
            Point mouse = Input.InputHandler.MousePosition;
            SetPosition(mouse.X + 1, mouse.Y);
            Invalidate();
        }
        private void UpdateCanvas()
        {
            Canvas canvas = GetCanvas();
            if (canvas != null)
            {
                _ = IsHidden ? canvas.OpenMenus.Remove(this) : canvas.OpenMenus.Add(this);
            }
        }

        #endregion Methods

        public override Size GetSizeToFitContents()
        {
            Size ret = m_Panel.GetSizeToFitContents() + PanelMargin.Size + Padding.Size;
            if (Y + ret.Height > GetCanvas().Height)
            {
                ret.Height = GetCanvas().Height - Y;
                ret.Width += VScrollWidth;
            }
            return ret;
        }

        /// <summary>
        /// Add item handler.
        /// </summary>
        /// <param name="item">Item added.</param>
        protected virtual void OnAddItem(MenuItem item)
        {
            item.TextPadding = new Padding(IconMarginDisabled ? 0 : 24, 0, 16, 0);
            item.Dock = Dock.Top;
            item.Alignment = Pos.CenterV | Pos.Left;
            item.HoverEnter += OnHoverItem;
            _ = item.SizeToChildren();
            _ = SizeToChildren();
            // Do this here - after Top Docking these values mean nothing in layout
            //  int w = item.Width + 10 + 32;
            //  if (w < Width) w = Width;
            //   SetSize(w, Height);
        }

        /// <summary>
        /// Mouse hover handler.
        /// </summary>
        /// <param name="control">Event source.</param>
        protected virtual void OnHoverItem(ControlBase control, EventArgs args)
        {
            if (!ShouldHoverOpenMenu)
                return;

            if (!(control is MenuItem item))
                return;
            if (item.IsMenuOpen)
                return;

            CloseAll();
            item.OpenMenu();
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawMenu(this, IconMarginDisabled);

        /// <summary>
        /// Renders under the actual control (shadows etc).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void RenderUnder(Skin.SkinBase skin)
        {
            base.RenderUnder(skin);
            skin.DrawShadow(this);
        }
    }
}