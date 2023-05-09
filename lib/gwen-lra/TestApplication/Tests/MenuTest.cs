using System;
using System.Diagnostics;
using Gwen.Controls;
using Gwen;
namespace TestApplication
{
    public class MenuTest : ControlTest
    {
        public MenuTest(ControlBase parent) : base(parent)
        {
            MenuStrip menu = new MenuStrip(parent);
            var item = menu.AddItem("Test");

            item.Menu.AddItem("I'm a menu item");
            var chk = item.Menu.AddItem("Check");
            chk.IsCheckable = true;
            chk.IsChecked = true;
            var ex = item.Menu.AddItem("Expand me");
            ex.Menu.AddItem("I was expanded");
            item.Menu.AddDivider();
            item.Menu.AddItem("Divider^");
            item = menu.AddItem("Too many test");
            for (int i = 0; i < 30; i++)
                item.Menu.AddItem("item " + i);

            FlowLayout flow = new FlowLayout(parent);
            flow.Dock = Dock.Fill;
            flow.Margin = Margin.Three;
            ComboBox cb = new ComboBox(flow)
            {
                Margin = Margin.Three
            };
            cb.AddItem("Test");
            cb.AddItem("Come on combobox");
            cb.AddItem("maybe");
            cb = new ComboBox(flow);
            cb.AddItem("Disabled");
            cb.AddItem("combobox");
            cb.AddItem("maybe");
            cb.Disable();
        }
    }
}
