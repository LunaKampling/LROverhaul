using Gwen;
using Gwen.Controls;
using linerider.UI.Components;
using linerider.Utils;
using OpenTK.Mathematics;
using System;

namespace linerider.UI.Widgets
{
    public class ZoomBar : Panel
    {
        private readonly Editor _editor;
        private MultiSlider _slider;
        private Playhead _playheadDefaultZoom;
        private bool _prevSuperZoom;
        private float _prevBaseZoom;
        private int _notches = 200;

        private double MinZoom => Constants.MinimumZoom;
        private double MaxZoom => Settings.Computed.MaxZoom;
        private double MinVal => _slider.Min;
        private double MaxVal => _slider.Max;

        private double ZoomRounded
        {
            get
            {
                double zoom = _editor.BaseZoom;
                return Math.Round(zoom, zoom > 100 ? 0 : zoom > 10 ? 1 : zoom > 1 ? 2 : 3);
            }
        }

        public ZoomBar(ControlBase parent, Editor editor) : base(parent)
        {
            _editor = editor;

            ShouldDrawBackground = false;
            MouseInputEnabled = false;

            // TODO: Make width dynamic?
            Width = (int)(Constants.WindowSize.Width / (10 / Settings.Computed.UIScale));

            Setup();
        }

        private void Setup()
        {
            Panel sliderPanel = new(this)
            {
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                MouseInputEnabled = false,
                Dock = Dock.Top,
            };

            _slider = new MultiSlider(sliderPanel)
            {
                Dock = Dock.Bottom,
                Padding = new Padding(4, 11, 4, 0), // Same as in TimelineBar
                Max = _notches,
            };
            _slider.ValueChanged += (o, e) =>
            {
                if (!e.ByUser)
                    return;

                _editor.Zoom = SliderValueToZoom(e.Value);
            };
            _slider.Value = ZoomToSliderValue(_editor.BaseZoom);

            _playheadDefaultZoom = new Playhead(_slider)
            {
                Cursor = Cursors.Hand,
                Bitmap = GameResources.ux_playhead_defaultzoom.Bitmap,
                AllowDragging = false,
                Y = -9,
                TooltipRequest = (o, e) =>
                {
                    float frameZoom = _editor.Timeline.GetFrameZoom(_editor.Offset);
                    string result = $"Frame actual zoom: {frameZoom}x\nClick to set as current zoom";
                    if (Hotkey.PlaybackResetCamera != Hotkey.None)
                        result += $" (or press {Settings.HotkeyToString(Hotkey.PlaybackResetCamera)})";
                    return result;
                },
            };
            _playheadDefaultZoom.Clicked += (o, e) => _editor.ResetCamera();
            _playheadDefaultZoom.SendToBack();

            Panel textPanel = new(this)
            {
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                MouseInputEnabled = false,
                Dock = Dock.Bottom,
                Margin = new Margin(0, WidgetContainer.WidgetMargin, 0, 0),
                Padding = new Padding( // Same as in TimelineBar
                    _slider.AbsolutePadding.Left,
                    WidgetContainer.WidgetPadding,
                    _slider.AbsolutePadding.Right,
                    0
                ),
            };

            _ = new TrackLabel(textPanel)
            {
                Dock = Dock.Fill,
                Alignment = Pos.Center,
                TextRequest = (o, e) =>
                {
                    return $"\u00A0{ZoomRounded}\u00D7";
                },
            };
        }

        /// <summary>
        /// Nonlinearly converts slider value to zoom value
        /// </summary>
        /// <param name="value">Slider value</param>
        /// <returns>Zoom value</returns>
        public float SliderValueToZoom(int value)
        {
            // Legacy linear method just for reference
            //double raw = (double)(value - MinVal) / (MaxVal - MinVal);
            //return (float)(MinZoom + raw * (MaxZoom - MinZoom));

            value = (int)MathHelper.Clamp(value, MinVal, MaxVal);

            double maxValRelative = MaxVal - MinVal;
            double maxZoomRelative = MaxZoom - MinZoom;
            double valRelative = value - MinVal;

            double mul = (Math.Pow(maxValRelative + 1, valRelative / maxValRelative) - 1) / maxValRelative;
            double result = maxZoomRelative * mul + MinZoom;

            return (float)result;
        }

        /// <summary>
        /// Nonlinearly converts zoom value to slider value
        /// </summary>
        /// <param name="zoom">Zoom value</param>
        /// <returns>Slider value</returns>
        public int ZoomToSliderValue(float zoom)
        {
            // Legacy linear method just for reference
            //double raw = (double)(zoom - MinZoom) / (MaxZoom - MinZoom);
            //return (int)Math.Round(MinVal + raw * (MaxVal - MinVal));

            zoom = (float)MathHelper.Clamp(zoom, MinZoom, MaxZoom);

            double maxValRelative = MaxVal - MinVal;
            double maxZoomRelative = MaxZoom - MinZoom;
            double zoomRelative = zoom - MinZoom;

            double raw = Math.Log(zoomRelative / maxZoomRelative * maxValRelative + 1) * maxValRelative / Math.Log(maxValRelative + 1);
            double result = raw + MinVal;

            return (int)Math.Round(result);
        }

        public override void Think()
        {
            _playheadDefaultZoom.IsHidden = !_editor.UseUserZoom && _editor.BaseZoom == _editor.Timeline.GetFrameZoom(_editor.Offset);

            if (!_playheadDefaultZoom.IsHidden)
            {
                float frameZoom = _editor.Timeline.GetFrameZoom(_editor.Offset);
                _playheadDefaultZoom.Value = ZoomToSliderValue(frameZoom);
            }

            if (!_slider.Playhead.IsHeld && (_prevBaseZoom != _editor.BaseZoom || _prevSuperZoom != Settings.SuperZoom))
            {
                _prevBaseZoom = _editor.BaseZoom;
                _prevSuperZoom = Settings.SuperZoom;
                _slider.Value = ZoomToSliderValue(_editor.BaseZoom);
            }

            base.Think();
        }
    }
}