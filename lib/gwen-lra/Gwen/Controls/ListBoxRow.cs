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

        /// <summary>
        /// Indicates whether the control is selected.
        /// </summary>
        public bool IsSelected { get; set; }
        protected override Color CurrentColor => IsDisabled
                    ? Skin.Colors.Text.Disabled
                    : IsSelected
                        ? Skin.Colors.Text.Highlight
                        : IsHovered ? Skin.Colors.Text.Contrast : IsTextOverrideVisible ? TextColorOverride : TextColor;
        /// <summary>
        /// Indicates whether the row is even or odd (used for alternate coloring).
        /// </summary>
        public bool EvenRow { get; set; }

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
            TextPadding = new Padding(5, 3, 0, 3);
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
        protected virtual void OnRowSelected() => Selected?.Invoke(this, new ItemSelectedEventArgs(this));
    }
}
