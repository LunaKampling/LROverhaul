using System;

namespace Gwen.Controls
{
    public class Container : ControlBase
    {
        protected readonly ContainerPanel m_Panel;
        public override ControlCollection Children => m_Panel == null ? PrivateChildren : m_Panel.Children;

        /// <summary>
        /// The children of the container that aren't within the panel.
        /// </summary>
        protected ControlCollection PrivateChildren => base.Children;
        protected virtual Margin PanelMargin => Margin.Zero;
        public override Padding Padding
        {
            get => m_Panel.Padding;
            set => m_Panel.Padding = value;
        }
        public override Size InnerSize => PanelBounds.Size;
        internal Rectangle PanelBounds => m_Panel.Bounds;
        public Container(ControlBase parent) : base(parent)
        {
            m_Panel = new ContainerPanel(null)
            {
                Dock = Dock.Fill
            };
            PrivateChildren.Add(m_Panel);
            BoundsOutlineColor = Color.Cyan;
        }
        public override void Invalidate()
        {
            base.Invalidate();
            m_Panel?.Invalidate();
        }
        public override void Redraw()
        {
            base.Redraw();
            m_Panel?.Redraw();
        }
        public override void BringChildToFront(ControlBase control)
        {
            int privateidx = PrivateChildren.IndexOf(control);
            if (privateidx != -1)
            {
                PrivateChildren.BringToFront(privateidx);
            }
            else
            {
                base.BringChildToFront(control);
            }
        }
        public override void SendChildToBack(ControlBase control)
        {
            int privateidx = PrivateChildren.IndexOf(control);
            if (privateidx != -1)
            {
                PrivateChildren.SendToBack(privateidx);
            }
            else
            {
                base.SendChildToBack(control);
            }
        }
        public override void DeleteAllChildren() => m_Panel.DeleteAllChildren();
        public override ControlBase FindChildByName(string name, bool recursive = false) => m_Panel.FindChildByName(name, recursive);
        protected virtual void RenderPanel(Skin.SkinBase skin)
        {
        }
        protected class ContainerPanel(ControlBase parent) : ControlBase(parent)
        {
            public override Margin Margin
            {
                get => Parent is Container parent ? parent.PanelMargin : base.Margin;
                set => throw new Exception("Attempt to set panel margin");
            }

            public override void SendToBack()
            {
                Container container = Parent as Container;
                container.PrivateChildren.SendToBack(this);
            }
            public override void BringToFront()
            {
                Container container = (Container)Parent;
                container.PrivateChildren.BringToFront(this);
            }
            protected override void Render(Skin.SkinBase skin)
            {
                Container parent = Parent as Container;
                parent?.RenderPanel(skin);
            }
            protected override void OnChildAdded(ControlBase child)
            {
                base.OnChildAdded(child);
                if (Parent is Container c)
                {
                    c.OnChildAdded(child);
                }
            }
            protected override void OnChildBoundsChanged(Rectangle oldChildBounds, ControlBase child)
            {
                base.OnChildBoundsChanged(oldChildBounds, child);
                if (Parent is Container c)
                {
                    c.OnChildBoundsChanged(oldChildBounds, child);
                }
            }
            protected override void OnChildRemoved(ControlBase child)
            {
                base.OnChildRemoved(child);
                if (Parent is Container c)
                {
                    c.OnChildRemoved(child);
                }
            }
            protected override void OnChildTouched(ControlBase control)
            {
                base.OnChildTouched(control);
                if (Parent is Container c)
                {
                    c.OnChildTouched(control);
                }
            }
            public override string ToString() => Parent != null ? "(Container) " + Parent.ToString() : "(Container)";
        }
    }
}
