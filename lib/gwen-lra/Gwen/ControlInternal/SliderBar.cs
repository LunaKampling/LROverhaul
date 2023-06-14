﻿namespace Gwen.ControlInternal
{
    /// <summary>
    /// Slider bar.
    /// </summary>
    public class SliderBar : Dragger
    {
        #region Properties

        /// <summary>
        /// Indicates whether the bar is horizontal.
        /// </summary>
        public bool IsHorizontal { get; set; }
        public override bool ToolTipProvider => false;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderBar"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public SliderBar(Controls.ControlBase parent)
            : base(parent)
        {
            Target = this;
            RestrictToParent = true;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawSliderButton(this, IsHeld, IsHorizontal);

        #endregion Methods

        #region Fields

        #endregion Fields
    }
}