using Gwen.ControlInternal;
using System;

namespace Gwen.Controls
{
    /// <summary>
    /// Movable window with title bar.
    /// </summary>
    public class WindowControl : ResizableControl
    {
        private class TitleLabel : Label
        {
            private readonly WindowControl m_Window;
            protected override Color CurrentColor => m_Window.IsOnTop ? Skin.Colors.Text.AccentForeground : Skin.Colors.Text.Foreground;
            public TitleLabel(ControlBase parent, WindowControl window) : base(parent)
            {
                m_Window = window;
            }
        }
        #region Events

        /// <summary>
        /// Invoked when the text has changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> IsHiddenChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Determines whether the control should be disposed on close.
        /// </summary>
        public bool DeleteOnClose { get; set; }

        /// <summary>
        /// Determines whether the window has close button.
        /// </summary>
        public bool IsClosable { get => !m_CloseButton.IsHidden; set => m_CloseButton.IsHidden = !value; }

        /// <summary>
        /// Indicates whether the control is hidden.
        /// </summary>
        public override bool IsHidden
        {
            get => base.IsHidden;
            set
            {
                if (IsHidden == value)
                    return;
                base.IsHidden = value;
                if (m_Modal != null)
                {
                    m_Modal.IsHidden = value;
                }
                if (!value)
                {
                    BringToFront();
                    Focus();
                    if (m_Modal != null)
                    {
                        m_Modal.BringToFront();
                        m_Modal.Layout();
                    }
                }
                IsHiddenChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Indicates whether the control is on top of its parent's children.
        /// </summary>
        public override bool IsOnTop
        {
            get
            {
                if (Parent != null)
                {
                    for (int i = Parent.Children.Count - 1; i >= 0; i--)
                        if (Parent.Children[i] is WindowControl win)
                            return win == this;
                }
                return false;
            }
        }

        /// <summary>
        /// Window caption.
        /// </summary>
        public string Title { get => m_Title.Text; set => m_Title.Text = value; }

        protected override Margin PanelMargin => Margin.Two;
        public override bool AutoSizeToContents
        {
            get => base.AutoSizeToContents;
            set
            {
                if (AutoSizeToContents != value)
                {
                    base.AutoSizeToContents = value;
                    if (value)
                    {
                        DisableResizing();
                    }
                }
            }
        }
        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="caption">Window caption.</param>
        /// <param name="modal">Determines whether the window should be modal.</param>
        public WindowControl(ControlBase parent, string title = "")
            : base(parent)
        {
            m_TitleBar = new Dragger(null)
            {
                Height = 24,
                Target = this,
                Dock = Dock.Top,
                Margin = new Margin(0, 0, 0, 6)
            };
            PrivateChildren.Insert(0, m_TitleBar);

            m_Title = new TitleLabel(m_TitleBar, this)
            {
                Alignment = Pos.Left | Pos.Top,
                Text = title,
                Dock = Dock.Fill,
                TextPadding = new Padding(8, 4, 0, 0),
                TextColor = Skin.Colors.Text.AccentForeground,
            };

            m_CloseButton = new CloseButton(m_TitleBar, this)
            {
                Width = 24,
                Height = 24,
                Dock = Dock.Right,
                IsTabable = false,
                Name = "closeButton"
            };
            m_CloseButton.Clicked += CloseButtonPressed;
            // Create a blank content control, dock it to the top - Should this be a ScrollControl?
            //GetResizer(8).Hide();
            _resizer_top.Hide();
            IsTabable = false;
            MinimumSize = new Size(100, 50);
            ClampMovement = true;
            KeyboardInputEnabled = false;
            m_Panel.Dock = Dock.Fill;
            m_Panel.AutoSizeToContents = true;
            IsHidden = true;
            HideResizersOnDisable = false;
        }

        #endregion Constructors

        #region Methods
        public void ShowCentered()
        {
            if (IsHidden)
            {
                base.Show();
                Canvas canvas = GetCanvas();
                if (canvas != null)
                {
                    Layout();
                    SetPosition(canvas.Width / 2 - Width / 2, canvas.Height / 2 - Height / 2);
                }
            }
        }
        /// <summary>
        /// Closes the window. Returns true if this window was closed
        /// </summary>
        public virtual bool Close()
        {
            bool prev = IsHidden;
            CloseButtonPressed(this, EventArgs.Empty);
            return IsHidden && IsHidden != prev;
        }

        /// <summary>
        /// Makes the window modal: covers the whole canvas and gets all input.
        /// </summary>
        /// <param name="dim">Determines whether all the background should be dimmed.</param>
        public void MakeModal(bool dim = false)
        {
            if (m_Modal != null)
                return;

            m_Modal = new Modal(GetCanvas());
            Parent = m_Modal;

            m_Modal.ShouldDrawBackground = dim;
            m_Modal.IsHidden = IsHidden;
        }

        public override void Touch()
        {
            base.Touch();
            BringToFront();
        }

        protected virtual void CloseButtonPressed(ControlBase control, EventArgs args)
        {
            IsHidden = true;

            if (m_Modal != null)
            {
                m_Modal.DelayedDelete();
                m_Modal = null;
            }

            if (DeleteOnClose && Parent != null)
            {
                Parent.RemoveChild(this, true);
            }
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            bool hasFocus = IsOnTop;

            skin.DrawWindow(this, m_TitleBar.Bottom + m_Title.Margin.Bottom, hasFocus);
        }

        /// <summary>
        /// Renders the focus overlay.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void RenderFocus(Skin.SkinBase skin)
        {
        }

        /// <summary>
        /// Renders under the actual control (shadows etc).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void RenderUnder(Skin.SkinBase skin)
        {
            base.RenderUnder(skin);
            skin.DrawShadow(this);
        }

        #endregion Methods

        #region Fields

        private readonly CloseButton m_CloseButton;
        private readonly Label m_Title;
        private readonly Dragger m_TitleBar;
        private Modal m_Modal;

        #endregion Fields
    }
}