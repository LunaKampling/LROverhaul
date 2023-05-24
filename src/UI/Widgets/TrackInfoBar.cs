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
        private GameCanvas _canvas;
        
        private TrackLabel _title;
        private TrackLabel _autosavelabel;
        private TrackLabel _changedlines;
        private TrackLabel _linecount;
        private TrackLabel _selectioncount;
        private TrackLabel _ridercoordlabel;

        public TrackInfoBar(ControlBase parent, Editor editor) : base(parent)
        {
            _canvas = (GameCanvas)parent.GetCanvas();
            Dock = Dock.Left;
            _editor = editor;
            AutoSizeToContents = true;
            Setup();
            OnThink += Think;
            Padding = Padding.Five;
            ShouldDrawBackground = false;
        }
        private void Think(object sender, EventArgs e)
        {
            bool rec = Settings.Local.RecordingMode;
            int changes = _editor.TrackChanges;

            _title.IsHidden = rec;
            _autosavelabel.IsHidden = rec || changes <= Settings.autosaveChanges;
            _changedlines.IsHidden = rec || changes == 0;
            _linecount.IsHidden = rec;
            _selectioncount.IsHidden = rec || GetSelectedLinesCount() == 0;
            _ridercoordlabel.IsHidden = rec || !Settings.Editor.ShowCoordinateMenu;
        }
        private void Setup()
        {
            Margin = new Margin(_canvas.ScreenEdgeSpacing, _canvas.ScreenEdgeSpacing, 0, 0);

            _title = new TrackLabel(this)
            {
                Dock = Dock.Top,
                TextRequest = (o, current) =>
                {
                    bool hasChanges = _editor.TrackChanges > 0;
                    string name = hasChanges ? $"{_editor.Name} *" : _editor.Name;
                    return name;
                },
            };

            _autosavelabel = new TrackLabel(this)
            {
                Dock = Dock.Top,
                Margin = new Margin(0, _canvas.WidgetSpacing, 0, 0),
                IsHidden = true,
                Text = "Autosave enabled!",
            };

            _changedlines = new TrackLabel(this)
            {
                Dock = Dock.Top,
                Margin = new Margin(0, _canvas.WidgetSpacing, 0, 0),
                IsHidden = true,
                TextRequest = (o, current) =>
                {
                    int changes = _editor.TrackChanges;
                    string text = changes == 1 ? $"{changes} change" : $"{changes} changes";

                    return text;
                },
                UserData = 0,
            };

            _linecount = new TrackLabel(this)
            {
                Dock = Dock.Top,
                Margin = new Margin(0, _canvas.WidgetSpacing, 0, 0),
                TextRequest = (o, current) =>
                {
                    int u = (int)_linecount.UserData;
                    if (u != _editor.LineCount)
                    {
                        _linecount.UserData = _editor.LineCount;
                        string lineCountText = "Lines: " + _editor.LineCount.ToString();
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
                Margin = new Margin(0, _canvas.WidgetSpacing, 0, 0),
                TextRequest = (o, current) =>
                {
                    if (!_editor.Paused || TrackRecorder.Recording)
                        return "";

                    int linecount = GetSelectedLinesCount();
                    return $"Selected: {linecount}";
                },
            };

            _ridercoordlabel = new TrackLabel(this)
            {
                Dock = Dock.Left,
                Margin = new Margin(0, _canvas.WidgetSpacing * 2, 0, 0),
                TextRequest = (o, e) =>
                {
                    string x = "";
                    for (int i = 0; i < Coordinates.CoordsData.Length; i++)
                    {
                        x += Coordinates.CoordsData[i] + "\n";
                    }
                    return x;
                },
                UserData = 0.0,
            };
        }
        private int GetSelectedLinesCount()
        {
            if (CurrentTools.SelectedTool == CurrentTools.SelectTool)
            {
                SelectTool selectTool = (SelectTool)CurrentTools.SelectedTool;
                int linecount = selectTool.GetLineSelectionsInBox().Count;
                if (linecount == 0)
                    linecount = selectTool.GetLineSelections().Count;

                return linecount;
            }
            return 0;
        }
    }
}