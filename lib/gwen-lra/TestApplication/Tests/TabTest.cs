using System;
using System.Diagnostics;
using Gwen.Controls;
using Gwen;
namespace TestApplication
{
    public class TabTest : ControlTest
    {
        public TabTest(ControlBase parent) : base(parent)
        {
            TabControl tab = new TabControl(parent);
            // tab.DrawDebugOutlines = true;
            tab.Width = 300;
            tab.Height = 200;
            var page = tab.AddPage("Layout Test");
            Label l = new Label(page);
            l.Text = "0,0 100x100";
            // l.SetSize(100, 100);
            l.AutoSizeToContents = true;
            var btn = new Button(page);
            btn.Y += 100;
            btn.Text = "Reorder";
            btn.Clicked += (o, e) =>
            {
                var ch = tab.GetPage(0);
                ch.SetIndex(3);
            };

            page = tab.AddPage("Page 2");
            btn = new Button(page);
            btn.Clicked += (o, e) =>
            {
                var ch = tab.GetPage("Page 3");
                if (ch != null)
                {
                    ch.RemoveTab();
                }
                else
                {
                    var p = tab.AddPage("Page 3");
                    var b = new Button(p);
                    b.Text = "Delete self (X)";
                    b.Clicked += (ox, ex) =>
                    {
                        tab.GetPage("Page 3").RemoveTab();
                    };
                    p.SetIndex(2);
                }
            };
            btn.Text = "Toggle Delete page 3";
            btn.AutoSizeToContents = true;
            btn = new Button(page);
            btn.Clicked += (o, e) =>
            {
                tab.GetPage(0).FocusTab();
            };
            btn.Text = "Focus First Page";
            btn.AutoSizeToContents = true;
            btn.Y += 50;
            page = tab.AddPage("Page 3");
            btn = new Button(page);
            btn.Text = "Delete self";
            btn.AutoSizeToContents = true;
            btn.Clicked += (o, e) =>
            {
                tab.GetPage("Page 3").RemoveTab();
            };
            page = tab.AddPage("disabled");
            page.TabButton.IsDisabled = true;
            page = tab.AddPage("stretch");
            page = tab.AddPage("stretch");
        }
    }
}
