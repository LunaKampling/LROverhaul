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
using System;
using System.Diagnostics;
using System.Drawing;
using Gwen;
using Gwen.Controls;
using linerider.IO;
using linerider.Tools;
using linerider.LRL;

namespace linerider.UI
{
    public class TrackInfoBar : WidgetContainer
    {
        private Editor _editor;
        private TrackLabel _title;
        private TrackLabel _linecount;
        private TrackLabel _selectioncount;
        private TrackLabel _ridercoordlabel;
        private GameCanvas _canvas;
        public TrackInfoBar(ControlBase parent, Editor editor) : base(parent)
        {
            _canvas = (GameCanvas)parent.GetCanvas();
            Dock = Dock.Left;
            _editor = editor;
            AutoSizeToContents = true;
            Setup();
            OnThink += Think;
        }
        private void Think(object sender, EventArgs e)
        {
            var rec = Settings.Local.RecordingMode;
            _title.IsHidden = rec;
            _linecount.IsHidden = rec;
            _ridercoordlabel.IsHidden = rec || !Settings.Editor.ShowCoordinateMenu;
        }
        private void Setup()
        {
            _title = new TrackLabel(this)
            {
                Dock = Dock.Top,
                TextRequest = (o, current) =>
                {
                    return GetTitle();
                }
            };
            _linecount = new TrackLabel(this)
            {
                Dock = Dock.Top,
                Margin = new Margin(0, 5, 0, 0),
                TextRequest = (o, current) =>
                {
                    var u = (int)_linecount.UserData;
                    if (u != _editor.LineCount)
                    {
                        _linecount.UserData = _editor.LineCount;
                        var lineCountText = "Lines: " + _editor.LineCount.ToString();
                        return lineCountText;
                    }
                    return current;
                },
                UserData = 0,
                Text = "Lines: 0",
            };
            _selectioncount = new TrackLabel(this)
            {
                Dock = Dock.Top,
                Margin = new Margin(0, 5, 0, 0),
                TextRequest = (o, current) =>
                {
                    if (!_editor.Paused || TrackRecorder.Recording)
                    {
                        return "";
                    }
                    var u = (int)_selectioncount.UserData;
                    if (CurrentTools.SelectedTool == CurrentTools.SelectTool)
                    {
                        var linecount = ((SelectTool)(CurrentTools.SelectedTool)).GetLineSelectionsInBox().Count;
                        if (linecount == 0) { linecount = ((SelectTool)(CurrentTools.SelectedTool)).GetLineSelections().Count; }

                        if (u != linecount)
                        {
                            if (linecount > 0)
                            {
                                _selectioncount.UserData = linecount;
                                return "Selected: " + linecount.ToString();
                            }
                            return "";
                        }
                    }
                    return current;
                },
                UserData = 0,
            };
            _ridercoordlabel = new TrackLabel(this)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, 5, 0, 0),
                TextRequest = (o, e) =>
                {
                    var x = "";
                    for (var i = 0; i < Coordinates.CoordsData.Length; i++)
                    {
                        x += Coordinates.CoordsData[i] + "\n";
                    }
                    return x;
                },
                UserData = 0.0,
            };
        }
        private ImageButton CreateButton(Bitmap image, string tooltip)
        {
            ImageButton btn = new ImageButton(this);
            btn.SetImage(image);
            btn.SetSize(32, 32);
            btn.Tooltip = tooltip;
            return btn;
        }
        private string GetTitle()
        {
            string name = _editor.Name;
            var changes = _editor.TrackChanges;
            if (changes > 0)
            {
                name += " (*)";
                name += changes == 1 ? "\n" + (changes) + " change" : "\n" + (changes) + " changes";
            }
            if (changes > Settings.autosaveChanges) {
                name += "\nAutosave enabled!";
            }
            return name;
        }
    }
}