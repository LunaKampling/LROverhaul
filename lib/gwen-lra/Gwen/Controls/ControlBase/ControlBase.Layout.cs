using Gwen.Input;
using System;
using System.Diagnostics;
using System.Drawing;
namespace Gwen.Controls
{
    public partial class ControlBase
    {
        public static bool LogLayout = false;
        private bool m_AutoSizeToContents;
        private int m_LayoutSuspendedCount = 0;
        internal bool NeedsLayout { get; private set; }
        /// <summary>
        /// Bounds adjusted by padding.
        /// </summary>
        internal virtual Rectangle InnerBounds
        {
            get
            {
                Padding padding = Padding;
                Rectangle bounds = Bounds;
                bounds.X += padding.Left;
                bounds.Width -= padding.Left + padding.Right;
                bounds.Y += padding.Top;
                bounds.Height -= padding.Top + padding.Bottom;
                return bounds;
            }
        }
        /// <summary>
        /// Bounds adjusted by margin.
        /// </summary>
        internal virtual Rectangle OuterBounds
        {
            get
            {
                Margin margin = Margin;
                Rectangle bounds = Bounds;
                bounds.X += margin.Left;
                bounds.Width -= margin.Left + margin.Right;
                bounds.Y += margin.Top;
                bounds.Height -= margin.Top + margin.Bottom;
                return bounds;
            }
        }
        /// <summary>
        /// Determines if the control should autosize to its text.
        /// </summary>
        public virtual bool AutoSizeToContents
        {
            get => m_AutoSizeToContents;
            set
            {
                m_AutoSizeToContents = value;
                Invalidate();
                InvalidateParent();
            }
        }
        public PositionerDelegate Positioner = null;
        /// <summary>
        /// Function invoked after layout.
        /// </summary>
        protected virtual void PostLayout()
        {
            if (Dock == Dock.None && Positioner != null)
            {
                Point pos = Positioner.Invoke(this);
                SetPosition(pos.X, pos.Y);
            }
        }

