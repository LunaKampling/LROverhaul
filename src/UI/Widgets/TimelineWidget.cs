//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Drawing;
using Gwen;
using Gwen.Controls;
using linerider.Tools;

namespace linerider.UI
{
    public class TimelineWidget : WidgetContainer
    {
        public Playhead Playhead { get; private set; }
        private ControlBase _topbar;
        private ControlBase _bottombar;
        private Editor _editor;
        private GameCanvas _canvas;
        private ImageButton _speedincrease;
        private ImageButton _speeddecrease;
        private TrackLabel _timecurrent;
        private TrackLabel _iterations;
        private TrackLabel _timemax;
        public TimelineWidget(ControlBase parent, Editor editor) : base(parent)
        {
            _canvas = (GameCanvas)parent.GetCanvas();

            Margin margin = new Margin(_canvas.ScreenEdgeSpacing, 0, _canvas.ScreenEdgeSpacing, _canvas.ScreenEdgeSpacing);
            margin += new Margin(50, 0, 50, 0);

            Dock = Dock.Bottom;
            _editor = editor;
            Margin = margin;
            AutoSizeToContents = true;
            MouseInputEnabled = false;
            Setup();
        }
        private void Setup()
        {
            _bottombar = new Panel(this)
            {
                AutoSizeToContents = true,
                ShouldDrawBackground = false,
                MouseInputEnabled = false,
                Dock = Dock.Top,
            };
            _topbar = new Panel(this)
            {
                //AutoSizeToContents = true,
                ShouldDrawBackground = false,
                MouseInputEnabled = false,
                Dock = Dock.Top,
                Height = 32,
            };
            _speedincrease = CreateButton(_topbar, GameResources.fast_forward, "Increase Speed" + Settings.HotkeyToString(Hotkey.PlaybackSpeedUp, true));
            _speedincrease.Clicked += (o, e) => _editor.PlaybackSpeedUp();
            _speedincrease.Dock = Dock.Right;

            _speeddecrease = CreateButton(_topbar, GameResources.rewind, "Decrease Speed" + Settings.HotkeyToString(Hotkey.PlaybackSpeedDown, true));
            _speeddecrease.Clicked += (o, e) => _editor.PlaybackSpeedDown();
            _speeddecrease.Dock = Dock.Left;

            Playhead = new Playhead(_topbar, _editor);
            Playhead.Dock = Dock.Fill;
            Panel time = new Panel(_bottombar)
            {
                Dock = Dock.Fill,
                AutoSizeToContents = true,
                ShouldDrawBackground = false,
                MouseInputEnabled = false,
            };
            _iterations = new TrackLabel(time)
            {
                Dock = Dock.Fill,
                TextRequest = (o, e) =>
                {
                    switch (_editor.IterationsOffset)
                    {
                        case 0:
                            return "Physics Iteration: 0 (momentum tick)";
                        case 1:
                            return "Physics Iteration: 1";
                        case 2:
                            return "Physics Iteration: 2";
                        case 3:
                            return "Physics Iteration: 3";
                        case 4:
                            return "Physics Iteration: 4";
                        case 5:
                            return "Physics Iteration: 5";
                        default:
                            return "";
                    }
                },
                Alignment = Pos.Center,
            };
            _timecurrent = new TrackLabel(time)
            {
                Dock = Dock.Left,
                TextRequest = (o, e) =>
                {
                    string ret = GetTimeString((int)Math.Round(Playhead.Value));
                    return ret;
                },
                Alignment = Pos.Center,
            };
            _timemax = new TrackLabel(time)
            {
                Dock = Dock.Right,
                TextRequest = (o, e) =>
                {
                    string ret = GetTimeString(Playhead.DisplayMax);
                    return ret;
                },
                Alignment = Pos.Center,
            };
        }
        public override void Think()
        {
            _speeddecrease.IsHidden = !Settings.UIShowSpeedButtons;
            _speedincrease.IsHidden = !Settings.UIShowSpeedButtons;

            base.Think();
        }
        private void SetTextColor(object sender, int alpha)
        {
            var text = (Label)sender;
            text.TextColor = Color.FromArgb(alpha, _canvas.TextForeground);
        }
        private string GetTimeString(int frameid)
        {
            string formatstring = "mm\\:ss";
            string longformatstring = "h\\:" + formatstring;
            var currts = TimeSpan.FromSeconds(frameid / 40f);
            var format = currts.ToString(currts.Hours > 0 ? longformatstring : formatstring);
            var frame = (frameid % 40).ToString("D2");
            return format + ":" + frame;
        }
        private ImageButton CreateButton(ControlBase parent, Bitmap image, string tooltip)
        {
            ImageButton btn = new ImageButton(parent);
            btn.SetImage(image);
            btn.SetSize(32, 32);
            btn.Tooltip = tooltip;
            return btn;
        }

    }
}