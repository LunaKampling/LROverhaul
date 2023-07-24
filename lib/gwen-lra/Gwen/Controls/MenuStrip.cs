namespace Gwen.Controls
{
    /// <summary>
    /// Menu strip.
    /// </summary>
    public class MenuStrip : Menu
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuStrip"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public MenuStrip(ControlBase parent)
            : base(parent)
        {
            _ = SetBounds(0, 0, 200, 22);
            Dock = Dock.Top;
            m_Panel.Padding = Padding.Zero;
        }

        /// <summary>
        /// Closes the current menu.
        /// </summary>
        public override void Close()
        {

        }

        /// <summary>
        /// Renders under the actual control (shadows etc).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void RenderUnder(Skin.SkinBase skin)
        {
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawMenuStrip(this);

        /// <summary>
        /// Determines whether the menu should open on mouse hover.
        /// </summary>
        protected override bool ShouldHoverOpenMenu => IsMenuOpen();

        /// <summary>
        /// Add item handler.
        /// </summary>
        /// <param name="item">Item added.</param>
        protected override void OnAddItem(MenuItem item)
        {
            item.Dock = Dock.Left;
            item.TextPadding = new Padding(5, 0, 5, 0);
            item.Padding = new Padding(10, 5, 10, 5);
            item.IsOnStrip = true;
            item.HoverEnter += OnHoverItem;
            _ = item.SizeToChildren();
            _ = SizeToChildren();
        }
    }
}
