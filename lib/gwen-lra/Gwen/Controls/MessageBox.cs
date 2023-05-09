using System;
using System.Drawing;

namespace Gwen.Controls
{
    public class MessageBox : Gwen.Controls.WindowControl
    {
        public enum ButtonType
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel,
        }
        public EventHandler<DialogResult> Dismissed;
        public ControlBase Container;
        public DialogResult Result { get; set; }
        public string Text { get; private set; }
        private ButtonType _buttons;
        public static MessageBox Show(Canvas canvas, string text, string title, bool modal = true, bool dim = false)
        {
            var ret = new MessageBox(canvas, text, title, ButtonType.Ok);
            if (modal)
                ret.MakeModal(dim);
            ret.ShowCentered();
            return ret;
        }
        public static MessageBox Show(Canvas canvas, string text, string title, ButtonType buttons, bool modal = true, bool dim = false)
        {
            var ret = new MessageBox(canvas, text, title, buttons);
            if (modal)
                ret.MakeModal(dim);
            ret.ShowCentered();
            return ret;
        }
        public MessageBox(Gwen.Controls.ControlBase ctrl, string text, string title, ButtonType buttons) : base(ctrl, title)
        {
            Text = text;
            _buttons = buttons;
            DeleteOnClose = true;
            Container = new ControlBase(m_Panel);
            Container.Margin = new Margin(0, Skin.DefaultFont.LineHeight, 0, 0);
            Container.Dock = Dock.Bottom;
            Container.AutoSizeToContents = true;
            switch (buttons)
            {
                case ButtonType.Ok:
                    AddButton("Okay", DialogResult.OK);
                    break;
                case ButtonType.OkCancel:
                    AddButton("Cancel", DialogResult.Cancel);
                    AddButton("Okay", DialogResult.OK);
                    break;
                case ButtonType.YesNo:
                    AddButton("No", DialogResult.No);
                    AddButton("Yes", DialogResult.Yes);
                    break;
                case ButtonType.YesNoCancel:
                    AddButton("Cancel", DialogResult.Cancel);
                    AddButton("No", DialogResult.No);
                    AddButton("Yes", DialogResult.Yes);
                    break;
            }
            Setup();
            Align.Center(this);
            DisableResizing();
            Invalidate();
        }
        private void Setup()
        {
            foreach (var child in Children.ToArray())
            {
                if (child is Label && !(child is Button))
                {
                    RemoveChild(child, true);
                }
            }
            Container.SizeToChildren(true, true);
            int min = Container.Width + PanelMargin.Width + (50);
            Height = Container.Height + (Skin.DefaultFont.LineHeight * 2);
            MinimumSize = new Size(Math.Max(min, MinimumSize.Width), MinimumSize.Height);
            SetupText(Text);
            m_Panel.SizeToChildren(true, true);
            SizeToChildren(true, true);
            Invalidate();
        }
        private void SetupText(string text)
        {
            var charsize = Skin.Renderer.MeasureText(Skin.DefaultFont, "_").X;
            int maxwidth = Math.Max(charsize * 30, MinimumSize.Width - PanelMargin.Width);
            int maxwidth2 = maxwidth + (maxwidth / 2);
            var wrapped1 = Skin.DefaultFont.WordWrap(text, maxwidth);
            var wrapped2 = Skin.DefaultFont.WordWrap(text, maxwidth2);
            var wrap = wrapped2;
            // this is a cheat that doesnt work perfectly, but decently for making
            // short messageboxes appear ok
            if (wrapped1.Count == wrapped2.Count)
            {
                wrap = wrapped1;
            }
            foreach (var line in wrap)
            {
                AddLine(line);
            }
        }
        public void RenameButtons(string ok, string cancel = "")
        {
            if (_buttons != ButtonType.Ok && _buttons != ButtonType.OkCancel)
                throw new InvalidOperationException(
                    "Cannot rename OK/Cancel buttons on Button Type " + _buttons);
            ((Button)Container.FindChildByName("Okay", true)).Text = ok;
            if (_buttons == ButtonType.OkCancel && cancel != "")
            {
                ((Button)Container.FindChildByName("Cancel", true)).Text = cancel;
            }
            Setup();
        }
        public void RenameButtonsYN(string yes, string no, string cancel = "")
        {
            if (_buttons != ButtonType.YesNo && _buttons != ButtonType.YesNoCancel)
                throw new InvalidOperationException(
                    "Cannot rename Y/N buttons on Button Type " + _buttons);
            ((Button)Container.FindChildByName("Yes", true)).Text = yes;
            ((Button)Container.FindChildByName("No", true)).Text = no;
            if (_buttons == ButtonType.YesNoCancel && cancel != "")
            {
                ((Button)Container.FindChildByName("Cancel", true)).Text = cancel;
            }
            Setup();
        }
        private void AddButton(string text, DialogResult result)
        {
            Button btn = new Button(Container);
            btn.Margin = new Margin(7, 1, 1, 1);
            btn.Dock = Dock.Right;
            btn.Name = text;
            btn.Text = text;
            btn.Clicked += (o, e) =>
            {
                Result = result;
                Close();
                DismissedHandler(o, e);
            };
            btn.Padding = new Padding(5, 0, 5, 0);
        }

        private void DismissedHandler(ControlBase control, EventArgs args)
        {
            if (Dismissed != null)
                Dismissed.Invoke(this, Result);
        }
        private void AddLine(string line)
        {
            Label add = new Label(m_Panel);
            add.Margin = new Margin(0, 0, 0, 0);
            add.Alignment = Pos.Left | Pos.Top;
            add.Dock = Dock.Top;
            add.AutoSizeToContents = true;
            add.Text = line;
        }
    }
}