        /// <summary>
        /// Function that does the layout process. Applying child docks, 
        /// rearranging controls, etc.
        /// This function should not change our bounds.
        /// </summary>
        /// <param name="size">Our current size</param>
        protected virtual void ProcessLayout(Size size)
        {
            ControlBase control = this;
            Rectangle bounds = new Rectangle(control.Padding.Left,
                                            control.Padding.Top,
                                            size.Width - (control.Padding.Right + control.Padding.Left),
                                            size.Height - (control.Padding.Bottom + control.Padding.Top));
            foreach (ControlBase child in control.m_Children)
            {
                if (child.IsHidden || child.Dock == Dock.Fill)
                    continue;
                // Ignore fill for now, as it uses total free space.
                Rectangle dock = CalculateBounds(child, ref bounds);
                _ = child.SetBounds(dock);
            }
            foreach (ControlBase child in control.m_Children)
            {
                if (child.IsHidden || child.Dock != Dock.Fill)
                    continue;
                // Fill uses leftover space
                Rectangle dock = CalculateBounds(child, ref bounds);
                _ = child.SetBounds(dock);
            }
        }
        public void SuspendLayout() => m_LayoutSuspendedCount++;
        public void ResumeLayout(bool layout)
        {
            if (m_LayoutSuspendedCount == 0)
                throw new InvalidOperationException(
                    "Layout resumed but was not suspended");
            m_LayoutSuspendedCount--;

            if (layout)
                _ = Layout(true, false);
        }
        /// <summary>
        /// Recursively lays out the control's interior according to alignment, margin, padding, dock etc.
        /// If AutoSizeToContents is enabled, sizes the control before layout.
        /// </summary>
        public void Layout(bool force = true) => Layout(force, false);
        /// <summary>
        /// Recursively lays out the control's interior according to alignment, margin, padding, dock etc.
        /// If AutoSizeToContents is enabled, sizes the control before layout.
        /// </summary>
        protected virtual bool Layout(bool force, bool recursioncheck = false)
        {
            // TODO: layout and processlayout do not play well with nested autosize controls
            // REPLY: possibly fixed?
            if (IsHidden || m_LayoutSuspendedCount > 0)
                return false;
            bool shouldlayout = NeedsLayout || force;
            bool ret = shouldlayout;
            if (shouldlayout)
            {
                // If we have a dock property, our parent handles sizing us.
                bool autosize = AutoSizeToContents && Dock == Dock.None;
                if (autosize)
                {
                    Size sz = ClampSize(this, GetSizeToFitContents());
                    _ = SetBounds(X, Y, sz.Width, sz.Height);
                    // SetBounds can be overridden, so just get size for
                    // processlayout later.
                }
                if (Dock != Dock.None)
                {
                    InvalidateParent();
                }
                Rectangle oldbounds = Bounds;
                ProcessLayout(oldbounds.Size);
                if (Bounds.Size != oldbounds.Size)
                {
                    throw new InvalidOperationException(
                        "Control cannot resize itself during ProcessLayout");
                }
                NeedsLayout = false;
                foreach (ControlBase child in m_Children)
                {
                    child.Layout(force);
                }
                PostLayout();
                if (LogLayout)
                    Console.WriteLine("Layout performed on " + ToString());
            }
            else
            {
                foreach (ControlBase child in m_Children)
                {
                    _ = child.Layout(false, false);
                }
                if (NeedsLayout)
                {
                    if (recursioncheck)
                        throw new Exception("Layout recursion detected.");
                    _ = Layout(false, true);
                }
            }
            if (IsTabable)
            {
                if (GetCanvas().FirstTab == null)
                    GetCanvas().FirstTab = this;
                if (GetCanvas().NextTab == null)
                    GetCanvas().NextTab = this;
            }

            if (InputHandler.KeyboardFocus == this)
            {
                GetCanvas().NextTab = null;
            }
            return ret;
        }
        /// <summary>
        /// Positions the control inside its parent relative to its edges
        /// </summary>
        /// <param name="pos">Target position.</param>
        /// <param name="xpadding">X padding.</param>
        /// <param name="ypadding">Y padding.</param>
        public void AlignToEdge(Pos pos, int xpadding = 0, int ypadding = 0) => AlignToEdge(pos, new Padding(xpadding, ypadding, xpadding, ypadding));
        /// <summary>
        /// Positions the control inside its parent relative to its edges
        /// </summary>
        /// <param name="pos">Target position.</param>
        /// <param name="xpadding">X padding.</param>
        /// <param name="ypadding">Y padding.</param>
        public void AlignToEdge(Pos pos, Padding padding) => AlignToEdge(pos, padding, X, Y);
        /// <summary>
        /// Positions the control inside its parent relative to its edges
        /// </summary>
        /// <param name="pos">Target position.</param>
        /// <param name="xpadding">X padding.</param>
        /// <param name="ypadding">Y padding.</param>
        public virtual void AlignToEdge(Pos pos, Padding padding, int startx, int starty)
        {
            int newx = startx;
            int newy = starty;

            if (pos.HasFlag(Pos.Left))
            {
                newx = Parent.Padding.Left + padding.Left;
            }
            else if (pos.HasFlag(Pos.Right))
            {
                newx = Parent.Width - Width - (Parent.Padding.Right + padding.Right);
            }
            else if (pos.HasFlag(Pos.CenterH))
            {
                int left = Parent.Padding.Left + padding.Left;
                int right = Parent.Width - Width - (Parent.Padding.Right + padding.Right);
                newx = left + (right - left) / 2;
            }

            if (pos.HasFlag(Pos.Top))
            {
                newy = Parent.Padding.Top + padding.Top;
            }
            else if (pos.HasFlag(Pos.Bottom))
            {
                newy = Parent.Height - Height - (Parent.Padding.Bottom + padding.Bottom);
            }
            else if (pos.HasFlag(Pos.CenterV))
            {
                int top = Parent.Padding.Top + padding.Top;
                int bot = Parent.Height - Height - (Parent.Padding.Bottom + padding.Bottom);
                newy = top + (bot - top) / 2;
            }

            SetPosition(newx, newy);
        }
        /// <summary>
        /// Gets the minimum size of the control based on its children
        /// </summary>
        public virtual Size GetSizeToFitContents()
        {
            ControlBase control = this;
            // We need minimum size
            Size size = Size.Empty;
            Size maxundocked = Size.Empty;
            int verticaldock = 0;
            int horzdock = 0;
            foreach (ControlBase child in control.m_Children)
            {
                if (child.IsHidden)
                    continue;
                // Ignore fill for now, as it uses total free space.
                if (child.Dock != Dock.Fill)
                {
                    Size childsize = child.Bounds.Size;
                    if (child.AutoSizeToContents)
                    {
                        childsize = child.GetSizeToFitContents();
                    }
                    childsize = ClampSize(child, childsize);
                    childsize.Width += child.Margin.Width;
                    childsize.Height += child.Margin.Height;
                    if (child.Dock == Dock.None)
                    {
                        // Using the childs coordinates has the side effect
                        // of meaning controls in negative space do not count
                        // towards our maximum size. the logic is that if we
                        // resize to fit it, what would that do to make the
                        // child visible?
                        maxundocked.Width = Math.Max(maxundocked.Width, child.X + childsize.Width);
                        maxundocked.Height = Math.Max(maxundocked.Height, child.Y + childsize.Height);
                        continue;
                    }
                    int horzfreespace = size.Width - horzdock;
                    int vertfreespace = size.Height - verticaldock;
                    if (child.Dock == Dock.Top || child.Dock == Dock.Bottom)
                    {
                        verticaldock += childsize.Height;
                        size.Height += Math.Max(0, childsize.Height - vertfreespace);
                        ;
                        if (childsize.Width > horzfreespace)
                        {
                            // Size to be wide enough to fit
                            size.Width += childsize.Width - horzfreespace;
                        }
                    }
                    else if (child.Dock == Dock.Right || child.Dock == Dock.Left)
                    {
                        horzdock += childsize.Width;
                        size.Width += Math.Max(0, childsize.Width - horzfreespace);
                        if (childsize.Height > vertfreespace)
                        {
                            // Size to be tall enough to fit
                            size.Height += childsize.Height - vertfreespace;
                        }
                    }
                }
            }
            // There could be more than one fill control, but we arent advanced
            // enough to give them all equal spacing. So we just make sure we
            // can fit the largest one
            Size fill = Size.Empty;
            foreach (ControlBase child in control.m_Children)
            {
                if (child.IsHidden)
                    continue;
                if (child.Dock == Dock.Fill)
                {
                    // Fill is lowest priority
                    Size childsize = child.Bounds.Size;
                    if (child.AutoSizeToContents)
                    {
                        if (child.AutoSizeToContents)
                        {
                            childsize = child.GetSizeToFitContents();
                        }
                        childsize = ClampSize(child, childsize);
                        childsize.Width += child.Margin.Width;
                        childsize.Height += child.Margin.Height;
                    }
                    else
                    {
                        childsize.Width = child.MinimumSize.Width + child.Margin.Width;
                        childsize.Height = child.MinimumSize.Height + child.Margin.Height;
                    }
                    fill.Width = Math.Max(fill.Width, childsize.Width);
                    fill.Height = Math.Max(fill.Height, childsize.Height);
                }
            }
            int horzavail = size.Width - horzdock;
            int vertavail = size.Height - verticaldock;
            size.Width += Math.Max(0, fill.Width - horzavail);
            size.Height += Math.Max(0, fill.Height - vertavail);
            // If theres a control placed somewhere greater than our dock needs,
            // size to that.
            size.Width = Math.Max(maxundocked.Width, size.Width);
            size.Height = Math.Max(maxundocked.Height, size.Height);

            size.Width += control.Padding.Left + control.Padding.Right;
            size.Height += control.Padding.Top + control.Padding.Bottom;
            return size;
        }
        protected static Size ClampSize(ControlBase control, Size size) => new Size(
                Math.Min(Math.Max(control.MinimumSize.Width, size.Width), control.MaximumSize.Width),
                Math.Min(Math.Max(control.MinimumSize.Height, size.Height), control.MaximumSize.Height));
        /// <summary>
        /// Calculates a child's new bounds according to its Dock property and applies AutoSizeToContents
        /// </summary>
        /// <param name="control">The child control.</param>
        /// <param name="area">The area available to dock to.</param>
        /// <returns>New bounds of the control.</returns>
        public static Rectangle CalculateBounds(ControlBase control, ref Rectangle area)
        {
            if (control.IsHidden)
                return control.Bounds;
            Rectangle ret = control.Bounds;
            if (control.AutoSizeToContents)
            {
                ret.Size = control.GetSizeToFitContents();
            }
            ret.Size = ClampSize(control, ret.Size);
            if (control.Dock == Dock.None)
            {
                return ret;
            }
            Margin margin = control.Margin;
            if (control.Dock == Dock.Fill)
            {
                ret = new Rectangle(area.X + control.Margin.Left,
                                    area.Y + control.Margin.Top,
                                    area.Width - control.Margin.Width,
                                    area.Height - control.Margin.Height);
            }
            else if (control.Dock == Dock.Left)
            {
                ret = new Rectangle(area.X + margin.Left,
                                area.Y + margin.Top,
                                ret.Width,
                                area.Height - margin.Height);

                int width = margin.Width + ret.Width;
                area.X += width;
                area.Width -= width;
            }
            else if (control.Dock == Dock.Right)
            {
                ret = new Rectangle(area.X + area.Width - ret.Width - margin.Right,
                                area.Y + margin.Top,
                                ret.Width,
                                area.Height - margin.Height);

                int width = margin.Width + ret.Width;
                area.Width -= width;
            }
            else if (control.Dock == Dock.Top)
            {
                ret = new Rectangle(area.X + margin.Left,
                                area.Y + margin.Top,
                                area.Width - margin.Width,
                                ret.Height);

                int height = margin.Height + ret.Height;
                area.Y += height;
                area.Height -= height;
            }
            else if (control.Dock == Dock.Bottom)
            {
                ret = new Rectangle(area.X + margin.Left,
                                area.Y + area.Height - ret.Height - margin.Bottom,
                                area.Width - margin.Width,
                                ret.Height);
                int height = margin.Height + ret.Height;
                area.Height -= height;
            }
            else
            {
                throw new Exception("Unhandled Dock Pos" + control.Dock);
            }
            return ret;
        }
        /// <summary>
        /// Resizes the control to fit its children.
        /// </summary>
        /// <param name="width">Determines whether to change control's width.</param>
        /// <param name="height">Determines whether to change control's height.</param>
        /// <returns>True if bounds changed.</returns>
        public virtual bool SizeToChildren(bool width = true, bool height = true)
        {
            Size size = ClampSize(this, GetSizeToFitContents());
            return SetSize(width ? size.Width : Width, height ? size.Height : Height);
        }

