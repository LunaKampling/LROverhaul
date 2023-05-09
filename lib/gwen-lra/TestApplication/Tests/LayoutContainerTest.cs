using System;
using System.Diagnostics;
using Gwen.Controls;
using Gwen;
namespace TestApplication
{
    public class LayoutContainerTest : ControlTest
    {
        public LayoutContainerTest(ControlBase parent) : base(parent)
        {
            GroupBox gb = new GroupBox(parent);
            gb.Text = "Flow layout";
            gb.Width = 120;
            gb.Height = 120;


            FlowLayout sp = new FlowLayout(gb);
            // sp.DrawDebugOutlines = true;
            sp.AutoSizeToContents = true;
            sp.Dock = Dock.Top;
            CreateButton(sp, "Test 1");
            CreateButton(sp, "Test 2");
            CreateButton(sp, "Test 3");
            CreateButton(sp, "Test 4");
            gb = new GroupBox(parent);
            gb.Y += 130;
            gb.Text = "Table layout";
            gb.Width = 200;
            gb.Height = 200;
            CreateTable(gb);
        }
        private void CreateTable(ControlBase container)
        {
            TableLayout sp = new TableLayout(container);
            sp.AutoSizeRows = false;
            // sp.DrawDebugOutlines = true;
            sp.AutoSizeToContents = true;
            sp.Dock = Dock.Fill;
            var row = sp.CreateRow();
            CreateButton(row.GetCell(0), "autosize").Clicked += (o, e) =>
            {
                sp.AutoSizeRows = true;
                var txt = ((Button)o).Text;
                if (txt == "short")
                    txt = "much longer";
                else
                    txt = "short";
                ((Button)o).Text = txt;
            };
            CreateButton(row.GetCell(1), "Test 2");
            row = sp.CreateRow();
            CreateButton(row.GetCell(0), "Test 3 --");
            var b = CreateButton(row.GetCell(1), "Tall 4");
            b.SizeToChildren();
            b.AutoSizeToContents = false;
            b.Height = 50;
            row = sp.CreateRow();
            CreateButton(row.GetCell(0), "no auto").Clicked += (o, e) =>
            {
                sp.AutoSizeRows = false;
                sp.SetColumnWidth(0, 20);
            };
            row = sp.CreateRow();
            row.SetCell(CreateButton(null, "Test 6"), 1);
        }
        private Button CreateButton(ControlBase parent, string text)
        {
            Button button = new Button(parent);

            // button.Margin = Margin.Two;
            button.Text = text;
            return button;
        }
    }
}
