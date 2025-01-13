using Gwen.DragDrop;
using Gwen.Input;
using System;
using System.Linq;

namespace Gwen.Controls
{
    public partial class ControlBase
    {
        /// <summary>
        /// Invoked when mouse pointer enters the control.
        /// </summary>
        public event GwenEventHandler<EventArgs> HoverEnter;

        /// <summary>
        /// Invoked when mouse pointer leaves the control.
        /// </summary>
        public event GwenEventHandler<EventArgs> HoverLeave;

        /// <summary>
        /// Invoked when control's bounds have been changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> BoundsChanged;

        /// <summary>
        /// Invoked when the control has been left-clicked.
        /// </summary>
        public virtual event GwenEventHandler<ClickedEventArgs> Clicked;

        /// <summary>
        /// Invoked when the control has been double-left-clicked.
        /// </summary>
        public virtual event GwenEventHandler<ClickedEventArgs> DoubleClicked;

        /// <summary>
        /// Invoked when the control has been right-clicked.
        /// </summary>
        public virtual event GwenEventHandler<ClickedEventArgs> RightClicked;

        /// <summary>
        /// Invoked when the control has been double-right-clicked.
        /// </summary>
        public virtual event GwenEventHandler<ClickedEventArgs> DoubleRightClicked;

        /// <summary>
        /// Returns true if any on click events are set.
        /// </summary>
        internal bool ClickEventAssigned => Clicked != null || RightClicked != null || DoubleClicked != null || DoubleRightClicked != null;
        internal bool IsMouseDepressed = false;
        internal bool IsMouseRightDepressed = false;

        /// <summary>
        /// Focuses the control.
        /// </summary>
        public virtual void Focus()
        {
            if (InputHandler.KeyboardFocus == this)
                return;

            InputHandler.KeyboardFocus?.OnLostKeyboardFocus();

            InputHandler.KeyboardFocus = this;
            OnKeyboardFocus();
            Redraw();
        }

        /// <summary>
        /// Unfocuses the control.
        /// </summary>
        public virtual void Blur()
        {
            if (InputHandler.KeyboardFocus != this)
                return;

            InputHandler.KeyboardFocus = null;
            OnLostKeyboardFocus();
            Redraw();
        }

        /// <summary>
        /// Control has been clicked - invoked by input system. Windows use it to propagate activation.
        /// </summary>
        public virtual void Touch() => Parent?.OnChildTouched(this);

        protected virtual void OnChildTouched(ControlBase control) => Touch();

        /// <summary>
        /// Default accelerator handler.
        /// </summary>
        /// <param name="control">Event source.</param>
        private void DefaultAcceleratorHandler(ControlBase control, EventArgs args) => OnAccelerator();

        /// <summary>
        /// Default accelerator handler.
        /// </summary>
        protected virtual void OnAccelerator()
        {
        }

        /// <summary>
        /// Handler invoked on mouse wheel event.
        /// </summary>
        /// <param name="delta">Scroll delta.</param>
        protected virtual bool OnMouseWheeled(int delta) => m_Parent != null && m_Parent.OnMouseWheeled(delta);

        /// <summary>
        /// Invokes mouse wheeled event (used by input system).
        /// </summary>
        internal bool InputMouseWheeled(int delta) => OnMouseWheeled(delta);

        /// <summary>
        /// Handler invoked on mouse moved event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="dx">X change.</param>
        /// <param name="dy">Y change.</param>
        protected virtual void OnMouseMoved(int x, int y, int dx, int dy)
        {
        }

        /// <summary>
        /// Invokes mouse moved event (used by input system).
        /// </summary>
        internal void InputMouseMoved(int x, int y, int dx, int dy) => OnMouseMoved(x, y, dx, dy);

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected virtual void OnMouseClickedLeft(int x, int y, bool down)
        {
            if (down && Clicked != null)
                Clicked(this, new ClickedEventArgs(x, y, down));
        }

        /// <summary>
        /// Invokes left mouse click event (used by input system).
        /// </summary>
        internal void InputMouseClickedLeft(int x, int y, bool down)
        {
            IsMouseDepressed = down;
            OnMouseClickedLeft(x, y, down);
        }

        /// <summary>
        /// Handler invoked on mouse click (right) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected virtual void OnMouseClickedRight(int x, int y, bool down)
        {
            if (down && RightClicked != null)
                RightClicked(this, new ClickedEventArgs(x, y, down));
        }

        /// <summary>
        /// Invokes right mouse click event (used by input system).
        /// </summary>
        internal void InputMouseClickedRight(int x, int y, bool down)
        {
            IsMouseRightDepressed = down;
            OnMouseClickedRight(x, y, down);
        }

