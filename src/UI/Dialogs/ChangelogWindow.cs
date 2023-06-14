using Gwen;
using Gwen.Controls;
using linerider.Utils;

namespace linerider.UI
{
    public class ChangelogWindow : DialogBase
    {
        public ChangelogWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = "Changelog for " + Program.FullVersion;
            AutoSizeToContents = true;

            string changelogText =
                "5/15/2023 \n" +
                "*Change line type based on selection! Alt + 1/2/3 to change the line type \n" +
                "*Copy line data to your clipboard! Ctrl+shift+c/v to copy/paste line data \n" +
                "*Generators now contain a button within the top bar UI \n" +
                "*.json files no longer make the background for exports 000000FF (Black)  \n" +
                "*A few links have been changed to now point to the LROverhaul GitHub page \n" +
                "*General hotkey changes \n" +
                "*Toolbar buttons should be upscaled by now \n \n" +
                "*Note: Bezier and Remount still need fixing \n" +
                "*New selection mechanics have been tested, though if you happen to somehow crash \n" +
                "the program, report it on the Trello page. Of course also send your log file.";

            ControlBase bottomcontainer = new ControlBase(this)
            {
                Margin = new Margin(0, 0, 0, 0),
                Dock = Dock.Bottom,
                AutoSizeToContents = true
            };

            Button btncontinue = new Button(null)
            {
                Text = "Continue",
                Name = "btncontinue",
                Dock = Dock.Right,
                Margin = new Margin(10, 0, 0, 0),
                AutoSizeToContents = true,
            };
            btncontinue.Clicked += (o, e) =>
            {
                _ = Close();
            };

            Button btndontshow = new Button(null)
            {
                Text = "Continue and don\'t show again",
                Name = "btndontshow",
                Dock = Dock.Right,
                Margin = new Margin(10, 0, 0, 0),
                AutoSizeToContents = true,
            };
            btndontshow.Clicked += (o, e) =>
            {
                Settings.showChangelog = false;
                Settings.Save();
                _ = Close();
            };

            Button btngithub = new Button(null)
            {
                Text = "Previous Changelogs (Github)",
                Name = "btngithub",
                Dock = Dock.Right,
                Margin = new Margin(10, 0, 0, 0),
                AutoSizeToContents = true,
            };
            btngithub.Clicked += (o, e) =>
            {
                try
                {
                    GameCanvas.OpenUrl($"{Constants.GithubPageHeader}/tree/master/Changelogs");
                }
                catch
                {
                    _ = MessageBox.Show(parent, "Unable to open your browser.", "Error!");
                }
                _ = Close();
            };

            ControlBase buttoncontainer = new ControlBase(bottomcontainer)
            {
                Margin = new Margin(0, 0, 0, 0),
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                Children =
                {
                    btncontinue,
                    btndontshow,
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

        private void CreateLabeledControl(ControlBase parent, string label, ControlBase control)
        {
            control.Dock = Dock.Right;
            _ = new ControlBase(parent)
            {
                Children =
                {
                    new Label(null)
                    {
                        Text = label,
                        Dock = Dock.Left,
                        Alignment = Pos.Left | Pos.CenterV,
                        Margin = new Margin(0,0,10,0)
                    },
                    control
                },
                AutoSizeToContents = true,
                Dock = Dock.Top,
                Margin = new Margin(0, 1, 0, 1)
            };
        }
    }
}
