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
using Gwen;
using Gwen.Controls;
using linerider.LRL;
using linerider.UI.Components;
using linerider.Utils;

namespace linerider.UI
{
    public class InfoBarCoords : WidgetContainer
    {
        private GameCanvas _canvas;
        private TrackLabel _ridercoordlabel;

        public InfoBarCoords(ControlBase parent) : base(parent)
        {
            _canvas = (GameCanvas)parent.GetCanvas();
            Setup();
        }
        private void Setup()
        {
            _ridercoordlabel = new TrackLabel(this)
            {
                Dock = Dock.Top,
                TextRequest = (o, e) =>
                {
                    string coords = string.Join("\n", Coordinates.CoordsData);
                    return coords;
                },
            };
        }
    }
}