        /// <summary>
        /// Handler invoked on mouse double click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        protected virtual void OnMouseDoubleClickedLeft(int x, int y)
        {
            // [omeg] should this be called?
            // [halfofastaple] Maybe. Technically, a double click is still technically a single click. However, this shouldn't be called here, and
            //                    Should be called by the event handler.
            OnMouseClickedLeft(x, y, true);

            DoubleClicked?.Invoke(this, new ClickedEventArgs(x, y, true));
        }

        /// <summary>
        /// Invokes left double mouse click event (used by input system).
        /// </summary>
        internal void InputMouseDoubleClickedLeft(int x, int y) => OnMouseDoubleClickedLeft(x, y);

        /// <summary>
        /// Handler invoked on mouse double click (right) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        protected virtual void OnMouseDoubleClickedRight(int x, int y)
        {
            // [halfofastaple] See: OnMouseDoubleClicked for discussion on triggering single clicks in a double click event
            OnMouseClickedRight(x, y, true);

            DoubleRightClicked?.Invoke(this, new ClickedEventArgs(x, y, true));
        }

        /// <summary>
        /// Invokes right double mouse click event (used by input system).
        /// </summary>
        internal void InputMouseDoubleClickedRight(int x, int y) => OnMouseDoubleClickedRight(x, y);

        /// <summary>
        /// Handler invoked on mouse cursor entering control's bounds.
        /// </summary>
        protected virtual void OnMouseEntered()
        {
            HoverEnter?.Invoke(this, EventArgs.Empty);

            Redraw();
        }

        /// <summary>
        /// Invokes mouse enter event (used by input system).
        /// </summary>
        internal void InputMouseEntered() => OnMouseEntered();

        /// <summary>
        /// Handler invoked on mouse cursor leaving control's bounds.
        /// </summary>
        protected virtual void OnMouseLeft()
        {
            HoverLeave?.Invoke(this, EventArgs.Empty);

            Redraw();
        }

        /// <summary>
        /// Invokes mouse leave event (used by input system).
        /// </summary>
        internal void InputMouseLeft() => OnMouseLeft();

        // Giver
        public virtual Package DragAndDrop_GetPackage(int x, int y) => m_DragAndDrop_Package;

        // Giver
        public virtual bool DragAndDrop_Draggable() => m_DragAndDrop_Package != null && m_DragAndDrop_Package.IsDraggable;

        // Giver
        public virtual void DragAndDrop_SetPackage(bool draggable, string name = "", object userData = null)
        {
            if (m_DragAndDrop_Package == null)
            {
                m_DragAndDrop_Package = new Package
                {
                    IsDraggable = draggable,
                    Name = name,
                    UserData = userData
                };
            }
        }

        // Giver
        public virtual bool DragAndDrop_ShouldStartDrag() => true;

        // Giver
        public virtual void DragAndDrop_StartDragging(Package package, int x, int y)
        {
            package.HoldOffset = CanvasPosToLocal(new Point(x, y));
            package.DrawControl = this;
        }

        // Giver
        public virtual void DragAndDrop_EndDragging(bool success, int x, int y)
        {
        }

        // Receiver
        public virtual bool DragAndDrop_HandleDrop(Package p, int x, int y)
        {
            DragAndDrop.SourceControl.Parent = this;
            return true;
        }

        // Receiver
        public virtual void DragAndDrop_HoverEnter(Package p, int x, int y)
        {
        }

        // Receiver
        public virtual void DragAndDrop_HoverLeave(Package p)
        {
        }

        // Receiver
        public virtual void DragAndDrop_Hover(Package p, int x, int y)
        {
        }

        // Receiver
        public virtual bool DragAndDrop_CanAcceptPackage(Package p) => false;

        /// <summary>
        /// Handles keyboard accelerator.
        /// </summary>
        /// <param name="accelerator">Accelerator text.</param>
        /// <returns>True if handled.</returns>
        internal virtual bool HandleAccelerator(string accelerator)
        {
            if (InputHandler.KeyboardFocus == this || !AccelOnlyFocus)
            {
                if (m_Accelerators.ContainsKey(accelerator))
                {
                    m_Accelerators[accelerator].Invoke(this, EventArgs.Empty);
                    return true;
                }
            }

            return m_Children.Any(child => child.HandleAccelerator(accelerator));
        }

