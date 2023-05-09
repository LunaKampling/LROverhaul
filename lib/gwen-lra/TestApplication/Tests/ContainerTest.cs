using System;
using System.Diagnostics;
using Gwen.Controls;
using Gwen;
namespace TestApplication
{
    public class ContainerTest : ControlTest
    {
        public ContainerTest(ControlBase parent) : base(parent)
        {
            GroupBox gb = new GroupBox(parent);
            gb.Text = "group box";
            gb.Width = 120;
            gb.Height = 120;
            gb.AddChild(new Button(null) { Text = "Button" });

            Panel panel = new Panel(parent)
            {
                Children =
                {
                    new FlowLayout(null)
                    {
                        Children =
                        {
                            new Label(null) { Text = "A label in a panel"},
                            new Button(null)
                            {
                                Text = "A button in a panel",

                            }
                        },
                        Dock = Dock.Fill
                    }
                },

            };
            panel.SetBounds(130, 130, 150, 150);
            ScrollControl sc = new ScrollControl(parent)
            {
                Children =
                {
                    new Label(null)
                    {
                        Text = "Scroll control"
                    },
                    new Button(null)
                    {
                        X = 180,
                        Y = 180,
                        Text = "Scroll to me"
                    },
                    new Button(null)
                    {
                        X = 180,
                        Y = 380,
                        Text = "No, scroll to me."
                    },
                }
            };
            sc.SetBounds(0, 300, 200, 200);
            VerticalScrollBar vs = new VerticalScrollBar(sc);
            vs.NudgeAmount = 30;
            vs.ViewableContentSize = 10;
            vs.ContentSize = 120;
            vs.Disable();
            vs.Height = 100;
            vs.Y += 20;

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
