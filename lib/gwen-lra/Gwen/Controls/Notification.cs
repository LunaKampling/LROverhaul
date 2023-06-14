namespace Gwen.Controls
{
    /// <summary>
    /// Notification (container).
    /// </summary>
    public class Notification : Container
    {
        protected override Margin PanelMargin => Margin.Three;
        private readonly ControlBase m_TitleBar;

        /// <summary>
        /// Determines whether the notification should be disposed on close.
        /// </summary>
        public bool DeleteOnClose { get; set; } = true;
        private readonly RichLabel _label;
        private readonly Label _title;
        // public string Text
        // {
        //     get
        //     {
        //         return _label.Text;
        //     }
        //     set
        //     {
        //         _label.Text = value;
        //     }
        // }
        // public string TitleText
        // {
        //     get
        //     {
        //         return _label.Text;
        //     }
        //     set
        //     {
        //         _label.Text = value;
        //     }
        // }
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Notification(ControlBase parent) : base(parent)
        {
            Dock = Dock.Bottom;
            Invalidate();
            IsHidden = true;
            _ = SetSize(200, 100);

            m_TitleBar = new ControlBase(null)
            {
                Height = 24,
                Dock = Dock.Top,
                Margin = new Margin(0, 0, 0, 6),
                MouseInputEnabled = false

            };
            PrivateChildren.Add(m_TitleBar);
            _title = new Label(m_TitleBar)
            {
                Dock = Dock.Left,
                Text = "Notification",
                Margin = new Margin(6, 3, 0, 0),
            };
            _label = new RichLabel(this) { Dock = Dock.Fill, AutoSizeToContents = true };
            //Margin = new Margin(5, 5, 5, 5);
            Clicked += (o, e) =>
            {
                CloseNotification();
            };
            m_Panel.Clicked += (o, e) =>
            {
                CloseNotification();
            };
        }
        public void CloseNotification()
        {
            IsHidden = true;
            if (DeleteOnClose)
            {
                Parent.RemoveChild(this, DeleteOnClose);
            }
        }
        public void Show(int ms)
        {
            IsHidden = false;
            Anim_HeightIn(ms / 1000f, 0, 0.25f);
        }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            float fade = 1;
            // if (_showdelay > 0)
            // {
            //     fade = (float)_timer.ElapsedMilliseconds / (float)_showdelay;
            //     if (fade > 1)
            //     {
            //         _timer.Stop();
            //         fade = 1;
            //     }
            // }
            if (ShouldDrawBackground)
                skin.DrawNotification(this, fade);
            //InvalidateParent();
        }

        #endregion Methods
    }
}