using Gwen.ControlInternal;

namespace Gwen.Controls
{
    /// <summary>
    /// Static text label.
    /// </summary>
    public class Label : ControlBase
    {
        #region Fields

        protected readonly Text m_Text;
        private Color m_TextColor;
        private Color m_textColorOverride;
        private Pos m_Align;
        private Padding m_TextPadding;

        #endregion Fields

        #region Properties

        public override bool MouseInputEnabled
        {
            get => base.MouseInputEnabled || ClickEventAssigned;
            set => base.MouseInputEnabled = value;
        }
        /// <summary>
        /// Text alignment.
        /// </summary>
        public Pos Alignment
        {
            get => m_Align;
            set
            {
                m_Align = value;
                Invalidate();
            }
        }
        /// <summary>
        /// Font.
        /// </summary>
        public virtual Font Font
        {
            get => m_Text.Font;
            set
            {
                m_Text.Font = value;
                Invalidate();
                InvalidateParent();
            }
        }

        /// <summary>
        /// Text.
        /// </summary>
        public virtual string Text { get => m_Text.String; set => SetText(value); }

        /// <summary>
        /// Text color.
        /// </summary>
        public Color TextColor
        {
            get => m_TextColor;
            set
            {
                if (m_TextColor != value)
                {
                    m_TextColor = value;
                    Redraw();
                }
            }
        }

        /// <summary>
        /// Height of the text (in pixels).
        /// </summary>
        public int TextHeight => m_Text.Height;

        /// <summary>
        /// Text length (in characters).
        /// </summary>
        public int TextLength => m_Text.Length;

        /// <summary>
        /// Text color override - used by tooltips.
        /// </summary>
        public Color TextColorOverride
        {
            get => m_textColorOverride;

            set
            {
                m_textColorOverride = value;
                if (IsTextOverrideVisible)
                {
                    Redraw();
                }
            }
        }

        private string m_TextOverride;

        /// <summary>
        /// Text override - used to display different string.
        /// </summary>
        public string TextOverride
        {
            get => m_TextOverride;

            set
            {
                m_TextOverride = value;
                if (IsTextOverrideVisible)
                {
                    Redraw();
                }
            }
        }

        /// <summary>
        /// Text padding.
        /// </summary>
        internal Padding TextPadding
        {
            get => m_TextPadding;
            set
            {
                m_TextPadding = value;
                Invalidate();
                InvalidateParent();
            }
        }

        public int TextRight => m_Text.Right + m_Text.Margin.Right;

        /// <summary>
        /// Width of the text (in pixels).
        /// </summary>
        public int TextWidth => m_Text.Width;

        public int TextX => m_Text.X;
        public int TextY => m_Text.Y;
        public bool IsTextOverrideVisible => TextColorOverride.A != 0;
        protected virtual Color CurrentColor => IsDisabled
                    ? Skin.Colors.Text.Disabled
                    : IsHovered && ClickEventAssigned ? Skin.Colors.Text.Contrast : IsTextOverrideVisible ? TextColorOverride : TextColor;

        /// <summary>
        /// Function to define the label text before render.
        /// </summary>
        public TextRequestHandler TextRequest = null;
        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Label(ControlBase parent) : base(parent)
        {
            SuspendLayout();
            m_Text = new Text(this);
            _ = SetSize(100, m_Text.Height);
            Alignment = Pos.Left | Pos.Top;

            AutoSizeToContents = true;
            base.MouseInputEnabled = false;
            TextColor = Skin.Colors.Foreground;
            TextColorOverride = Color.FromArgb(0, 255, 255, 255);// A==0, override disabled
            BoundsOutlineColor = Color.LawnGreen;
            ResumeLayout(true);
        }

        #endregion Constructors

        #region Methods
        public override void Think()
        {
            if (TextRequest != null)
            {
                Text = TextRequest(this, Text);
            }
            base.Think();
        }
        /// <summary>
        /// Gets the coordinates of specified character.
        /// </summary>
        /// <param name="index">Character index.</param>
        /// <returns>Character coordinates (local).</returns>
        public virtual Point GetCharacterPosition(int index)
        {
            Point p = m_Text.GetCharacterPosition(index);
            return new Point(p.X + m_Text.X, p.Y + m_Text.Y);
        }

        /// <summary>
        /// Sets the label text.
        /// </summary>
        /// <param name="str">Text to set.</param>
        /// <param name="doEvents">Determines whether to invoke "text changed" event.</param>
        public virtual void SetText(string str, bool doEvents = true)
        {
            if (Text == str)
                return;

            m_Text.String = str;
            Invalidate();

            if (doEvents)
                OnTextChanged();
        }
        #endregion Methods

        /// <summary>
        /// Returns index of the character closest to specified point (in canvas coordinates).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected virtual Point GetClosestCharacter(int x, int y) => new(m_Text.GetClosestCharacter(m_Text.CanvasPosToLocal(new Point(x, y))), 0);

        protected override void ProcessLayout(Size size)
        {
            base.ProcessLayout(size);
            m_Text.AlignToEdge(m_Align, TextPadding, 0, 0);
        }

        /// <summary>
        /// Handler for text changed event.
        /// </summary>
        protected virtual void OnTextChanged() { }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            m_Text.TextColor = CurrentColor;
            base.Render(skin);
        }
        public override Size GetSizeToFitContents()
        {
            Size sz = m_Text.GetSizeToFitContents() + Padding.Size + TextPadding.Size;
            return sz;
        }

        /// <summary>
        /// Sets the position of the internal text control.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        protected void SetTextPosition(int x, int y) => m_Text.SetPosition(x, y);
    }
}