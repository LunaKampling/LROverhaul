using System;
using System.Diagnostics;
using Gwen.Controls;
using Gwen;
namespace TestApplication
{
    public class ProgressBarTest : ControlTest
    {

        public ProgressBarTest(ControlBase parent) : base(parent)
        {
            ProgressBar p = new ProgressBar(parent) { Y = 20};
            ProgressBar p2 = new ProgressBar(parent);
            p2.Y += 100;
            p2.Height = 32;
            p2.Value = 0.025f;
            ProgressBar p3 = new ProgressBar(parent);
            p3.IsHorizontal = false;
            p3.Y += 100;
            p3.X += 100;
            p3.Width = 15;
            p3.Height = 100;
            HorizontalSlider hs = new HorizontalSlider(parent) { X = 100, Y = 300, Min = 0, Max = 1, Height = 20, Dock = Dock.Top };
            hs.ValueChanged += (o,e)=>
            {
                p.Value = (float)hs.Value;
                // p2.Value = p.Value;
                p3.Value = p.Value;
            };
        }
    }
}
