﻿namespace Gwen.Controls
{
    /// <summary>
    /// Progress bar.
    /// </summary>
    public class ProgressBar : ControlBase
    {

        private float m_Progress;

        /// <summary>
        /// Progress value (0-1).
        /// </summary>
        public float Value
        {
            get => m_Progress;
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 1)
                    value = 1;

                m_Progress = value;
            }
        }
        /// <summary>
        /// Determines whether the control is horizontal.
        /// </summary>
        public bool IsHorizontal { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressBar"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ProgressBar(ControlBase parent)
            : base(parent)
        {
            AutoSizeToContents = false;
            Height = 15;
            Width = 100;

            m_Progress = 0;
            IsHorizontal = true;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin) => skin.DrawProgressBar(this, IsHorizontal, m_Progress);
    }
}
