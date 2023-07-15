using Gwen;
using Gwen.ControlInternal;
using Gwen.Controls;
using OpenTK;
using System;
using System.Drawing;

namespace linerider.UI.Components
{
    /// <summary>
    /// Playhead to use with <see cref="MultiSlider"/> class.
    /// </summary>
    public class Playhead : Dragger
    {
        private readonly MultiSlider _parent;
        private Texture _texture;
        private Bitmap _bitmap;
        private int _value;
        private int _y;

        private Color _colorOverride = Color.Empty;
        private Color ColorDefault => IsHovered ? Utility.MixColors(Settings.Computed.BGColor, Settings.Computed.LineColor, 0.5f) : Settings.Computed.LineColor;
        private Color ColorCurrent => _colorOverride.IsEmpty ? ColorDefault : _colorOverride;

        private int Min => _parent.Min;
        private int Max => _parent.Max;
        private int MinX => _parent.MinX + Margin.Left + (_parent.InternalPadding.Left - Width / 2);
        private int MaxX => _parent.MaxX - Width - Margin.Right - (_parent.InternalPadding.Right - Width / 2);

        /// <summary>
        /// Invoked when the value has been changed.
        /// </summary>
        public event GwenEventHandler<PlayheadValueEventArgs> ValueChanged;

        /// <summary>
        /// Invoked when the control has been left-clicked.
        /// </summary>
        public override event GwenEventHandler<ClickedEventArgs> Clicked;

        /// <summary>
        /// Playhead bitmap.
        /// </summary>
        public Bitmap Bitmap
        {
            get => _bitmap ?? new Bitmap(1, 1);
            set
            {
                _texture?.Dispose();
                Texture tx = new Texture(Skin.Renderer);
                Gwen.Renderer.OpenTK.LoadTextureInternal(tx, value);
                Size = value.Size;
                _texture = tx;
                _bitmap = value;
                Y = _y;
            }
        }

        /// <summary>
        /// Current value.
        /// </summary>
        public int Value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;

                int oldValue = _value;
                _value = MathHelper.Clamp(value, Min, Max);
                if (_value == oldValue)
                    return;

                SetPosFromValue(_value);
                ValueChanged?.Invoke(this, new PlayheadValueEventArgs(_value, m_Held));
            }
        }

        /// <summary>
        /// When <c>falce</c>, playhead cannot be dragged.
        /// </summary>
        public bool AllowDragging = true;

        /// <summary>
        /// Function to define the tooltip text before render.
        /// </summary>
        public TextRequestHandler TooltipRequest = null;

        /// <summary>
        /// Playhead position relative to slider. Auto adopts UI scale.
        /// Set negative value to put it above the slider or positive value to put it below.
        /// </summary>
        public new int Y
        {
            get => _y;
            set
            {
                _y = value;
                base.Y = Utility.NumberToCurrentScale(_parent.Padding.Top) + Utility.NumberToCurrentScale(value) + _parent.Bitmap.Height / 2 - Bitmap.Height / 2 + _parent.InternalPadding.Top;
            }
        }

        /// <summary>
        /// Represents outer spacing.
        /// </summary>
        public new Margin Margin
        {
            get => base.Margin;
            set
            {
                base.Margin = new Margin(
                    Utility.NumberToCurrentScale(value.Left),
                    Utility.NumberToCurrentScale(value.Top),
                    Utility.NumberToCurrentScale(value.Right),
                    Utility.NumberToCurrentScale(value.Bottom)
                );
                if (Value == 0)
                    X = MinX;
            }
        }

        /// <summary>
        /// Custom playhead color. Set it to <c>Color.Empty</c> to reset color.
        /// </summary>
        public Color Color
        {
            get => _colorOverride;
            set => _colorOverride = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Playhead"/> class.
        /// </summary>
        /// <param name="parent">MultiSlider instance.</param>
        public Playhead(MultiSlider parent) : base(parent)
        {
            _parent = parent;

            KeyboardInputEnabled = false;
            MouseInputEnabled = true;
            RestrictToParent = true;
            ToolTipProvider = true;
            IsTabable = false;
            m_Target = this;

            Setup();
        }

        private void Setup()
        {
            X = MinX;
            Y = 0;

            Point startPos = Point.Empty;
            Pressed += (o, e) => startPos = new Point(X, Y);
            Dragged += (o, e) => OnDragged(startPos);

            Bitmap = GameResources.ux_playhead_main.Bitmap;

            _parent.Resized += (o, e) => SetPosFromValue(_value);
            _parent.RangeChanged += (o, e) =>
            {
                if (_value > e.Max)
                    Value = e.Max;
                else if (_value < e.Min)
                    Value = e.Min;
                else
                    SetPosFromValue(Value);
            };
        }

        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
            base.OnMouseClickedLeft(x, y, down);

            // "Clicked" event
            if (!down)
            {
                Point releasePos = CanvasPosToLocal(new Point(x, y));
                bool releasedInsideBounds = releasePos.X > 0 && releasePos.Y > 0 && releasePos.X <= RenderBounds.Width && releasePos.Y <= RenderBounds.Height;

                if (releasedInsideBounds)
                    Clicked?.Invoke(this, new ClickedEventArgs(x, y, down));
            }
        }

        private void OnDragged(Point startPos)
        {
            Y = startPos.Y; // Lock Y position

            if (AllowDragging)
                SetValueFromPos(X);
            else
                X = startPos.X;

        }

        /// <summary>
        /// Set value based on x position.
        /// </summary>
        /// <remarks>
        /// Used by <see cref="MultiSlider"/>. Most likely you don't need to invoke this method manually.
        /// </remarks>
        /// <param name="x">Playhead X position</param>
        public void SetValueFromPos(int x)
        {
            SetValueFromPos(x, false);
        }

        /// <summary>
        /// Set value based on x position.
        /// </summary>
        /// <remarks>
        /// Used by <see cref="MultiSlider"/>. Most likely you don't need to invoke this method manually.
        /// </remarks>
        /// <param name="x">Playhead X position</param>
        /// <param name="triggerAsUser">Affects <c>ValueChanged</c>'s <c>byUser</c> argument</param>
        public void SetValueFromPos(int x, bool triggerAsUser)
        {
            x = MathHelper.Clamp(x, MinX, MaxX);

            double raw = (double)(x - MinX) / (MaxX - MinX);
            _value = (int)Math.Round(Min + raw * (Max - Min));
            SetPosFromValue(_value);
            ValueChanged?.Invoke(this, new PlayheadValueEventArgs(Value, triggerAsUser || m_Held));
        }

        /// <summary>
        /// Set x position based on value.
        /// </summary>
        /// <remarks>
        /// Used by <see cref="MultiSlider"/>. Most likely you want to use <see cref="Value"/> instead. 
        /// </remarks>
        /// <param name="value">New value</param>
        public void SetPosFromValue(int value)
        {
            value = MathHelper.Clamp(value, Min, Max);

            double raw = (double)(value - Min) / (Max - Min);
            X = (int)Math.Round(MinX + raw * (MaxX - MinX));
        }
        public override void Think()
        {
            if (TooltipRequest != null && IsHovered)
            {
                Tooltip = TooltipRequest(this, Tooltip);
            }
            base.Think();
        }

        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            skin.Renderer.DrawColor = Color.FromArgb(255, ColorCurrent);
            skin.Renderer.DrawTexturedRect(_texture, RenderBounds);
        }

        public override void Dispose()
        {
            _texture?.Dispose();
            base.Dispose();
        }
    }
}