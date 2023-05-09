using Gwen.Anim;
using Gwen.DragDrop;
using Gwen.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

namespace Gwen.Controls
{
    public class Container : ControlBase
    {
        protected readonly ContainerPanel m_Panel;
        public override ControlCollection Children
        {
            get
            {
                return m_Panel == null ? PrivateChildren : m_Panel.Children;
            }
        }

        /// <summary>
        /// The children of the container that aren't within the panel.
        /// </summary>
        protected ControlCollection PrivateChildren
        {
            get
            {
                return base.Children;
            }
        }
        protected virtual Margin PanelMargin
        {
            get
            {
                return Margin.Zero;
            }
        }
        public override Padding Padding
        {
            get
            {
                return m_Panel.Padding;
            }
            set
            {
                m_Panel.Padding = value;
            }
        }
        public override Size InnerSize { get { return PanelBounds.Size; } }
        internal Rectangle PanelBounds => m_Panel.Bounds;
        public Container(ControlBase parent) : base(parent)
        {
            m_Panel = new ContainerPanel(null);
            m_Panel.Dock = Dock.Fill;
            PrivateChildren.Add(m_Panel);
            this.BoundsOutlineColor = Color.Cyan;
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
            var privateidx = PrivateChildren.IndexOf(control);
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
            var privateidx = PrivateChildren.IndexOf(control);
            if (privateidx != -1)
            {
                PrivateChildren.SendToBack(privateidx);
            }
            else
            {
                base.SendChildToBack(control);
            }
        }
        public override void DeleteAllChildren()
        {
            m_Panel.DeleteAllChildren();
        }
        public override ControlBase FindChildByName(string name, bool recursive = false)
        {
            return m_Panel.FindChildByName(name, recursive);
        }
        protected virtual void RenderPanel(Skin.SkinBase skin)
        {
        }
        protected class ContainerPanel : ControlBase
        {
            public override Margin Margin
            {
                get
                {
                    var parent = Parent as Container;

                    return parent != null ? parent.PanelMargin : base.Margin;
                }
                set
                {
                    throw new Exception("Attempt to set panel margin");
                }
            }
            public ContainerPanel(ControlBase parent) : base(parent)
            {
            }
            public override void SendToBack()
            {
                var container = Parent as Container;
                container.PrivateChildren.SendToBack(this);
            }
            public override void BringToFront()
            {
                var container = (Container)Parent;
                container.PrivateChildren.BringToFront(this);
            }
            protected override void Render(Gwen.Skin.SkinBase skin)
            {
                var parent = Parent as Container;
                if (parent != null)
                    parent.RenderPanel(skin);
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
            public override string ToString()
            {
                if (Parent != null)
                {
                    return "(Container) " + Parent.ToString();
                }
                return "(Container)";
            }
        }
    }
}