        public void FitChildrenToSize()
        {
            foreach (ControlBase child in Children)
            {
                // Push them back into view if they are outside it
                child.X = Math.Min(Bounds.Width, child.X + child.Width) - child.Width;
                child.Y = Math.Min(Bounds.Height, child.Y + child.Height) - child.Height;

                // Non-negative has priority, so do it second.
                child.X = Math.Max(0, child.X);
                child.Y = Math.Max(0, child.Y);
            }
        }

        /// <summary>
        /// Sends the control to the bottom of the parent's visibility stack.
        /// </summary>
        public virtual void SendToBack()
        {
            if (m_Parent == null)
                return;
            m_Parent.SendChildToBack(this);
        }

        /// <summary>
        /// Brings the control to the top of the parent's visibility stack.
        /// </summary>
        public virtual void BringToFront()
        {
            if (m_Parent == null)
                return;
            m_Parent.BringChildToFront(this);
        }
        /// <summary>
        /// Sends the child to the bottom of the visibility stack.
        /// </summary>
        public virtual void SendChildToBack(ControlBase control)
        {
            int idx = Children.IndexOf(control);
            if (idx != -1)
            {
                Children.SendToBack(idx);
                if (AutoSizeToContents)
                    InvalidateParent();
                Redraw();
            }
            else
            {
                throw new Exception("Unable to send control to back of parent -- index missing");
            }
        }
        /// <summary>
        /// Sends the child to the top of the visibility stack.
        /// </summary>
        public virtual void BringChildToFront(ControlBase control)
        {
            int idx = Children.IndexOf(control);
            if (idx != -1)
            {
                Children.BringToFront(idx);
                if (AutoSizeToContents)
                    InvalidateParent();
                Redraw();
            }
            else
            {
                throw new Exception("Unable to send control to front of parent -- index missing");
            }
        }

