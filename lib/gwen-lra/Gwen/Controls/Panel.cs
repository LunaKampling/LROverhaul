using System;
using System.Drawing;

namespace Gwen.Controls
{
    /// <summary>
    /// Panel (container).
    /// </summary>
    public class Panel : ControlBase
    {
        private byte _backgroundalpha = 255;
        public byte BackgroundAlpha
        {
            get
            {
                return _backgroundalpha;
            }
            set
            {
                if (_backgroundalpha != value)
                {
                    _backgroundalpha = value;
                    Redraw();
                }
            }
        }

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Panel(ControlBase parent) : base(parent)
        {
            //Padding = Padding.Three;
            Invalidate();
            //Margin = new Margin(5, 5, 5, 5);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            if (ShouldDrawBackground)
                skin.DrawPanel(this, BackgroundAlpha);
        }

        #endregion Methods
    }
}