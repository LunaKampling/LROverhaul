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
using Gwen;
using Gwen.Controls;
using linerider.UI.Components;
using linerider.Utils;
using System;
using System.Diagnostics;
using System.Linq;

namespace linerider.UI.Widgets
{
    public class InfoBarRight : WidgetContainer
    {
        private readonly Editor _editor;
        private readonly Stopwatch _fpswatch = new Stopwatch();

        private WidgetLabel _fpslabel;
        private WidgetLabel _riderspeedlabel;
        private WidgetLabel _zoomlabel;
        private WidgetLabel _playbackratelabel;
        private WidgetLabel _lockedcameralabel;
        private WidgetLabel _notifylabel;
        private Panel _resetcamerawrapper;

        private double ZoomRounded
        {
            get
            {
                double zoom = _editor.BaseZoom;
                return Math.Round(zoom, zoom > 100 ? 0 : zoom > 10 ? 1 : zoom > 1 ? 2 : 3);
            }
        }
        public InfoBarRight(ControlBase parent, Editor editor) : base(parent)
        {
            _editor = editor;
            OnThink += Think;
            Setup();
        }
        private void Think(object sender, EventArgs e)
        {
            bool rec = Settings.Local.RecordingMode;
            _fpslabel.IsHidden = rec && !Settings.Recording.ShowFps;
            _riderspeedlabel.IsHidden = rec && !Settings.Recording.ShowPpf;
            _zoomlabel.IsHidden = rec || Settings.UIShowZoom;
            _playbackratelabel.IsHidden = rec || _editor.Scheduler.Rate == 1;
            _lockedcameralabel.IsHidden = rec || !Settings.Local.LockCamera;
            _notifylabel.IsHidden = rec || string.IsNullOrEmpty(_editor.CurrentNotifyMessage);
            _resetcamerawrapper.IsHidden = rec || Settings.UIShowZoom || !_editor.UseUserZoom && _editor.BaseZoom == _editor.Timeline.GetFrameZoom(_editor.Offset);

            bool hasNoContent = Children.All(x => x.IsHidden);
            ShouldDrawBackground = !hasNoContent;
        }
        private void Setup()
        {
            _fpslabel = new WidgetLabel(this)
            {
                Dock = Dock.Top,
                Margin = Margin.Zero,
                Alignment = Pos.Right | Pos.CenterV,
                TextRequest = (o, currenttext) =>
                {
                    bool rec = Settings.Local.RecordingMode;
                    if (rec && Settings.Recording.ShowFps)
                    {
                        return Settings.RecordSmooth ? $"{Constants.FrameRate} FPS" : $"{Constants.PhysicsRate} FPS";
                    }
                    else if (!_fpswatch.IsRunning || _fpswatch.ElapsedMilliseconds > 500)
                    {
                        _fpswatch.Restart();
                        return Math.Round(_editor.FramerateCounter.FPS) + " FPS";
                    }
                    return currenttext;
                },
            };

            _riderspeedlabel = new WidgetLabel(this)
            {
                Dock = Dock.Top,
                Margin = new Margin(0, WidgetItemSpacing, 0, 0),
                Alignment = Pos.Right | Pos.CenterV,
                TextRequest = (o, e) =>
                {
                    double ppf = _editor.RenderRider.CalculateMomentum().Length;
                    double n = (double)_riderspeedlabel.UserData;
                    if (n != ppf)
                    {
                        double roundppf = Math.Round(ppf, 2);
                        _riderspeedlabel.UserData = roundppf;
                        return string.Format("{0:N2}", Math.Round(ppf, 2)) + " P/f";
                    }
                    return e;
                },
                Text = "0.00 P/f",
                UserData = 0.0,
            };

            _zoomlabel = new WidgetLabel(this)
            {
                Dock = Dock.Top,
                Alignment = Pos.Right | Pos.CenterV,
                Margin = new Margin(0, WidgetItemSpacing, 0, 0),
                TextRequest = (o, e) =>
                {
                    string text = $"Zoom: {ZoomRounded}\u00D7";
                    return _editor.UseUserZoom ? $"{text} *" : text;
                },
            };

            _playbackratelabel = new WidgetLabel(this)
            {
                Dock = Dock.Top,
                Alignment = Pos.Right | Pos.CenterV,
                Margin = new Margin(0, WidgetItemSpacing, 0, 0),
                TextRequest = (o, e) => $"Sim Speed: {_editor.Scheduler.Rate}x",
            };

            _lockedcameralabel = new WidgetLabel(this)
            {
                Dock = Dock.Top,
                Alignment = Pos.Right | Pos.CenterV,
                Margin = new Margin(0, WidgetItemSpacing, 0, 0),
                Text = "Camera is locked",
            };

            _notifylabel = new WidgetLabel(this)
            {
                Dock = Dock.Top,
                Alignment = Pos.Right | Pos.CenterV,
                Margin = new Margin(0, WidgetItemSpacing, 0, 0),
                TextRequest = (o, currenttext) =>
                {
                    return _editor.CurrentNotifyMessage;
                },
            };

            _resetcamerawrapper = new Panel(this)
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
            };

            _ = new WidgetButton(_resetcamerawrapper)
            {
                Dock = Dock.Right,
                Name = "Reset Camera",
                Icon = GameResources.icon_reset_camera.Bitmap,
                Action = (o, e) => _editor.ResetCamera(),
                TooltipHotkey = Hotkey.PlaybackResetCamera,
            };

        }
    }
}