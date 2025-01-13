using Gwen.Input;
using System;

namespace Gwen.Controls
{
    /// <summary>
    /// Tab header.
    /// </summary>
    public class TabButton : Button
    {
        private TabControl m_Control;

        /// <summary>
        /// Indicates whether the tab is active.
        /// </summary>
        public bool IsActive => Page != null && Page.IsVisible;

        // TODO: remove public access
        public TabControl TabControl
        {
            get => m_Control;
            set
            {
                if (value == m_Control)
                    return;
                m_Control?.OnLoseTab(this);
                m_Control = value;
            }
        }

        /// <summary>
        /// Interior of the tab.
        /// </summary>
        public TabPage Page { get; set; }

        /// <summary>
        /// Determines whether the control should be clipped to its bounds while rendering.
        /// </summary>
        protected override bool ShouldClip => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TabButton(ControlBase parent)
            : base(parent)
        {
            DragAndDrop_SetPackage(true, "TabButtonMove");
            Alignment = Pos.Top | Pos.Left;
            TextPadding = new Padding(5, 3, 3, 3);
            Padding = Padding.Two;
            // KeyboardInputEnabled = true;
            AutoSizeToContents = true;
            ShouldDrawBackground = false;
        }

        public override void DragAndDrop_StartDragging(DragDrop.Package package, int x, int y) => IsHidden = true;

        public override void DragAndDrop_EndDragging(bool success, int x, int y)
        {
            IsHidden = false;
            IsDepressed = false;
        }

        public override bool DragAndDrop_ShouldStartDrag() => m_Control.AllowReorder;

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            base.Render(skin);
            switch (m_Control.TabStrip.Dock)
            {
                case Dock.Top:
                    skin.DrawTabButton(this, IsActive, Pos.Top);
                    break;
                case Dock.Left:
                    skin.DrawTabButton(this, IsActive, Pos.Left);
                    break;
                case Dock.Bottom:
                    skin.DrawTabButton(this, IsActive, Pos.Bottom);
                    break;
                case Dock.Right:
                    skin.DrawTabButton(this, IsActive, Pos.Right);
                    break;
            }
        }

        /// <summary>
        /// Handler for Right Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyRight(bool down)
        {
            if (down)
            {
                int count = Parent.Children.Count;
                int me = Parent.Children.IndexOf(this);
                if (me + 1 < count)
                {
                    ControlBase nextTab = Parent.Children[me + 1];
                    TabControl.OnTabPressed(nextTab, EventArgs.Empty);
                    InputHandler.KeyboardFocus = nextTab;
                }
            }

            return true;
        }

        /// <summary>
        /// Handler for Left Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyLeft(bool down)
        {
            if (down)
            {
                _ = Parent.Children.Count;
                int me = Parent.Children.IndexOf(this);
                if (me - 1 >= 0)
                {
                    ControlBase prevTab = Parent.Children[me - 1];
                    TabControl.OnTabPressed(prevTab, EventArgs.Empty);
                    InputHandler.KeyboardFocus = prevTab;
                }
            }

            return true;
        }

        protected override Color CurrentColor => IsActive
                    ? IsDisabled ? Skin.Colors.Text.Disabled : Skin.Colors.Text.Foreground
                    : IsDisabled
                    ? Skin.Colors.Text.Disabled
                    : IsDepressed ? Skin.Colors.Text.ContrastLow : IsHovered ? Skin.Colors.Text.ContrastLow : Skin.Colors.Text.Foreground;
    }
}
