using System;
using System.Diagnostics;
using Gwen.Controls;
using Gwen;
namespace TestApplication
{
    public class ListBoxTest : ControlTest
    {
        public ListBoxTest(ControlBase parent) : base(parent)
        {
            FlowLayout l = new FlowLayout(parent);
            l.Dock = Dock.Fill;
            ListBox lb = new ListBox(l)
            {
                AutoSizeToContents = true,
                Margin = Margin.Two,
                AlternateColors=true,
            };
            lb.AddRow("Listbox");
            lb.AddRow("This one is autosized");
            lb.AddRow("Yep.");
            lb.AddRow("Also, alternate colors");
            ListBox manual = new ListBox(l);
            manual.Margin = Margin.Two;
            manual.Width = 200;
            manual.Height = 200;
            manual.AddRow("Listbox.");
            manual.AddRow("This is manually sized and has an item too big for it.");
            manual.AddRow("so scrollbars are nice.");
            manual.AddRow("Also gonna pad down.");
            manual.AddRow("Pad.");
            manual.AddRow("Pad.");
            manual.AddRow("Pad.");
            manual.AddRow("Pad.");
            manual.AddRow("Pad.");
            manual.AddRow("Pad.");
        }
    }
}
