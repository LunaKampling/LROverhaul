using Gwen;
using Gwen.Controls;
using linerider.UI.Components;
using System;

namespace linerider.UI.Widgets
{
    public class Timeline : Panel
    {
        private const int _framesInSecond = 40;
        private const int _lengthMin = _framesInSecond;
        private readonly int _lengthDefault = _framesInSecond * Settings.DefaultTimelineLength;
        private readonly Editor _editor;
        private readonly GameCanvas _canvas;
        private MultiSlider _slider;
        private string _trackName;
        private Playhead _playheadFlag;
        private Playhead _playheadLimiter;
        private TrackLabel _flagTime;
        private TrackLabel _timeSeparator;
        private TrackLabel _totalTime;

        public bool Scrubbing => _slider.Playhead.IsHeld;

        public Timeline(ControlBase parent, Editor editor) : base(parent)
        {
            _canvas = (GameCanvas)GetCanvas();
            _editor = editor;
            _trackName = editor.Name;

            ShouldDrawBackground = false;
            AutoSizeToContents = true;
            MouseInputEnabled = false;

            Setup();
        }

        private void Setup()
        {
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
                Padding = new Padding(4, 11, 4, 0),
                Max = _lengthDefault,
            };
            _slider.ValueChanged += (o, e) =>
            {
                if (!e.ByUser)
                    return;

                _editor.SetFrame(e.Value, false);
                _editor.UpdateCamera();
                _editor.Scheduler.Reset();
                Audio.AudioService.EnsureSync();
            };

            _playheadLimiter = new Playhead(_slider)
            {
                Bitmap = GameResources.ux_playhead_limiter.Bitmap,
                Cursor = Cursors.SizeWE,
                Value = _slider.Max,
            };
            _playheadLimiter.Released += (o, e) =>
            {
                if (_slider.Value > _playheadLimiter.Value)
                {
                    _editor.SetFrame(_playheadLimiter.Value, false);
                    _editor.UpdateCamera();
                }
                _slider.Max = Math.Max(_lengthMin, _playheadLimiter.Value);
                _editor.Scheduler.Reset();
                Audio.AudioService.EnsureSync();
            };
            _playheadLimiter.SendToBack();

            _playheadFlag = new Playhead(_slider)
            {
                Bitmap = GameResources.ux_playhead_flag.Bitmap,
                Margin = new Margin(4, 0, -4, 0),
                Y = -9,
            };
            _playheadFlag.ValueChanged += (o, e) =>
            {
                _editor.SetFlagFrame(e.Value, false);
            };
            _playheadFlag.SendToBack();

            Panel textPanel = new Panel(this)
            {
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                MouseInputEnabled = false,
                Dock = Dock.Bottom,
                Padding = new Padding(
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
                    int iteration = _editor.IterationsOffset;
                    if (iteration == 6)
                        return "";

                    string label = $"Physics Iteration: {iteration}";

                    if (iteration == 0)
                        label += " (momentum tick)";

                    return label;
                },
            };
            _ = new TrackLabel(textPanel)
            {
                Dock = Dock.Left,
                Alignment = Pos.Center,
                TextRequest = (o, e) => GetTimeString(_slider.Value),
            };

            _totalTime = new TrackLabel(textPanel)
            {
                Dock = Dock.Right,
                Alignment = Pos.Center,
                TextRequest = (o, e) => GetTimeString(_playheadLimiter.IsHeld ? _playheadLimiter.Value : _slider.Max),
            };

            _timeSeparator = new TrackLabel(textPanel)
            {
                Dock = Dock.Right,
                Alignment = Pos.Center,
                Text = " / ",
            };

            _flagTime = new TrackLabel(textPanel)
            {
                Dock = Dock.Right,
                Alignment = Pos.Center,
                TextRequest = (o, e) => GetTimeString(_playheadFlag.Value),
            };
        }
        private string GetTimeString(int frameid)
        {
            string format = "mm\\:ss";
            string formatlong = "h\\:" + format;
            TimeSpan currts = TimeSpan.FromSeconds((double)frameid / _framesInSecond);
            string time = currts.ToString(currts.Hours > 0 ? formatlong : format);
            string frame = (frameid % _framesInSecond).ToString("D2");
            return time + ":" + frame;
        }
        public override void Think()
        {
            if (_editor.Name != _trackName)
            {
                _trackName = _editor.Name;
                _slider.Max = _lengthDefault;
            }

            if (_editor.Playing)
                _slider.Max = Math.Max(_slider.Max, _editor.Offset);

            _slider.Value = _editor.Offset;

            if (!_playheadLimiter.IsHeld && _playheadLimiter.Value != _slider.Max)
                _playheadLimiter.Value = _slider.Max;

            _playheadFlag.IsHidden = !_editor.HasFlag;
            _timeSeparator.IsHidden = !_editor.HasFlag;

            if (!_playheadFlag.IsHeld && _editor.HasFlag)
                _playheadFlag.Value = _editor.Flag.FrameID;

            _totalTime.Font = _playheadLimiter.IsHeld ? _canvas.Fonts.DefaultBold : _canvas.Fonts.Default;
            _flagTime.Font = _playheadFlag.IsHeld ? _canvas.Fonts.DefaultBold : _canvas.Fonts.Default;
            _flagTime.IsHidden = _playheadFlag.IsHidden;

            if (_playheadLimiter.Color != _slider.SliderColor)
                _playheadLimiter.Color = _slider.SliderColor;

            base.Think();
        }
    }
}