        /// <summary>
        /// Attaches specified control as a child of this one.
        /// </summary>
        /// <remarks>
        /// If InnerPanel is not null, it will become the parent.
        /// </remarks>
        /// <param name="child">Control to be added as a child.</param>
        public virtual void AddChild(ControlBase child)
        {
            if (child == null)
                throw new NullReferenceException("Cannot add null child");
            if (!Children.Contains(child))
                Children.Add(child);// The collection will run events.
            else
            {
                Debug.Assert(child.m_Parent == this,
                "Child is already contained but doesnt have us as its parent");
            }
            child.m_Parent = this;
        }

        /// <summary>
        /// Detaches specified control from this one.
        /// </summary>
        /// <param name="child">Child to be removed.</param>
        /// <param name="dispose">Determines whether the child should be disposed (added to delayed delete queue).</param>
        public virtual void RemoveChild(ControlBase child, bool dispose)
        {
            if (child == null)
                throw new NullReferenceException("Cannot remove null child");
            _ = Children.Remove(child);// The collection will run events.

            if (dispose)
            {
                Canvas canvas = GetCanvas();
                if (canvas != null)
                    canvas.AddDelayedDelete(child);
                else
                    child.Dispose();
            }
        }

        /// <summary>
        /// Checks if the given control is a child of this instance.
        /// </summary>
        /// <param name="child">Control to examine.</param>
        /// <returns>True if the control is out child.</returns>
        public bool IsChild(ControlBase child) => Children.Contains(child);

