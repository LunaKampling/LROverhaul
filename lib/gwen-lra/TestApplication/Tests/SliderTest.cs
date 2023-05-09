using System;
using System.Diagnostics;
using Gwen.Controls;
using Gwen;
namespace TestApplication
{
    public class SliderTest : ControlTest
    {
        public SliderTest(ControlBase parent) : base(parent)
        {
            TableLayout table = new TableLayout(parent);
            table.ColumnCount = 2;
            // table.Dock = Dock.Fill;
            table.Width = 500;
            table.Height = 500;
            var row = table.CreateRow();
            // Gwen.Controls.sl
            VerticalSlider slider = new VerticalSlider(row.GetCell(0));
            slider.Tooltip = "val";
            slider.SetRange(0, 100);
            //row.AutoSizeToContents = true;
            HorizontalSlider slider2 = new HorizontalSlider(row.GetCell(1));
            slider2.Width = 100;
            slider2.SetRange(0, 100);
            row = table.CreateRow();
            slider = new VerticalSlider(row.GetCell(0));
            slider.NotchCount = 10;
            slider.ValueChanged += (o, e) =>
            {
                var sl = (Slider)o;
                Console.WriteLine(sl.Value);
            };
            slider.SnapToNotches = true;
            slider.SetRange(0, 100);
            //row.AutoSizeToContents = true;
            slider2 = new HorizontalSlider(row.GetCell(1));
            slider2.SnapToNotches = true;
            slider2.Width = 100;
            slider2.SetRange(-100, 100);
            slider2.NotchCount = 15;
            slider2.SnapToNotches = true;
            slider2.Tooltip = "I have notches but theyre hidden";
            slider2.DrawNotches = false;
            row = table.CreateRow();
            slider2 = new HorizontalSlider(row.GetCell(1));
            slider2.SnapToNotches = true;
            slider2.Disable();
            slider2.Width = 100;
            slider2.SetRange(-100, 100);
            slider2.NotchCount = 15;
            slider2.SnapToNotches = true;
            slider2.Tooltip = "I have notches but theyre hidden";
            slider2.DrawNotches = false;
        }
        private void InitTable(PropertyTable table)
        {
            table.Add("text", "val").Tooltip = "Heyy tooltip";
            table.Add("check", new CheckProperty(null));
        }
    }
}
