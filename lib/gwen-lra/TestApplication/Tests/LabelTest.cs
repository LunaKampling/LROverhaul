using System;
using Gwen.Controls;
using Gwen;

namespace TestApplication.Tests
{
    public class LabelTest : ControlTest
    {
        public LabelTest(ControlBase parent) : base(parent)
        {
            Label dynamiclabel = new Label(parent);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Random r = new Random();
            dynamiclabel.TextRequest = (o, s) =>
            {
                if (sw.ElapsedMilliseconds > 1000)
                {
                    sw.Restart();
                    return "Random num 1/sec:" +r.Next().ToString();
                }
                else
                {
                    return s;
                }
            };
            dynamiclabel.DrawDebugOutlines = true;
            dynamiclabel.X = 200;
            CreateAlign(Pos.Top | Pos.Left);
            CreateAlign(Pos.Top | Pos.CenterH);
            CreateAlign(Pos.Top | Pos.Right);
            row++;
            counter = 0;
            CreateAlign(Pos.Left | Pos.CenterV);
            CreateAlign(Pos.Center);
            CreateAlign(Pos.Right | Pos.CenterV);
            row++;
            counter = 0;
            CreateAlign(Pos.Bottom | Pos.Left);
            CreateAlign(Pos.Bottom | Pos.CenterH);
            CreateAlign(Pos.Bottom | Pos.Right);
            row++;
            var label = Create("Autosized label");
            row++;
            label = Create("Autosize, padding");
            label.Padding = new Padding(10, 10, 5, 10);
            row++;
            label = Create("padding, centerv");
            label.AutoSizeToContents = false;
            label.Height = 50;
            label.Width = 150;
            label.Padding = new Padding(10, 10, 10, 10);
            label.Alignment = Pos.CenterV;
            row++;
            label = Create("padding, centerh");
            label.AutoSizeToContents = false;
            label.Height = 50;
            label.Width = 150;
            label.Padding = new Padding(10, 10, 10, 10);
            label.Alignment = Pos.CenterH;
            row++; label = Create("padding, top left");
            label.AutoSizeToContents = false;
            label.Height = 50;
            label.Width = 150;
            label.Padding = new Padding(10, 10, 10, 10);
            label.Alignment = Pos.Top | Pos.Left;
            row++;
            label = Create("padding, bottom right");
            label.AutoSizeToContents = false;
            label.Height = 50;
            label.Width = 150;
            label.Padding = new Padding(10, 10, 10, 10);
            label.Alignment = Pos.Bottom | Pos.Right;
            row++;
            label = Create("Too small");
            label.AutoSizeToContents = false;
            label.Height = 30;
            label.Width = 60;
            label.Padding = new Padding(10, 10, 10, 10);
            label.Alignment = Pos.Bottom | Pos.Right;
            row++;
        }
        private Label Create(string text)
        {
            Label label = new Label(Parent);
            label.Y += row * 50;
            label.Text = text;
            label.DrawDebugOutlines = true;
            return label;
        }
        private int row = 0;
        private int counter = 0;
        private void CreateAlign(Pos align)
        {
            Label label = new Label(Parent);
            label.SetSize(50, 50);
            string text = "";
            if (align.HasFlag(Pos.Top))
                text += "T";
            if (align.HasFlag(Pos.Bottom))
                text += "B";
            if (align.HasFlag(Pos.Right))
                text += "R";
            if (align.HasFlag(Pos.Left))
                text += "L";
            if (align == Pos.Center)
            {
                text = "C";
            }
            label.Text = text;
            label.Alignment = align;
            label.X += 50 * counter++;
            label.Y += 50 * row;
            label.AutoSizeToContents = false;
            label.DrawDebugOutlines = true;
        }
    }
}