        /// <summary>
        /// Adds keyboard accelerator.
        /// </summary>
        /// <param name="accelerator">Accelerator text.</param>
        /// <param name="handler">Handler.</param>
        public void AddAccelerator(string accelerator, GwenEventHandler<EventArgs> handler)
        {
            accelerator = accelerator.Trim().ToUpperInvariant();
            m_Accelerators[accelerator] = handler;
        }

        /// <summary>
        /// Adds keyboard accelerator with a default handler.
        /// </summary>
        /// <param name="accelerator">Accelerator text.</param>
        public void AddAccelerator(string accelerator) => m_Accelerators[accelerator] = DefaultAcceleratorHandler;

        /// <summary>
        /// Handler for keyboard events.
        /// </summary>
        /// <param name="key">Key pressed.</param>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyPressed(Key key, bool down = true)
        {
            bool handled = false;
            switch (key)
            {
                case Key.Tab: handled = OnKeyTab(down); break;
                case Key.Space: handled = OnKeySpace(down); break;
                case Key.Home: handled = OnKeyHome(down); break;
                case Key.End: handled = OnKeyEnd(down); break;
                case Key.Return: handled = OnKeyReturn(down); break;
                case Key.Backspace: handled = OnKeyBackspace(down); break;
                case Key.Delete: handled = OnKeyDelete(down); break;
                case Key.Right: handled = OnKeyRight(down); break;
                case Key.Left: handled = OnKeyLeft(down); break;
                case Key.Up: handled = OnKeyUp(down); break;
                case Key.Down: handled = OnKeyDown(down); break;
                case Key.Escape: handled = OnKeyEscape(down); break;
                default: break;
            }

            if (!handled && Parent != null)
                _ = Parent.OnKeyPressed(key, down);

            return handled;
        }

        /// <summary>
        /// Invokes key press event (used by input system).
        /// </summary>
        internal bool InputKeyPressed(Key key, bool down = true) => OnKeyPressed(key, down);

        /// <summary>
        /// Handler for keyboard events.
        /// </summary>
        /// <param name="key">Key pressed.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyReleaseed(Key key) => OnKeyPressed(key, false);

        /// <summary>
        /// Handler for Tab keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyTab(bool down)
        {
            if (!down)
                return true;

            if (GetCanvas().NextTab != null)
            {
                GetCanvas().NextTab.Focus();
                Redraw();
            }

            return true;
        }

        /// <summary>
        /// Handler for Space keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeySpace(bool down) => false;

        /// <summary>
        /// Handler for Return keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyReturn(bool down) => false;

        /// <summary>
        /// Handler for Backspace keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyBackspace(bool down) => false;

        /// <summary>
        /// Handler for Delete keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyDelete(bool down) => false;

        /// <summary>
        /// Handler for Right Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyRight(bool down) => false;

        /// <summary>
        /// Handler for Left Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyLeft(bool down) => false;

        /// <summary>
        /// Handler for Home keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyHome(bool down) => false;

        /// <summary>
        /// Handler for End keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyEnd(bool down) => false;

        /// <summary>
        /// Handler for Up Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyUp(bool down) => false;

        /// <summary>
        /// Handler for Down Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyDown(bool down) => false;

        /// <summary>
        /// Handler for Escape keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnKeyEscape(bool down) => false;

        /// <summary>
        /// Handler for Paste event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected virtual void OnPaste(ControlBase from, EventArgs args)
        {
        }

        /// <summary>
        /// Handler for Copy event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected virtual void OnCopy(ControlBase from, EventArgs args)
        {
        }

        /// <summary>
        /// Handler for Cut event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected virtual void OnCut(ControlBase from, EventArgs args)
        {
        }

        /// <summary>
        /// Handler for Select All event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected virtual void OnSelectAll(ControlBase from, EventArgs args)
        {
        }

        internal void InputCopy(ControlBase from) => OnCopy(from, EventArgs.Empty);

        internal void InputPaste(ControlBase from) => OnPaste(from, EventArgs.Empty);

        internal void InputCut(ControlBase from) => OnCut(from, EventArgs.Empty);

        internal void InputSelectAll(ControlBase from) => OnSelectAll(from, EventArgs.Empty);

        /// <summary>
        /// Handler for gaining keyboard focus.
        /// </summary>
        protected virtual void OnKeyboardFocus()
        {
        }

        /// <summary>
        /// Handler for losing keyboard focus.
        /// </summary>
        protected virtual void OnLostKeyboardFocus()
        {
        }

        /// <summary>
        /// Handler for character input event.
        /// </summary>
        /// <param name="chr">Character typed.</param>
        /// <returns>True if handled.</returns>
        protected virtual bool OnChar(char chr) => false;

        internal bool InputChar(char chr) => OnChar(chr);
    }
}