using System;
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
            get => m_autosizerows;
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
            get => m_cols;
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
        public TableRow CreateRow() => new TableRow(this);
        private void CalculateRows()
        {
            if (AutoSizeRows)
            {
                m_rowwidths.Clear();
                foreach (ControlBase child in Children)
                {
                    if (child.IsHidden)
                        continue;
                    if (child is TableRow row)
                    {
                        List<int> widths = row.GetRowWidths();
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
                TableRow.TableCell cell = row.cells[i];
                int cellwidth = m_rowwidths[i];
                int celly = 0;
                int cellx = 0;
                if (cell != null)
                {
                    cell.MaximumSize = new Size(
                        cellwidth - cell.Margin.Width,
                        row.Height - cell.Margin.Height);
                    _ = cell.SizeToChildren(true, true);

                    if (cell.Height < row.Height)
                    {
                        int d = row.Height - cell.Height;
                        celly = d / 2; // Center child in row.
                    }
                    if (cell.Width < cellwidth)
                    {
                        int d = cellwidth - cell.Width;
                        cellx = d / 2; // Center child in row.
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
            foreach (ControlBase child in Children)
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
