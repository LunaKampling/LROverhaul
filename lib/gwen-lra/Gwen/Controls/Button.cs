using Gwen.Input;
using System;
using System.Drawing;

namespace Gwen.Controls
{
    /// <summary>
    /// Button control.
    /// </summary>
    public class Button : Label
    {
        #region Events

        /// <summary>
        /// Invoked when the button is pressed.
        /// </summary>
        public event GwenEventHandler<EventArgs> Pressed;

        /// <summary>
        /// Invoked when the button is released.
        /// </summary>
        public event GwenEventHandler<EventArgs> Released;

        /// <summary>
        /// Invoked when the button's toggle state has changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> Toggled;

        /// <summary>
        /// Invoked when the button's toggle state has changed to Off.
        /// </summary>
        public event GwenEventHandler<EventArgs> ToggledOff;

        /// <summary>
        /// Invoked when the button's toggle state has changed to On.
        /// </summary>
        public event GwenEventHandler<EventArgs> ToggledOn;

        #endregion Events

        #region Properties
        protected override Color CurrentColor => IsDisabled
                    ? Skin.Colors.Text.Disabled
                    : IsDepressed || ToggleState ? Skin.Colors.Text.Highlight : IsHovered ? Skin.Colors.Text.Contrast : Skin.Colors.Text.Foreground;
        /// <summary>
        /// Indicates whether the button is depressed.
        /// </summary>
        public bool IsDepressed
        {
            get => m_Depressed;
            set
            {
                if (m_Depressed == value)
                    return;
                m_Depressed = value;
                Redraw();
            }
        }

        /// <summary>
        /// Indicates whether the button is toggleable.
        /// </summary>
        public bool IsToggle { get; set; }

        /// <summary>
        /// Determines the button's toggle state.
        /// </summary>
        public bool ToggleState
        {
            get => m_ToggleStatus;
            set
            {
                if (!IsToggle)
                    return;
                if (m_ToggleStatus == value)
                    return;

                m_ToggleStatus = value;

                Toggled?.Invoke(this, EventArgs.Empty);

                if (m_ToggleStatus)
                {
                    ToggledOn?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ToggledOff?.Invoke(this, EventArgs.Empty);
                }

                Redraw();
            }
        }
        /// <summary>
        /// Font.
        /// </summary>
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                SetupDefault();
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Control constructor.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Button(ControlBase parent)
            : base(parent)
        {
            AutoSizeToContents = true;
            SetupDefault();
            MouseInputEnabled = true;
            Alignment = Pos.Center;
        }
        protected virtual void SetupDefault()
        {
            int extra = TextHeight / 3;
            Padding textpadding = new Padding(extra, extra, extra, extra);
            if (TextPadding != textpadding)
            {
                TextPadding = textpadding;
                if (AutoSizeToContents)
                {
                    if (GetSizeToFitContents().Height > Height)
                        _ = SizeToChildren(false, true);
                }
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// "Clicks" the button.
        /// </summary>
        public virtual void Press(ControlBase control = null) => OnClicked(0, 0);

        /// <summary>
        /// Sets the button's image.
        /// </summary>
        /// <param name="textureName">Texture name. Null to remove.</param>
        /// <param name="center">Determines whether the image should be centered.</param>
        public virtual void SetImage(string textureName, bool center = false)
        {
            if (string.IsNullOrEmpty(textureName))
            {
                m_Image?.Dispose();
                m_Image = null;
                return;
            }

            if (m_Image == null)
            {
                m_Image = new ImagePanel(this);
            }

            m_Image.ImageName = textureName;
            m_Image.SizeToContents();
            m_Image.SetPosition(Math.Max(Padding.Left, 2), 2);
            m_Image.Margin = new Margin(0, 2, 0, 2);
            m_CenterImage = center;

            TextPadding = new Padding(m_Image.Right + m_Image.Margin.Right + 2, TextPadding.Top, TextPadding.Right, TextPadding.Bottom);
        }

        /// <summary>
        /// Toggles the button.
        /// </summary>
        public virtual void Toggle() => ToggleState = !ToggleState;

        protected override void ProcessLayout(Size size)
        {
            m_Image?.AlignToEdge(m_CenterImage ? Pos.Center : Pos.CenterV);
            base.ProcessLayout(size);
        }

        /// <summary>
        /// Default accelerator handler.
        /// </summary>
        protected override void OnAccelerator() => OnClicked(0, 0);

        /// <summary>
        /// Internal OnPressed implementation.
        /// </summary>
        protected virtual void OnClicked(int x, int y)
        {
            if (IsToggle)
            {
                Toggle();
            }

            base.OnMouseClickedLeft(x, y, true);
        }

        /// <summary>
        /// Handler for Space keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeySpace(bool down) => base.OnKeySpace(down);

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
            //base.OnMouseClickedLeft(x, y, down);
            if (down)
            {
                IsDepressed = true;
                InputHandler.MouseFocus = this;
                Pressed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (GetCanvas().GetControlAt(x, y) == this && IsHovered && m_Depressed)
                {
                    OnClicked(x, y);
                }

                IsDepressed = false;
                InputHandler.MouseFocus = null;
                Released?.Invoke(this, EventArgs.Empty);
            }

            Redraw();
        }

        /// <summary>
        /// Handler invoked on mouse double click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        protected override void OnMouseDoubleClickedLeft(int x, int y)
        {
            base.OnMouseDoubleClickedLeft(x, y);
            OnMouseClickedLeft(x, y, true);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            base.Render(skin);

            if (ShouldDrawBackground)
            {
                bool drawDepressed = IsDepressed && IsHovered;
                if (IsToggle)
                    drawDepressed = drawDepressed || ToggleState;

                bool bDrawHovered = IsHovered && ShouldDrawHover;

                skin.DrawButton(this, drawDepressed, bDrawHovered, IsDisabled);
            }
        }

        #endregion Methods

        #region Fields

        private bool m_CenterImage;
        private bool m_Depressed;
        private ImagePanel m_Image;
        private bool m_ToggleStatus;

        #endregion Fields
    }
}