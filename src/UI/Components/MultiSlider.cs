using Gwen.Controls;
using Gwen;
using Gwen.Skin.Texturing;
using System;
using Gwen.Input;
using OpenTK;
using OpenTK.Mathematics;
using SkiaSharp;

namespace linerider.UI.Components
{
    public class PointEventArgs : EventArgs
    {
        public readonly int X;
        public readonly int Y;
        public PointEventArgs() : this(0, 0)
        { }
        public PointEventArgs(Point point) : this(point.X, point.Y)
        { }
        public PointEventArgs(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class PlayheadValueEventArgs : EventArgs
    {
        public readonly int Value;
        public readonly bool ByUser;
        public PlayheadValueEventArgs(int value, bool byUser)
        {
            Value = value;
            ByUser = byUser;
        }
    }

    public class PlayheadRangeEventArgs : EventArgs
    {
        public readonly int Min;
        public readonly int Max;
        public PlayheadRangeEventArgs(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

    /// <summary>
    /// Horizontal slider with multiple playheads support.
    /// It also auto adotps current color scheme and UI scale.
    /// </summary>
    public class MultiSlider : ControlBase
    {
        private Bordered _background;
        private Color _color1;
        private Color _color2;
        private Color _backgroundColor;
        private Padding _padding = Padding.Zero;
        private SKBitmap _bitmap;
        private int _oldWidth;
        private bool _held;

        /// <value>To avoid precise slider clicks.</value>
        private readonly int _extraDragAreaSize = Utility.NumberToCurrentScale(6);

        private int _min = 0;
        private int _max = 100;

        private event GwenEventHandler<PointEventArgs> Released;
        private event GwenEventHandler<PointEventArgs> Dragged;
        private event GwenEventHandler<PointEventArgs> Pressed;

        public int MinX => base.Padding.Left;
        public int MaxX => Width - base.Padding.Right;
        public new int Height => base.Height;
        public SKBitmap Bitmap => _bitmap;
        public Color SliderColor => _backgroundColor;

        /// <summary>
        /// Default slider playhead.
        /// </summary>
        public Playhead Playhead { get; private set; }

        /// <summary>
        /// Invoked when the value has been changed.
        /// </summary>
        public event GwenEventHandler<PlayheadValueEventArgs> ValueChanged;

        /// <summary>
        /// Invoked when min or max values have been changed.
        /// </summary>
        public event GwenEventHandler<PlayheadRangeEventArgs> RangeChanged;

        /// <summary>
        /// Invoked when control is resized (e.g. on window resize).
        /// </summary>
        public event GwenEventHandler<EventArgs> Resized;

        /// <summary>
        /// Minimum value.
        /// </summary>
        public int Min
        {
            get => _min;
            set
            {
                int oldMin = _min;
                _min = Math.Max(0, value);

                if (_min != oldMin)
                    RangeChanged?.Invoke(this, new PlayheadRangeEventArgs(Min, Max));
            }
        }

        /// <summary>
        /// Maximum value.
        /// </summary>
        public int Max
        {
            get => _max;
            set
            {
                int oldMax = _max;
                _max = Math.Max(Min, value);

                if (_max != oldMax)
                    RangeChanged?.Invoke(this, new PlayheadRangeEventArgs(Min, Max));
            }
        }

        /// <summary>
        /// Current value.
        /// </summary>
        public int Value
        {
            get => Playhead.Value;
            set => Playhead.Value = value;
        }

        /// <summary>
        /// Represents inner spacing. Auto adopts UI scale. You might need to set this if you plan to add extra playheads.
        /// </summary>
        public new Padding Padding
        {
            get => _padding;
            set
            {
                _padding = value;
                RecalculatePadding();
            }
        }

        public Padding InternalPadding
        {
            get
            {
                int playheadOverflowTopBottom = 0;
                int playheadOverflowSides = 0;

                if (Playhead != null)
                {
                    double rawTopBottomOverflow = Playhead.Bitmap.Height - Bitmap.Height;
                    playheadOverflowTopBottom = Math.Max(0, (int)Math.Ceiling(rawTopBottomOverflow / 2));
                    playheadOverflowSides = Playhead.Size.Width / 2;
                }

                return new Padding(playheadOverflowSides, playheadOverflowTopBottom, playheadOverflowSides, playheadOverflowTopBottom);
            }
        }

        public Padding AbsolutePadding => base.Padding + InternalPadding;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSlider"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public MultiSlider(ControlBase parent) : base(parent)
        {
            KeyboardInputEnabled = false;
            AutoSizeToContents = false;
            MouseInputEnabled = true;
            IsTabable = false;

            SetupBackground();
            SetupDefaultPlayhead();
        }

        private void SetupBackground()
        {
            int height = Utility.NumberToCurrentScale(3);
            Utils.RoundedSquareTexture rst = new Utils.RoundedSquareTexture(height, stretchVertically: false);
            _background = rst.Bordered;
            _bitmap = rst.Bitmap;
        }

        private void SetupDefaultPlayhead()
        {
            Playhead = new Playhead(this);
            Playhead.ValueChanged += (o, e) => ValueChanged?.Invoke(this, e);

            Pressed += (o, e) => ForwardEventToPlayhead(e);
            Dragged += (o, e) => ForwardEventToPlayhead(e);

            RecalculatePadding();
        }

        private void RecalculatePadding()
        {
            base.Padding = new Padding(
                Utility.NumberToCurrentScale(_padding.Left),
                Utility.NumberToCurrentScale(_padding.Top) + InternalPadding.Top,
                Utility.NumberToCurrentScale(_padding.Right),
                Utility.NumberToCurrentScale(_padding.Bottom) + InternalPadding.Bottom
            );
            base.Height = _bitmap.Height + base.Padding.Height;

            Playhead.Y = 0;
            Playhead.SetPosFromValue(Playhead.Value);
        }

        private void ForwardEventToPlayhead(PointEventArgs e)
        {
            int x = e.X - Playhead.Bitmap.Width / 2;
            Playhead.SetValueFromPos(x, true);
        }

        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
            if (down)
            {
                Point Pos = CanvasPosToLocal(new Point(x, y));

                bool cursorOverSlider = Pos.Y > base.Padding.Top - _extraDragAreaSize
                    && Pos.Y < Height - base.Padding.Bottom + _extraDragAreaSize
                    && Pos.X > base.Padding.Left - _extraDragAreaSize
                    && Pos.X < Width - base.Padding.Right + _extraDragAreaSize;

                if (!cursorOverSlider)
                    return;

                _held = true;
                InputHandler.MouseFocus = this;
                Pressed?.Invoke(this, new PointEventArgs(Pos));
            }
            else
            {
                _held = false;
                InputHandler.MouseFocus = null;
                Released?.Invoke(this, new PointEventArgs());
            }
        }
        protected override void OnMouseMoved(int x, int y, int dx, int dy)
        {
            if (!_held)
                return;

            Point Pos = CanvasPosToLocal(new Point(x, y));
            Pos.X = MathHelper.Clamp(Pos.X, 0, Width);
            Pos.Y = MathHelper.Clamp(Pos.Y, 0, Height);
            Dragged?.Invoke(this, new PointEventArgs(Pos));
        }
        
        public override void Think()
        {
            if (Width != _oldWidth)
            {
                _oldWidth = Width;
                Resized?.Invoke(this, EventArgs.Empty);
            }

            if (_color1 != Settings.Computed.BGColor || _color2 != Settings.Computed.LineColor)
            {
                _color1 = Settings.Computed.BGColor;
                _color2 = Settings.Computed.LineColor;
                _backgroundColor = Utility.MixColors(_color1, _color2, 0.25f);
            }

            base.Think();
        }

        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            Rectangle bounds = new Rectangle(
                MinX + InternalPadding.Left,
                RenderBounds.Y + base.Padding.Top,
                MaxX - MinX - InternalPadding.Width,
                RenderBounds.Height - base.Padding.Height
            );

            _background.Draw(skin.Renderer, bounds, _backgroundColor);
        }
    }
}