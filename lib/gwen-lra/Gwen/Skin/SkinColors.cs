using System;
using System.Drawing;

namespace Gwen.Skin
{
    /// <summary>
    /// UI colors used by skins.
    /// </summary>
    public struct SkinColors
    {
        /// Skin color principals
        /// Background is for empty content, or content that is bordered and does not want to stand out
        /// BackgroundHighlight is for panels, containers. It might also be used as a highlight color for bordered controls.
        /// Foreground is used for borders
        /// Accent is for immediate attention?, or focus borders
        /// 
        public struct _Text
        {
            public Color Foreground;
            /// <summary>
            /// For an appropriately contrasted color for going over the accent color
            /// </summary>
            public Color AccentForeground;
            public Color Contrast;
            public Color ContrastLow;
            public Color Highlight;
            public Color Inactive;
            public Color Disabled;
        }

        public Color ModalBackground;
        public _Text Text;
        public Color Accent;
        public Color AccentHigh;
        public Color AccentLow;
        public Color Background;
        public Color BackgroundHighlight;
        public Color Foreground;
        public Color ForegroundInactive;
        public Color ForegroundHighlight;
    }
}
