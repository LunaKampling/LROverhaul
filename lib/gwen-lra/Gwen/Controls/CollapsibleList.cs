using System;

namespace Gwen.Controls
{
    /// <summary>
    /// CollapsibleList control. Groups CollapsibleCategory controls.
    /// </summary>
    public class CollapsibleList : ScrollControl
    {
        /// <summary>
        /// Invoked when an entry has been selected.
        /// </summary>
        public event GwenEventHandler<ItemSelectedEventArgs> ItemSelected;

        /// <summary>
        /// Invoked when a category collapsed state has been changed (header button has been pressed).
        /// </summary>
        public event GwenEventHandler<EventArgs> CategoryCollapsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollapsibleList"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CollapsibleList(ControlBase parent) : base(parent)
        {
            MouseInputEnabled = true;
            EnableScroll(false, true);
            AutoHideBars = true;
        }

        // TODO: iterator, make this as function? check if works

        /// <summary>
        /// Selected entry.
        /// </summary>
        public Button GetSelectedButton()
        {
            foreach (ControlBase child in Children)
            {
                if (!(child is CollapsibleCategory cat))
                    continue;

                Button button = cat.GetSelectedButton();

                if (button != null)
                    return button;
            }

            return null;
        }

        protected override void OnChildAdded(ControlBase child)
        {
            base.OnChildAdded(child);
            if (child is CollapsibleCategory cat)
            {
                cat.Selected += OnCategorySelected;
                cat.Collapsed += OnCategoryCollapsed;
            }
        }

        protected override void OnChildRemoved(ControlBase child)
        {
            base.OnChildRemoved(child);
            if (child is CollapsibleCategory cat)
            {
                cat.Selected -= OnCategorySelected;
                cat.Collapsed -= OnCategoryCollapsed;
            }
        }

        /// <summary>
        /// Adds a new category to the list.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        /// <returns>Newly created control.</returns>
        public virtual CollapsibleCategory Add(string categoryName) => new CollapsibleCategory(this) { Text = categoryName };

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawCategoryHolder(this);
            base.Render(skin);
        }

        /// <summary>
        /// Unselects all entries.
        /// </summary>
        public virtual void UnselectAll()
        {
            foreach (ControlBase child in Children)
            {
                if (!(child is CollapsibleCategory cat))
                    continue;

                cat.UnselectAll();
            }
        }

        /// <summary>
        /// Handler for ItemSelected event.
        /// </summary>
        /// <param name="control">Event source: <see cref="CollapsibleList"/>.</param>
		protected virtual void OnCategorySelected(ControlBase control, EventArgs args)
        {
            if (!(control is CollapsibleCategory cat))
                return;

            ItemSelected?.Invoke(this, new ItemSelectedEventArgs(cat));
        }

        /// <summary>
        /// Handler for category collapsed event.
        /// </summary>
        /// <param name="control">Event source: <see cref="CollapsibleCategory"/>.</param>
        protected virtual void OnCategoryCollapsed(ControlBase control, EventArgs args)
        {
            if (!(control is CollapsibleCategory cat))
                return;

            CategoryCollapsed?.Invoke(control, EventArgs.Empty);
        }
    }
}