        /// <summary>
        /// Removes all children (and disposes them).
        /// </summary>
        public virtual void DeleteAllChildren()
        {
            // TODO: probably shouldn't invalidate after each removal
            while (m_Children.Count > 0)
                RemoveChild(m_Children[0], true);
        }

        /// <summary>
        /// Moves the control by a specific amount.
        /// </summary>
        /// <param name="x">X-axis movement.</param>
        /// <param name="y">Y-axis movement.</param>
        public virtual void MoveBy(int x, int y) => SetBounds(X + x, Y + y, Width, Height);

        /// <summary>
        /// Moves the control to a specific point.
        /// </summary>
        /// <param name="x">Target x coordinate.</param>
        /// <param name="y">Target y coordinate.</param>
        public virtual void MoveTo(float x, float y) => MoveClampToParent((int)x, (int)y);

        /// <summary>
        /// Moves the control to a specific point, clamping on paren't bounds if RestrictToParent is set.
        /// </summary>
        /// <param name="x">Target x coordinate.</param>
        /// <param name="y">Target y coordinate.</param>
        public virtual Point MoveClampToParent(int x, int y)
        {
            if (RestrictToParent && (Parent != null))
            {
                ControlBase parent = Parent;
                x = Util.Clamp(x,
                    parent.Padding.Left + Margin.Left,
                    parent.Width - parent.Padding.Right - (Width + Margin.Right));
                y = Util.Clamp(y,
                    parent.Padding.Top + Margin.Top,
                    parent.Height - parent.Padding.Bottom - (Height + Margin.Bottom));
            }

            _ = SetBounds(x, y, Width, Height);
            return new Point(x, y);
        }

