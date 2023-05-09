using System;
using System.Diagnostics;
using Gwen;
using Gwen.Controls;
namespace TestApplication
{
    public class ButtonTest : ControlTest
    {
        private TableLayout _layout;
        public ButtonTest(ControlBase parent) : base(parent)
        {
            Create();
        }
        private void Create()
        {

            _layout = new TableLayout(Parent)
            {
                AutoSizeRows = true,
                Dock = Dock.Fill,
                ColumnCount = 2,
                //DrawDebugOutlines = true
            };
            var btn = CreateButton("auto sized button (hover me)");
            btn.AutoSizeToContents = true;
            btn.Tooltip = "With tooltip";

            btn = CreateButton("Auto sized padding button");
            btn.Padding = new Gwen.Padding(10, 10, 10, 10);
            btn.AutoSizeToContents = true;

            btn = CreateButton("Event Test Button");
            btn.Padding = new Gwen.Padding(10, 10, 10, 10);
            btn.AutoSizeToContents = true;
            btn.Clicked += (o, e) =>
            {
                Console.WriteLine("Clicked");
            };
            btn.DoubleClicked += (o, e) =>
            {
                Console.WriteLine("Double Clicked");
            };
            btn.Pressed += (o, e) =>
            {
                Console.WriteLine("Pressed");
            };
            btn.Released += (o, e) =>
            {
                Console.WriteLine("Released");
            };
            btn = CreateButton("manual sized button");
            btn.Width += 100;
            btn.AutoSizeToContents = false;

            btn.AutoSizeToContents = false;
            btn = CreateButton("disabled");
            btn.IsDisabled = true;

            btn = CreateButton("toggle");
            btn.IsToggle = true;
            btn.Toggle();

            btn = CreateButton("left aligned button");
            btn.Width = 150;
            btn.AutoSizeToContents = false;
            btn.Alignment = Gwen.Pos.Left | Gwen.Pos.CenterV;

            btn = CreateButton("right align button.");
            btn.Width = 150;
            btn.Alignment = Gwen.Pos.Right | Gwen.Pos.CenterV;
            btn.AutoSizeToContents = false;

            var row = _layout.CreateRow();
            btn = new DropDownButton(row.GetCell(0))
            {
                Text = "Drop me"
            };
            CreateCheckables();
        }
        private void CreateCheckables()
        {
            var row = _layout.CreateRow();
            var box = new Checkbox(row.GetCell(0));
            row = _layout.CreateRow();
            box = new Checkbox(row.GetCell(0))
            {
                Text = "Checked",
                IsChecked = true
            };
            row = _layout.CreateRow();
            box = new Checkbox(row.GetCell(0))
            {
                Text = "Checkbox",
                IsChecked = false
            };
            row = _layout.CreateRow();
            box = new Checkbox(row.GetCell(0))
            {
                Text = "Disabled",
                IsChecked = false,
                IsDisabled = true,
            };
            box = new Checkbox(row.GetCell(1))
            {
                Text = "Disabled",
                IsChecked = true,
                IsDisabled = true,
            };
            row = _layout.CreateRow();
            var radiogroup = new RadioButtonGroup(row.GetCell(0));
            radiogroup.AddOption("Radio 1").Tooltip = "tooltip 1";
            radiogroup.AddOption("Radio 2").Tooltip = "tooltip 2";
            var dc = radiogroup.AddOption("disabledChecked");
            dc.IsChecked = true;
            dc.Disable();
            radiogroup.AddOption("disabled").Disable();

            row = _layout.CreateRow();
            var radio = new RadioButton(row.GetCell(0))
            {
                Text = "ungrouped 1",
                Dock = Dock.Top
            }; radio = new RadioButton(row.GetCell(0))
            {
                Text = "ungrouped 2",
                Dock = Dock.Top
            };
        }
        private Button CreateButton(string text)
        {
            var row = _layout.CreateRow();
            Button btn = new Button(row.GetCell(0));
            btn.Dock = Dock.Left;
            btn.Text = text;
            return btn;
        }
    }
}
