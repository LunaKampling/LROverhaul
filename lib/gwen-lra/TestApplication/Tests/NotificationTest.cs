using System;
using System.Diagnostics;
using Gwen.Controls;
using Gwen;
namespace TestApplication
{
    public class NotificationTest : ControlTest
    {
        public NotificationTest(ControlBase parent) : base(parent)
        {
            Panel notificationpane = new Panel(parent);
            notificationpane.Width = 200;
            notificationpane.Dock = Dock.Right;
            Button b = new Button(parent);
            b.Dock = Dock.Top;
            b.Text = "Show notify";
            RichLabel label = new RichLabel(parent)
            {
                Dock = Dock.Top
            };
            label.AddText("?",System.Drawing.Color.Black);
            b.Clicked += (o, e) =>
            {
                // Notification n = new Notification(notificationpane);
                // n.Text = ("I'm supposed to be multilined, and automatically word wrap.\nThis is on a new line");
                // n.Show(250);
                label.AddText("\nFuck you.", System.Drawing.Color.Red);
                // label.Text = "*error\nFuck you";
                // label.Layout();
            };
        }
    }
}