        /// <summary>
        /// Sets the control position.
        /// </summary>
        /// <param name="x">Target x coordinate.</param>
        /// <param name="y">Target y coordinate.</param>
        public virtual void SetPosition(int x, int y) => SetBounds(x, y, Width, Height);

        /// <summary>
        /// Sets the control size.
        /// </summary>
        /// <param name="width">New width.</param>
        /// <param name="height">New height.</param>
        /// <returns>True if bounds changed.</returns>
        public virtual bool SetSize(int width, int height) => SetBounds(X, Y, width, height);

        /// <summary>
        /// Sets the control bounds.
        /// </summary>
        /// <param name="bounds">New bounds.</param>
        /// <returns>True if bounds changed.</returns>
        public virtual bool SetBounds(Rectangle bounds) => SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);

        /// <summary>
        /// Sets the control bounds.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>
        /// True if bounds changed.
        /// </returns>
        public virtual bool SetBounds(float x, float y, float width, float height) => SetBounds((int)x, (int)y, (int)width, (int)height);

        /// <summary>
        /// Sets the control bounds.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>
        /// True if bounds changed.
        /// </returns>
        public virtual bool SetBounds(int x, int y, int width, int height)
        {
            width = Math.Max(
                MinimumSize.Width,
                Math.Min(MaximumSize.Width, width));
            height = Math.Max(
                MinimumSize.Height,
                Math.Min(MaximumSize.Height, height));
            if (m_Bounds.X == x &&
                m_Bounds.Y == y &&
                m_Bounds.Width == width &&
                m_Bounds.Height == height)
                return false;

            Rectangle oldBounds = Bounds;

            m_Bounds.X = x;
            m_Bounds.Y = y;

            m_Bounds.Width = width;
            m_Bounds.Height = height;

            OnBoundsChanged(oldBounds);

            BoundsChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }

        /// <summary>
        /// Handler invoked when control's bounds change.
        /// </summary>
        /// <param name="oldBounds">Old bounds.</param>
        protected virtual void OnBoundsChanged(Rectangle oldBounds)
        {
            // Anything that needs to update on size changes
            // Iterate my children and tell them I've changed
            Parent?.OnChildBoundsChanged(oldBounds, this);

            if (m_Bounds.Width != oldBounds.Width || m_Bounds.Height != oldBounds.Height)
            {
                Invalidate();
            }
            if (m_Bounds.X != oldBounds.X || m_Bounds.Y != oldBounds.Y)
                Redraw();
        }
        /// <summary>
        /// Handler invoked when control children's bounds change.
        /// </summary>
        protected virtual void OnChildBoundsChanged(Rectangle oldChildBounds, ControlBase child)
        {
            if (AutoSizeToContents || child.Dock != Dock.None)
                Invalidate();
        }
        /// <summary>
        /// Handler invoked when a child is added.
        /// </summary>
        /// <param name="child">Child added.</param>
        protected virtual void OnChildAdded(ControlBase child) => Invalidate();

        /// <summary>
        /// Handler invoked when a child is removed.
        /// </summary>
        /// <param name="child">Child removed.</param>
        protected virtual void OnChildRemoved(ControlBase child) => Invalidate();
    }
}