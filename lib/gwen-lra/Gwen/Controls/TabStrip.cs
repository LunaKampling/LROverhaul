using System;
using System.Drawing;
using Gwen.ControlInternal;
using Gwen.DragDrop;

namespace Gwen.Controls
{
    /// <summary>
    /// Tab strip - groups TabButtons and allows reordering.
    /// </summary>
    public class TabStrip : ControlBase
    {
        private ControlBase m_TabDragControl;
        private bool m_AllowReorder;

        /// <summary>
        /// Determines whether it is possible to reorder tabs by mouse dragging.
        /// </summary>
        public bool AllowReorder { get { return m_AllowReorder; } set { m_AllowReorder = value; } }

        /// <summary>
        /// Determines whether the control should be clipped to its bounds while rendering.
        /// </summary>
        protected override bool ShouldClip
        {
            get { return false; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabStrip"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TabStrip(ControlBase parent)
            : base(parent)
        {
            m_AllowReorder = false;
            AutoSizeToContents = true;
        }

        /// <summary>
        /// Strip position (top/left/right/bottom).
        /// </summary>
        public Dock StripPosition
        {
            get { return Dock; }
            set
            {
                Dock = value;
                if (Dock == Dock.Top)
                    Padding = new Padding(5, 0, 0, 0);
                else
                    throw new NotImplementedException("Alternative tab control docks are not inplemented properly yet.");
                // if (Dock == Dock.Left)
                //     Padding = new Padding(0, 5, 0, 0);
                // if (Dock == Dock.Bottom)
                //     Padding = new Padding(0, 0, 0, 5);
                // if (Dock == Dock.Right)
                //     Padding = new Padding(0, 5, 0, 0);
            }
        }

        public override bool DragAndDrop_HandleDrop(Package p, int x, int y)
        {
            Point LocalPos = CanvasPosToLocal(new Point(x, y));

            TabButton button = DragAndDrop.SourceControl as TabButton;
            TabControl tabControl = Parent as TabControl;
            if (tabControl != null && button != null)
            {
                if (button.TabControl != tabControl)
                {
                    // We've moved tab controls!
                    tabControl.AddPage(button);
                }
            }

            ControlBase droppedOn = GetControlAt(LocalPos.X, LocalPos.Y);
            if (droppedOn != null)
            {
                // Point dropPos = droppedOn.CanvasPosToLocal(new Point(x, y));
                //DragAndDrop.SourceControl.BringNextToControl(droppedOn, dropPos.X > droppedOn.Width/2);
                droppedOn.BringToFront();
            }
            else
            {
                DragAndDrop.SourceControl.BringToFront();
            }
            return true;
        }

        public override bool DragAndDrop_CanAcceptPackage(Package p)
        {
            if (!m_AllowReorder)
                return false;

            if (p.Name == "TabButtonMove")
                return true;

            return false;
        }

        protected override void ProcessLayout(Size size)
        {
            foreach (var child in Children)
            {
                SetupButton(child);
            }
            base.ProcessLayout(size);
        }
        protected override void OnChildAdded(ControlBase child)
        {
            if (child is TabButton)
            {
                SetupButton(child);
            }
            else
            {
                throw new Exception("Child of TabStrib must be a tab button");
            }
            base.OnChildAdded(child);
        }
        private void SetupButton(ControlBase child)
        {
            if (Children.Count == 0 || !(child is TabButton))
                return;//???
            var first = child == Children[0];
            if (Dock == Dock.Top)
            {
                child.Margin = new Margin(first ? 0 : -1, 0, 0, 0);
                child.Dock = Dock.Left;
            }

            if (Dock == Dock.Left)
            {
                child.Margin = new Margin(0, first ? 0 : -1, 0, 0);
                child.Dock = Dock.Top;
            }

            if (Dock == Dock.Right)
            {
                child.Margin = new Margin(0, first ? 0 : -1, 0, 0);
                child.Dock = Dock.Top; ;
            }

            if (Dock == Dock.Bottom)
            {
                child.Margin = new Margin(first ? 0 : -1, 0, 0, 0);
                child.Dock = Dock.Left;
            }
        }
        protected override void OnBoundsChanged(Rectangle oldBounds)
        {
            if (oldBounds != Bounds)
            {
                InvalidateParent();
            }
        }
        public override void DragAndDrop_HoverEnter(Package p, int x, int y)
        {
            if (m_TabDragControl != null)
            {
                throw new InvalidOperationException("ERROR! TabStrip::DragAndDrop_HoverEnter");
            }

            m_TabDragControl = new Highlight(this);
            m_TabDragControl.MouseInputEnabled = false;
            m_TabDragControl.SetSize(3, Height);
        }

        public override void DragAndDrop_HoverLeave(Package p)
        {
            if (m_TabDragControl != null)
            {
                RemoveChild(m_TabDragControl, false); // [omeg] need to do that explicitely
                m_TabDragControl.Dispose();
            }
            m_TabDragControl = null;
        }

        public override void DragAndDrop_Hover(Package p, int x, int y)
        {
            Point localPos = CanvasPosToLocal(new Point(x, y));

            ControlBase droppedOn = GetControlAt(localPos.X, localPos.Y);
            if (droppedOn != null && droppedOn != this)
            {
                Point dropPos = droppedOn.CanvasPosToLocal(new Point(x, y));
                m_TabDragControl.SetBounds(new Rectangle(0, 0, 3, Height));
                m_TabDragControl.BringToFront();
                m_TabDragControl.SetPosition(droppedOn.X - 1, 0);

                if (dropPos.X > droppedOn.Width / 2)
                {
                    m_TabDragControl.MoveBy(droppedOn.Width - 1, 0);
                }
                m_TabDragControl.Dock = Dock.None;
            }
            else
            {
                m_TabDragControl.Dock = Dock.Left;
                m_TabDragControl.BringToFront();
            }
        }
    }
}
