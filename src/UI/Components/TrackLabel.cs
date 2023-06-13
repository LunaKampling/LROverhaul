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

using System.Drawing;
using Gwen.Controls;
using linerider.Utils;

namespace linerider.UI.Components
{
    /// <summary>
    /// Just a label that is intend to go on top of the track
    /// Its color inherits line color to stay visible on any background
    /// </summary>
    public class TrackLabel : Label
    {
        protected override Color CurrentColor
        {
            get
            {
                bool recording = Settings.PreviewMode || IO.TrackRecorder.Recording;
                return recording ? Constants.ExportLineColor : Settings.Computed.LineColor;
            }
        }
        public TrackLabel(ControlBase parent) : base(parent)
        { }
    }
}