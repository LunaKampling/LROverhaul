using System;
using Gwen;
using Gwen.Controls;
using linerider.UI.Components;
using linerider.Utils;
using OpenTK;

namespace linerider.UI.Widgets
{
    public class ZoomBar : Panel
    {
        private readonly Editor _editor;
        private MultiSlider _slider;
        private Panel _resetCameraContainer;
        private bool _prevSuperZoom;
        private float _prevBaseZoom;
        private int _notches = 100;

        private double Min => Constants.MinimumZoom;
        private double Max => Settings.Computed.MaxZoom;
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

            Width = Constants.ScreenSize.Width / 10;

            Setup();
        }

        private void Setup()
        {
            _resetCameraContainer = new Panel(this)
            {
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                Dock = Dock.Right,
            };
            _ = new WidgetButton(_resetCameraContainer)
            {
                Dock = Dock.Right,
                Name = "Reset Camera",
                Icon = GameResources.icon_reset_camera.Bitmap,
                Action = (o, e) => _editor.ResetCamera(),
                TooltipHotkey = Hotkey.PlaybackResetCamera,
            };

            Panel sliderPanel = new Panel(this)
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

                SetZoomFromSlider(e.Value);
            };
            SetSliderFromZoom(_editor.BaseZoom);

            Panel textPanel = new Panel(this)
            {
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                MouseInputEnabled = false,
                Dock = Dock.Bottom,
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

        public void SetSliderFromZoom(float zoom)
        {
            double raw = (double)(zoom - Min) / (Max - Min);
            _slider.Value = (int)Math.Round(MinVal + raw * (MaxVal - MinVal));
        }

        public void SetZoomFromSlider(int value)
        {
            value = (int)MathHelper.Clamp(value, Constants.MinimumZoom, Settings.Computed.MaxZoom);

            double raw = (double)(value - MinVal) / (MaxVal - MinVal);
            _editor.Zoom = (float)Math.Round(Min + raw * (Max - Min));
        }

        public override void Think()
        {
            _resetCameraContainer.IsHidden = !_editor.UseUserZoom && _editor.BaseZoom == _editor.Timeline.GetFrameZoom(_editor.Offset);

            if (!_slider.Playhead.IsHeld && (_prevBaseZoom != _editor.BaseZoom || _prevSuperZoom != Settings.SuperZoom))
            {
                _prevBaseZoom = _editor.BaseZoom;
                _prevSuperZoom = Settings.SuperZoom;
                _slider.Max = (int)Max;
                SetSliderFromZoom(_editor.BaseZoom);
            }

            base.Think();
        }
    }
}