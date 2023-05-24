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
using System.Diagnostics;
using Gwen;
using Gwen.Controls;

namespace linerider.UI
{
    public class InfoBarRight : WidgetContainer
    {
        private Editor _editor;
        private GameCanvas _canvas;
        private Stopwatch _fpswatch = new Stopwatch();

        private TrackLabel _fpslabel;
        private TrackLabel _riderspeedlabel;
        private TrackLabel _zoomlabel;
        private TrackLabel _playbackratelabel;
        private TrackLabel _notifylabel;

        private Panel _iconpanel;
        private Sprite _usercamerasprite;

        private double _zoomrounded
        {
            get
            {
                double zoom = _editor.Zoom;
                return Math.Round(zoom, zoom > 100 ? 0 : zoom > 10 ? 1 : zoom > 1 ? 2 : 3);
            }
        }
        public InfoBarRight(ControlBase parent, Editor editor) : base(parent)
        {
            _canvas = (GameCanvas)parent.GetCanvas();
            Dock = Dock.Right;
            _editor = editor;
            AutoSizeToContents = true;
            ShouldDrawBackground = false;
            Setup();
            OnThink += Think;
            Padding = Padding.Five;
        }
        private void Think(object sender, EventArgs e)
        {
            bool rec = Settings.Local.RecordingMode;
            _fpslabel.IsHidden = rec && !Settings.Recording.ShowFps;
            _riderspeedlabel.IsHidden = rec && !Settings.Recording.ShowPpf;
            _zoomlabel.IsHidden = rec || Settings.UIShowZoom;
            _playbackratelabel.IsHidden = rec || _editor.Scheduler.UpdatesPerSecond == 40;
            _notifylabel.IsHidden = rec || string.IsNullOrEmpty(_editor.CurrentNotifyMessage);

            _usercamerasprite.IsHidden = !_editor.UseUserZoom && _editor.BaseZoom == _editor.Timeline.GetFrameZoom(_editor.Offset);
            _iconpanel.IsHidden = _usercamerasprite.IsHidden;
        }
        private void Setup()
        {
            Margin = new Margin(0, _canvas.ScreenEdgeSpacing, _canvas.ScreenEdgeSpacing, 0);

            _fpslabel = new TrackLabel(this)
            {
                Dock = Dock.Top,
                Margin = new Margin(0, 0, 0, 0),
                Alignment = Pos.Right | Pos.CenterV,
                TextRequest = (o, currenttext) =>
                {
                    bool rec = Settings.Local.RecordingMode;
                    if (rec && Settings.Recording.ShowFps)
                    {
                        return Settings.RecordSmooth ? "60 FPS" : "40 FPS";
                    }
                    else if (!_fpswatch.IsRunning || _fpswatch.ElapsedMilliseconds > 500)
                    {
                        _fpswatch.Restart();
                        return Math.Round(_editor.FramerateCounter.FPS) + " FPS";
                    }
                    return currenttext;
                },
            };

            _riderspeedlabel = new TrackLabel(this)
            {
                Dock = Dock.Top,
                Margin = new Margin(0, _canvas.WidgetSpacing, 0, 0),
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

            _zoomlabel = new TrackLabel(this)
            {
                Dock = Dock.Top,
                Alignment = Pos.Right | Pos.CenterV,
                Margin = new Margin(0, _canvas.WidgetSpacing, 0, 0),
                TextRequest = (o, e) =>
                {
                    string text = $"Zoom: {_zoomrounded}x";
                    return _editor.UseUserZoom ? $"{text} *" : text;
                },
            };

            _playbackratelabel = new TrackLabel(this)
            {
                Dock = Dock.Top,
                Alignment = Pos.Right | Pos.CenterV,
                Margin = new Margin(0, _canvas.WidgetSpacing, 0, 0),
                TextRequest = (o, e) =>
                {
                    double rate = Math.Round(_editor.Scheduler.UpdatesPerSecond / 40.0, 3);
                    return rate == 1 ? "" : $"Sim Speed: {rate}x";
                },
            };

            _notifylabel = new TrackLabel(this)
            {
                Dock = Dock.Top,
                Alignment = Pos.Right | Pos.CenterV,
                Margin = new Margin(0, _canvas.WidgetSpacing, 0, 0),
                TextRequest = (o, currenttext) =>
                {
                    return _editor.CurrentNotifyMessage;
                },
            };

            _iconpanel = new Panel(this)
            {
                ShouldDrawBackground = false,
                Dock = Dock.Top,
                Width = 32,
                Height = 32,
                Margin = new Margin(0, _canvas.WidgetSpacing, 0, 0),
            };
            _usercamerasprite = new Sprite(_iconpanel)
            {
                Dock = Dock.Right,
                IsHidden = true,
                Tooltip = "Click to Reset Camera" + Settings.HotkeyToString(Hotkey.PlaybackResetCamera, true),
                MouseInputEnabled = true,
                TooltipDelay = 0,
            };
            _usercamerasprite.Clicked += (o,e)=>
            {
                _editor.Zoom = _editor.Timeline.GetFrameZoom(_editor.Offset);
                _editor.UseUserZoom = false;
                _editor.UpdateCamera();
            };
            _usercamerasprite.SetImage(GameResources.camera_need_reset);

        }
    }
}