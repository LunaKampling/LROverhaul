//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using ExCSS;
using Gwen;
using Gwen.Controls;
using Gwen.Skin.Texturing;
using linerider.IO;
using linerider.Utils;
using System;

namespace linerider.UI.Components
{
    public class WidgetContainer : Panel
    {
        public static readonly int WidgetMargin = Utility.NumberToCurrentScale(3);
        public static readonly int WidgetPadding = Utility.NumberToCurrentScale(7);
        public static readonly int WidgetItemSpacing = Utility.NumberToCurrentScale(5);
        private readonly int BorderRadius = Utility.NumberToCurrentScale(10);

        private Bordered _image;
        private Color _bgcolor
        {
            get
            {
                bool recording = Settings.PreviewMode || TrackRecorder.Recording;
                return recording ? Constants.BgExportColor : Settings.Computed.BGColor;
            }
        }
        public WidgetContainer(ControlBase parent) : base(parent)
        {
            Padding = new Padding(WidgetPadding, WidgetPadding, WidgetPadding, WidgetPadding);
            ShouldDrawBackground = true;
            AutoSizeToContents = true;
            MouseInputEnabled = false;
            BackgroundAlpha = 128;

            _image = new RoundedSquareTexture(BorderRadius).Bordered;
        }
        protected override void Render(Gwen.Skin.SkinBase skin)
        {
            if (ShouldDrawBackground)
            {
                Color color = Color.FromArgb(BackgroundAlpha, _bgcolor);
                _image.Draw(skin.Renderer, RenderBounds, color);
            }
        }
    }
}