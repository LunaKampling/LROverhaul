using System;
using System.Drawing;

namespace Gwen.Controls
{
    /// <summary>
    /// List box row (selectable).
    /// </summary>
    public class ListBoxRow : Label
    {
        /// <summary>
        /// Invoked when the row has been selected.
        /// </summary>
        public event GwenEventHandler<ItemSelectedEventArgs> Selected;
        private bool m_EvenRow;


        /// <summary>
        /// Indicates whether the control is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return m_Selected; }
            set
            {
                //todo: this does not notify parent listbox, causing bugs.
                // needs to be part of parent.selectedrows, and optionally deselect
                // other rows
                m_Selected = value;
            }
        }
        protected override Color CurrentColor
        {
            get
            {

                if (IsDisabled)
                {
                    return Skin.Colors.Text.Disabled;
                }
                else if (IsSelected)
                {
                    return Skin.Colors.Text.Highlight;
                }
                else if (IsHovered)
                {
                    return Skin.Colors.Text.Contrast;
                }
                else
                {
                    return IsTextOverrideVisible ? TextColorOverride : TextColor;
                }
            }
        }
        /// <summary>
        /// Indicates whether the row is even or odd (used for alternate coloring).
        /// </summary>
        public bool EvenRow { get { return m_EvenRow; } set { m_EvenRow = value; } }
        private bool m_Selected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxRow"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ListBoxRow(ControlBase parent)
            : base(parent)
        {
            MouseInputEnabled = true;
            IsSelected = false;
            Dock = Dock.Top;
            Alignment = Pos.Left | Pos.CenterV;
            TextPadding = new Padding(5,3,0,3);
            AutoSizeToContents = true;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawListBoxLine(this, IsSelected, EvenRow);
            base.Render(skin);
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
                OnRowSelected();
            }
        }
        protected virtual void OnRowSelected()
        {
            if (Selected != null)
                Selected.Invoke(this, new ItemSelectedEventArgs(this));
        }
    }
}
