using System;
using System.Diagnostics;
using Gwen.Controls;
using Gwen;
namespace TestApplication
{
    public class PropertyTest : ControlTest
    {
        public PropertyTest(ControlBase parent) : base(parent)
        {
            PropertyTable table = new PropertyTable(parent);
            table.AutoSizeToContents = true;
            table.Width = 100;
            table.Height = 100;
            InitTable(table);
            PropertyTree tree = new PropertyTree(parent);
            tree.DrawDebugOutlines = false;
            tree.Height = 100;
            tree.Width = 200;
            tree.Y = 205;
            var t = tree.Add("Tree Test");
            InitTable(t);
            t = tree.Add("Tree Test 2");
            InitTable(t);
            InitTable(t);
        }
        private void InitTable(PropertyTable table)
        {
            table.Add("text", "textbox").Tooltip = "Heyy tooltip";
            LabelProperty lab = new LabelProperty(null)
            {
                Value = "Uneditable label",
            };
            table.Add("Label", lab);
            table.Add("check", new CheckProperty(null));
            var cb = new ComboBoxProperty(table);
            cb.AddItem("Test");
            cb.AddItem("auto selected");
            cb.AddItem("val 3");
            cb.SetValue("auto selected");
            table.Add("combo", cb);
            NumberProperty num = new NumberProperty(table) { Min = -10, Max = 10, Value = "3" };
            table.Add("Number:", num);
        }
    }
}
