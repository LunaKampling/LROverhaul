using System;
using System.Drawing;
using Gwen.ControlInternal;

namespace Gwen.Controls
{
    /// <summary>
    /// CollapsibleCategory control. Used in CollapsibleList.
    /// </summary>
    public class CollapsibleCategory : Container
    {
        private readonly Button m_HeaderButton;
        private readonly CollapsibleList m_List;

        /// <summary>
        /// Header text.
        /// </summary>
        public string Text { get { return m_HeaderButton.Text; } set { m_HeaderButton.Text = value; } }

        /// <summary>
        /// Determines whether the category is collapsed (closed).
        /// </summary>
        public bool IsCollapsed { get { return m_HeaderButton.ToggleState; } set { m_HeaderButton.ToggleState = value; } }

        /// <summary>
        /// Invoked when an entry has been selected.
        /// </summary>
		public event GwenEventHandler<ItemSelectedEventArgs> Selected;

        /// <summary>
        /// Invoked when the category collapsed state has been changed (header button has been pressed).
        /// </summary>
		public event GwenEventHandler<EventArgs> Collapsed;
        protected override Margin PanelMargin
        {
            get
            {
                return new Margin(0, 0, 0, 5);
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CollapsibleCategory"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CollapsibleCategory(CollapsibleList parent) : base(parent)
        {
            m_HeaderButton = new CategoryHeaderButton(null);
            m_HeaderButton.Text = "Category Title"; // [omeg] todo: i18n
            m_HeaderButton.Dock = Dock.Top;
            m_HeaderButton.Toggled += OnHeaderToggle;
            m_HeaderButton.AutoSizeToContents = true;
            PrivateChildren.Add(m_HeaderButton);
            m_HeaderButton.SendToBack();

            m_List = parent;

            AutoSizeToContents = true;
            m_Panel.Dock = Dock.Fill;
            m_Panel.AutoSizeToContents = true;
            this.Dock = Dock.Top;
            Margin = new Margin(2, 2, 2, 2);
        }

        /// <summary>
        /// Gets the selected entry.
        /// </summary>
        public Button GetSelectedButton()
        {
            foreach (ControlBase child in Children)
            {
                CategoryButton button = child as CategoryButton;
                if (button == null)
                    continue;

                if (button.ToggleState)
                    return button;
            }

            return null;
        }

        /// <summary>
        /// Handler for header button toggle event.
        /// </summary>
        /// <param name="control">Source control.</param>
		protected virtual void OnHeaderToggle(ControlBase control, EventArgs args)
        {
            m_Panel.IsHidden = m_HeaderButton.ToggleState;
            Invalidate();
            InvalidateParent();
            if (Collapsed != null)
                Collapsed.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handler for Selected event.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void OnSelected(ControlBase control, EventArgs args)
        {
            CategoryButton child = control as CategoryButton;
            if (child == null) return;

            if (m_List != null)
            {
                m_List.UnselectAll();
            }
            else
            {
                UnselectAll();
            }

            child.ToggleState = true;

            if (Selected != null)
                Selected.Invoke(this, new ItemSelectedEventArgs(control));
        }

        /// <summary>
        /// Adds a new entry.
        /// </summary>
        /// <param name="name">Entry name (displayed).</param>
        /// <returns>Newly created control.</returns>
        public Button Add(string name)
        {
            return new CategoryButton(this) { Text = name };
        }

        protected override void OnChildAdded(ControlBase child)
        {
            base.OnChildAdded(child);
            if (child is CategoryButton btn)
            {
                btn.Clicked += OnSelected;
            }
        }

        protected override void OnChildRemoved(ControlBase child)
        {
            base.OnChildRemoved(child);
            if (child is CategoryButton btn)
            {
                btn.Clicked -= OnSelected;
            }
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawCategoryInner(this, m_HeaderButton.ToggleState);
            base.Render(skin);
        }

        /// <summary>
        /// Unselects all entries.
        /// </summary>
        public void UnselectAll()
        {
            foreach (ControlBase child in Children)
            {
                CategoryButton button = child as CategoryButton;
                if (button == null)
                    continue;

                button.ToggleState = false;
            }
        }

        /// <summary>
        /// Function invoked after layout.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void PostLayout()
        {
            base.PostLayout();
            // alternate row coloring
            bool b = false;
            foreach (ControlBase child in Children)
            {
                CategoryButton button = child as CategoryButton;
                if (button == null)
                    continue;

                button.m_Alt = b;
                b = !b;
            }
        }
    }
}
