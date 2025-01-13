using Gwen;
using Gwen.Controls;
using linerider.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace linerider.UI
{
    public class ChangelogWindow : DialogBase
    {
        private List<string> _changelogLines;
        public ChangelogWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            List<string> changelogLines = AssemblyInfo.ChangelogLines;

            // Use first changelog line as window title
            string title = changelogLines[0];
            changelogLines.RemoveRange(0, 2); // Remove title and following empty line
            if (title.StartsWith("#"))
                title = title.Substring(1).Trim();

            _changelogLines = changelogLines;
            Title = title;

            MinimumSize = new Size(450, 400);
            Setup();
            MakeModal(true);
            DisableResizing();
        }

        private void Setup()
        {
            Gwen.Font fontNormal = _canvas.Fonts.Default;
            Gwen.Font fontBold = _canvas.Fonts.DefaultBold;
            Color colorNormal = Skin.Colors.Text.Foreground;
            Color colorGray = Color.Gray;

            ScrollControl scrollContainer = new ScrollControl(this)
            {
                Dock = Dock.Fill,
            };
            scrollContainer.EnableScroll(false, true);

            RichLabel textContainer = new RichLabel(scrollContainer)
            {
                Dock = Dock.Top,
                AutoSizeToContents = true,
                Margin = new Margin(0, 0, 15, 0),
            };
            foreach (string l in _changelogLines)
            {
                if (string.IsNullOrEmpty(l))
                {
                    textContainer.AddLineBreak();
                    continue;
                }

                string line = l;
                Gwen.Font font = fontNormal;
                Color color = colorNormal;

                // Header line (starts with any # count)
                if (line.StartsWith("#"))
                {
                    line = line.Trim(new char[] { '#', ' ' });
                    font = fontBold;
                }

                // Italic line (cannot render it italic so rendering it gray colored)
                else if (line.StartsWith("*") && line.EndsWith("*") || line.StartsWith("_") && line.EndsWith("_"))
                {
                    line = line.Trim(new char[] { '*', '_', ' ' });
                    color = colorGray;
                }

                // Bold line
                else if (line.StartsWith("**") && line.EndsWith("**") || line.StartsWith("__") && line.EndsWith("__"))
                {
                    line = line.Trim(new char[] { '*', '_', ' ' });
                    font = fontBold;
                }

                textContainer.AddText(line, color, font);
                textContainer.AddLineBreak();
            }

            ControlBase bottomcontainer = new ControlBase(this)
            {
                Margin = new Margin(0, 10, 0, 0),
                Dock = Dock.Bottom,
                AutoSizeToContents = true
            };

            Button btngithub = new Button(null)
            {
                Text = "Previous Changelogs",
                Dock = Dock.Left,
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
                    _ = MessageBox.Show(_canvas, "Unable to open your browser.", "Error!");
                }
                _ = Close();
            };

            Button btnclose = new Button(null)
            {
                Text = "Close",
                Dock = Dock.Right,
                AutoSizeToContents = true,
            };
            btnclose.Clicked += CloseButtonPressed;

            ControlBase buttoncontainer = new ControlBase(bottomcontainer)
            {
                Margin = Margin.Zero,
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                Children =
                {
                    btnclose,
                    btngithub,
                }
            };
        }

        protected override void CloseButtonPressed(ControlBase control, EventArgs args)
        {
            // Force update settings so it contains actual LRO version
            Settings.Save();

            base.CloseButtonPressed(control, args);
        }
    }
}
