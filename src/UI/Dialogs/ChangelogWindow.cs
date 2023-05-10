using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Gwen;
using Gwen.Controls;
using linerider.Tools;
using linerider.Utils;
using linerider.IO;

namespace linerider.UI
{
    public class ChangelogWindow : DialogBase
    {
        public ChangelogWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = "Changelog for " + Program.Version + Program.TestVersion;
            AutoSizeToContents = true;

            var changelogText =
                "11/29/2022 \n" +    
                    "*Fixed a bug with night mode not rendering properly \n" +
                    "*Fixed a bug with toolbar appearing too small on higher resolution screens \n" +
                    "*Fixed a bug with Numpad number hotkeys appearing the same as normal numbers \n" +
                    "*Temporarily disabled Check for Updates \n" +
                    "\n" +
                    "*Note: Toolbar buttons are still 32px x 32px. \n*For high resolution screens this will appear as low-res. \n*Working on upscaling them as of this changelog \n" +
                    "\n" +
                "11/27/2022 \n" + 
                    "*Removed Discord Integration \n" +
                    "*Added Smooth Pencil. Activated by re-activating the pencil tool when active \n" +
                    "*Smooth Pencil is fully customizable in preferenceswindow. Go nuts! \n" +
                    "*Cleaned up parts of preferenceswindow \n" +
                    "*Added Coordinate Menu. Hotkeys to copy to clipboard are Ctrl/Alt + Numpad \n" +
                    "\n" +
                    "*Note: Remount and Bezier tool have yet to be fixed \n";

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
                Close();
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
                Close();
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
                    MessageBox.Show(parent, "Unable to open your browser.", "Error!");
                }
                Close();
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
            
            RichLabel l = new RichLabel(this);
            l.Dock = Dock.Top;
            l.AutoSizeToContents = true;
            l.AddText(changelogText, Skin.Colors.Text.Foreground);
            MakeModal(true);
            DisableResizing();
        }
        
        private void CreateLabeledControl(ControlBase parent, string label, ControlBase control)
        {
            control.Dock = Dock.Right;
            ControlBase container = new ControlBase(parent)
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
