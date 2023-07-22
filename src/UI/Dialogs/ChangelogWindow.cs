using Gwen;
using Gwen.Controls;
using linerider.Utils;
using System;
using System.Drawing;
using System.Linq;

namespace linerider.UI
{
    public class ChangelogWindow : DialogBase
    {
        public ChangelogWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            System.Collections.Generic.List<string> changelogLines = AssemblyInfo.ChangelogLines.Split('\n').ToList();

            // Use first changelog line as window title
            string title = changelogLines[0];
            changelogLines.RemoveRange(0, 2); // Remove title and following empty line
            if (title.StartsWith("#"))
                title = title.Substring(1).Trim();

            string changelogText = string.Join("\n", changelogLines);
            AutoSizeToContents = true;
            Title = title;

            MinimumSize = new Size(250, MinimumSize.Height);

            ControlBase bottomcontainer = new ControlBase(this)
            {
                Margin = new Margin(0, 10, 0, 0),
                Dock = Dock.Bottom,
                AutoSizeToContents = true
            };

            Button btngithub = new Button(null)
            {
                Text = "Previous Changelogs",
                Name = "btngithub",
                Dock = Dock.Left,
                Margin = new Margin(0, 0, 0, 0),
                AutoSizeToContents = true,
            };
            btngithub.Clicked += (o, e) =>
            {
                try
                {
                    GameCanvas.OpenUrl($"{Constants.GithubPageHeader}/tree/main/Changelogs");
                }
                catch
                {
                    _ = MessageBox.Show(parent, "Unable to open your browser.", "Error!");
                }
                _ = Close();
            };

            Button btnclose = new Button(null)
            {
                Text = "Close",
                Dock = Dock.Right,
                Margin = new Margin(10, 0, 0, 0),
                AutoSizeToContents = true,
            };
            btnclose.Clicked += CloseButtonPressed;

            ControlBase buttoncontainer = new ControlBase(bottomcontainer)
            {
                Margin = new Margin(0, 0, 0, 0),
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                Children =
                {
                    btnclose,
                    btngithub,
                }
            };

            RichLabel l = new RichLabel(this)
            {
                Dock = Dock.Top,
                AutoSizeToContents = true
            };
            l.AddText(changelogText, Skin.Colors.Text.Foreground);
            MakeModal(true);
            DisableResizing();
        }
        protected override void CloseButtonPressed(ControlBase control, EventArgs args)
        {
            // Force update settings so it contains actual LRO version
            Settings.Save();

            base.CloseButtonPressed(control, args);
        }
    }
}
