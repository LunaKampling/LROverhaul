using System;
using System.Collections.Generic;
using System.Drawing;

namespace Gwen.Controls
{
    public class TableRow : ControlBase
    {
        public class TableCell : ControlBase
        {
            public override bool AutoSizeToContents
            {
                get => false;
                set
                {
                }
            }
            public override Dock Dock
            {
                get => Dock.None;
                set
                {
                    if (value != Dock.None)
                        throw new InvalidOperationException(
                            "Cannot set Dock of tablecell");
                }
            }
            private readonly TableRow m_parent;
            private readonly TableLayout m_parentlayout;
            public TableCell(TableRow parent) : base(parent)
            {
                m_parent = parent;
                m_parentlayout = m_parent.m_parent;
                Margin = Margin.One;
            }
            protected override void OnChildBoundsChanged(Rectangle oldChildBounds, ControlBase child)
            {
                base.OnChildBoundsChanged(oldChildBounds, child);
                m_parentlayout.Invalidate();
            }
            protected override void OnChildAdded(ControlBase child)
            {
                base.OnChildAdded(child);
                m_parentlayout.Invalidate();
            }
            protected override void OnChildRemoved(ControlBase child)
            {
                base.OnChildRemoved(child);
                m_parentlayout.Invalidate();
            }
        }
        private readonly TableLayout m_parent;
        internal TableCell[] cells = new TableCell[0];
        public TableRow(TableLayout parent) : base(parent)
        {
            m_parent = parent ?? throw new Exception("Table layout parent cannot be null");
        }
        /// <summary>
        /// Handler invoked when a child is added.
        /// </summary>
        /// <param name="child">Child added.</param>
        protected override void OnChildAdded(ControlBase child)
        {
            base.OnChildAdded(child);
            if (!(child is TableCell))
                throw new Exception(
                    "Cannot add child to tablerow that is not a cell");
        }
        public List<int> GetRowWidths()
        {
            List<int> ret = new List<int>();
            foreach (TableCell child in cells)
            {
                if (child == null)
                {
                    ret.Add(0);
                    continue;
                }
                Size childsize;
                childsize = child.GetSizeToFitContents();
                childsize += child.Margin.Size;
                ret.Add(childsize.Width);
            }
            return ret;
        }
        public int GetRowHeight()
        {
            int rowheight = 0;
            foreach (TableCell child in cells)
            {
                if (child == null)
                    continue;
                Size childsize;
                childsize = child.GetSizeToFitContents();
                childsize += child.Margin.Size;
                rowheight = Math.Max(rowheight, childsize.Height);
            }
            return rowheight + Padding.Height + Margin.Height;
        }
        protected override void ProcessLayout(Size size)
        {
            // We dont do any docking, etc, so this is empty.
        }
        public TableCell GetCell(int index)
        {
            int mincells = Math.Max(index, m_parent.ColumnCount);
            if (mincells >= cells.Length)
            {
                TableCell[] newcells = new TableCell[mincells];
                for (int i = 0; i < cells.Length; i++)
                {
                    newcells[i] = cells[i];
                }
                cells = newcells;
            }
            if (cells[index] == null)
            {
                cells[index] = new TableCell(this);
            }
            return cells[index];
        }
        public void SetCell(ControlBase control, int index)
        {
            if (control == null)
                throw new ArgumentNullException("Cannot set a null cell");
            GetCell(index).AddChild(control);
        }
    }
}
