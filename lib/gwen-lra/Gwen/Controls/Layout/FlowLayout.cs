using System;
using System.Collections.Generic;

namespace Gwen.Controls
{
    /// <summary>
    /// Flow layout. Automatically space children left to right, creating rows
    /// when necessary
    /// </summary>
    public class FlowLayout : ControlBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowLayout"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public FlowLayout(ControlBase parent) : base(parent)
        {
        }
        public override Size GetSizeToFitContents()
        {
            Size size = Size.Empty;
            if (Dock == Dock.Top || Dock == Dock.Bottom)
            {
                size.Width = Parent.InnerSize.Width - Parent.Padding.Width - Margin.Width;
            }
            if (Dock == Dock.Left || Dock == Dock.Right)
            {
                size.Height = Parent.InnerSize.Height - Parent.Padding.Height - Margin.Height;
            }
            int x = 0;
            int y = 0;
            int rowheight = 0;
            int maxwidth = 0;
            // Adjust bounds for padding
            foreach (ControlBase child in Children)
            {
                if (child.IsHidden)
                    continue;
                Size childsize = child.AutoSizeToContents ? child.GetSizeToFitContents() : child.Bounds.Size;
                childsize.Width += child.Margin.Width;
                childsize.Height += child.Margin.Height;

                if (x + childsize.Width > size.Width)
                {
                    y += rowheight;
                    x = 0;
                    rowheight = 0;
                }
                x += childsize.Width;
                maxwidth = Math.Max(x, maxwidth);
                rowheight = Math.Max(rowheight, childsize.Height);
            }
            size.Width = x;
            size.Height = y + rowheight;

            size.Width += Padding.Width;
            size.Height += Padding.Height;
            return size;
        }
        private void CorrectRow(int rowheight, int y, List<ControlBase> row)
        {
            foreach (ControlBase rowchild in row)
            {
                int h = rowchild.Height + rowchild.Margin.Height;
                if (h < rowheight)
                {
                    int d = rowheight - h;
                    rowchild.Y = y + d / 2; // Center child in row.
                }
            }
        }
        protected override void ProcessLayout(Size size)
        {
            size.Width -= Padding.Width;
            size.Height -= Padding.Height;
            int x = 0;
            int y = 0;
            int rowheight = 0;
            List<ControlBase> row = new List<ControlBase>();
            foreach (ControlBase child in Children)
            {
                if (child.IsHidden)
                    continue;
                Size childsize;
                if (child.AutoSizeToContents)
                {
                    childsize = child.GetSizeToFitContents();
                    child.Size = childsize;
                }
                else
                    childsize = child.Bounds.Size;
                childsize.Width += child.Margin.Width;
                childsize.Height += child.Margin.Height;
                if (x + childsize.Width > size.Width)
                {
                    CorrectRow(rowheight, y, row);
                    y += rowheight;
                    x = 0;
                    rowheight = 0;
                    row.Clear();
                }
                child.SetPosition(x, y);
                x += childsize.Width;
                rowheight = Math.Max(rowheight, childsize.Height);
                row.Add(child);
            }

            CorrectRow(rowheight, y, row);
        }
    }
}
