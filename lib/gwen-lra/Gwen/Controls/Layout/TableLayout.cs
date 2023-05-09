using System;
using System.Drawing;
using System.Collections.Generic;

namespace Gwen.Controls
{
    /// <summary>
    /// A grid that contains controls within its cells
    /// the largest cell of a row controls the size of that row
    /// </summary>
    public class TableLayout : ControlBase
    {
        protected List<int> m_rowwidths = new List<int>();
        private bool m_autosizerows = true;
        private int m_cols = 0;
        public bool AutoSizeRows
        {
            get
            {
                return m_autosizerows;
            }
            set
            {
                if (value != m_autosizerows)
                {
                    m_autosizerows = value;
                    Invalidate();
                }
            }
        }
        public int ColumnCount
        {
            get
            {
                return m_cols;
            }
            set
            {
                if (value != m_cols)
                {
                    m_cols = value;
                    Invalidate();
                }
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TableLayout"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TableLayout(ControlBase parent) : base(parent)
        {
            ColumnCount = 2;
        }
        public void SetColumnWidth(int column, int width)
        {
            if (column >= m_cols)
                throw new ArgumentOutOfRangeException(
                    "Column cannot be resized because it does not exist");
            m_rowwidths[column] = width;
            Invalidate();
        }
        public TableRow CreateRow()
        {
            return new TableRow(this);
        }
        private void CalculateRows()
        {
            if (AutoSizeRows)
            {
                m_rowwidths.Clear();
                foreach (var child in Children)
                {
                    if (child.IsHidden)
                        continue;
                    if (child is TableRow row)
                    {
                        var widths = row.GetRowWidths();
                        for (int i = 0; i < widths.Count; i++)
                        {
                            if (i >= m_rowwidths.Count)
                                m_rowwidths.Add(widths[i]);
                            m_rowwidths[i] = Math.Max(m_rowwidths[i], widths[i]);
                        }
                    }
                }
            }
            else
            {
                while (m_cols >= m_rowwidths.Count)
                {
                    m_rowwidths.Add(50);
                }
            }
        }
        private void LayoutRow(TableRow row)
        {
            int xoffset = 0;
            for (int i = 0; i < row.cells.Length && i < m_cols; i++)
            {
                var cell = row.cells[i];
                int cellwidth = m_rowwidths[i];
                int celly = 0;
                int cellx = 0;
                if (cell != null)
                {
                    cell.MaximumSize = new Size(
                        cellwidth - cell.Margin.Width,
                        row.Height - cell.Margin.Height);
                    cell.SizeToChildren(true, true);

                    if (cell.Height < row.Height)
                    {
                        var d = row.Height - cell.Height;
                        celly = (d / 2);//center child in row.
                    }
                    if (cell.Width < cellwidth)
                    {
                        var d = cellwidth - cell.Width;
                        cellx = (d / 2);//center child in row.
                    }
                    cell.SetPosition(xoffset + cellx, celly);
                }
                xoffset += cellwidth;
            }
            row.Width = xoffset;
        }
        protected override void ProcessLayout(Size size)
        {
            size.Width -= Padding.Width;
            size.Height -= Padding.Height;
            CalculateRows();
            int y = 0;
            foreach (var child in Children)
            {
                if (child.IsHidden)
                    continue;
                if (child is TableRow row)
                {
                    row.Height = row.GetRowHeight();
                    LayoutRow(row);
                    row.Y = y;
                    y += row.Height;
                }
            }
        }
    }
}
