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
                "6/23/2023 \n" +
                "*Fixed a bug regarding hotkeys \n" +
                "*Made Bosh more pleasing to the eye \n" +
                "*Fixed zooming to now zoom at cursor position. \n" +
                "*Made zooming linear instead of multiplicative to prevent odd zoom values \n" +
                "*Changed .exe icons \n" +
                "*Select tool can now select only unhit lines \n" +
                "*Bezier knobs now always have the same thickness \n" +
                "*Changed several menus' lay-out and functionality \n" +
                "*Preview mode is autoenabled when the trigger window is open \n" +
                "*Colour inputs now work using HEX instead of RGB \n" +
                "*Added force reload for rider and scarf \n" +
                "*New toolbar!!! New buttons!!! \n" +
                "*Tracks now pre-load 30 seconds instead of a single second \n" +
                "*Minor bug and UI fixes \n" +
                "*Upgraded to .NET 4.8 \n" +
                "*Global code clean-up\n";

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
