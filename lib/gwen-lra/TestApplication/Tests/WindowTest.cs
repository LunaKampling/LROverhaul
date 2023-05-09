using System;
using System.Diagnostics;
using Gwen.Controls;
using Gwen;
namespace TestApplication
{
    public class WindowTest : ControlTest
    {
        public WindowTest(ControlBase parent) : base(parent)
        {
            Button btn = new Button(parent);
            btn.Text = "SizeToContents window";
            btn.Clicked += (sender, arguments) =>
            {
                CreateWindow();
            };
            btn = new Button(parent);
            btn.X += 100;
            btn.Text = "Open Window (Tree)";
            btn.Clicked += (sender, arguments) =>
            {
                WindowControl win = new WindowControl(Parent, "Hello World");
                win.AutoSizeToContents = false;
                win.AddChild(new TreeControl(null)
                {
                    Children =
                    {
                        new TreeNode(null) { Text = "hi im a tree node"},
                        new TreeNode(null)
                        {
                            Text = "hi im a tree node",
                            Children =
                            {
                                new TreeNode(null) { Text = "im a child node"},
                                new TreeNode(null) { Text = "im a child node"},
                                new TreeNode(null) { Text = "im a child node"},
                                new TreeNode(null) { Text = "im a child node"},
                            }
                        },
                        new TreeNode(null) { Text = "hi im a tree node"},
                        new TreeNode(null) { Text = "hi im a tree node"},
                        new TreeNode(null) { Text = "hi im a tree node"},
                        new TreeNode(null) { Text = "hi im a tree node"},
                        new TreeNode(null) { Text = "hi im a tree node"},
                        new TreeNode(null) { Text = "hi im a tree node"},
                    },
                    AutoSizeToContents = true,
                    Dock = Dock.Fill
                });
                win.SizeToChildren();
                win.Show();
            };
            btn = new Button(parent);
            btn.Y += 50;
            btn.Text = "Open Modal";
            btn.Clicked += (sender, arguments) =>
            {
                CreateModal(false);
            };
            btn = new Button(parent);
            btn.Y += 100;
            btn.Text = "Open Modal with dim";
            btn.Clicked += (sender, arguments) =>
            {
                CreateModal(true);
            };
            btn = new Button(parent);
            btn.Y += 150;
            btn.Text = "Open Messagebox";
            btn.Clicked += (sender, arguments) =>
            {
                MessageBox.Show(Parent.GetCanvas(), "This is a test for a messagebox, " + filler, "Caption", MessageBox.ButtonType.OkCancel);
                filler += ",wrap filler ";
                // mb.ShowCentered();
            };
            btn = new Button(parent);
            btn.Y += 180;
            btn.Text = "Open Messagebox y/n/c";
            btn.Clicked += (sender, arguments) =>
            {
                MessageBox.Show(Parent.GetCanvas(), "This is a test for a y/n messagebox, " + filler, "Caption", MessageBox.ButtonType.YesNoCancel);
                filler += ",wrap filler ";
                // mb.ShowCentered();
            };
            btn = new Button(parent);
            btn.Y += 210;
            btn.Text = "Open Custom Mbox";
            btn.Clicked += (sender, arguments) =>
            {
                var mb = MessageBox.Show(Parent.GetCanvas(), "Do you agree?", "Caption", MessageBox.ButtonType.YesNoCancel);
                mb.RenameButtonsYN("Yeah", "I DO NOT AGREE","FUCK");
                // mb.ShowCentered();
            };
        }
        string filler = "with wrap filler";
        private void CreateWindow()
        {
            WindowControl win = new WindowControl(Parent, "Hello World");
            win.AutoSizeToContents = true;
            win.SetPosition(100, 100);
            Button close = new Button(win);
            close.Text = "Close or whatever ";
            close.Dock = Dock.Top;
            Button idk = new Button(win);
            idk.Text = "(Click me)";
            idk.SizeToChildren();
            idk.AutoSizeToContents = false;
            idk.Dock = Dock.Left;
            idk.Clicked += (sender, arguments) => idk.Height += 20;
            close.Clicked += (sender, arguments) => win.Close();
            win.Show();
        }
        private void CreateModal(bool dim)
        {
            WindowControl win = new WindowControl(Parent, "Modal");
            win.MakeModal(dim);
            win.SetSize(100, 100);
            Button close = new Button(win);
            close.Text = "Close";
            close.Clicked += (sender, arguments) => win.Close();
            Button add = new Button(win);
            add.Text = "Add New";
            add.Clicked += (sender, arguments) => CreateWindow();
            win.ShowCentered();
        }
    }
}
