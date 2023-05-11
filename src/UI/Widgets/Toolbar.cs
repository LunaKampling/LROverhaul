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
using linerider.Tools;
using System.Windows.Forms;
using System.Diagnostics;

namespace linerider.UI
{
    public class Toolbar : WidgetContainer
    {
        public static bool smPencil = false;

        public static ImageButton _pencilbtn;
        public static ImageButton _smpenbtn;
        private ImageButton _linebtn;
        private ImageButton _bezierbtn;
        private ImageButton _eraserbtn;
        private ImageButton _selectbtn;
        private ImageButton _handbtn;
        private ImageButton _start;
        private ImageButton _pause;
        private ImageButton _stop;
        private ImageButton _flag;
        private ImageButton _generator;
        private ImageButton _menu;
        private ColorSwatch _swatch;
        private ControlBase _buttoncontainer;
        private Editor _editor;
        private GameCanvas _canvas;
        public Toolbar(ControlBase parent, Editor editor) : base(parent)
        {
            _canvas = (GameCanvas)parent.GetCanvas();
            MouseInputEnabled = false;
            AutoSizeToContents = true;
            ShouldDrawBackground = true;
            _editor = editor;
            MakeButtons();
            SetupEvents();
            OnThink += Think;
        }
        private void Think(object sender, EventArgs e)
        {
            var rec = Settings.Local.RecordingMode;
            _swatch.IsHidden = rec;
        }
        private void MakeButtons()
        {
            _buttoncontainer = new ControlBase(this)
            {
                Dock = Dock.Top,
                AutoSizeToContents = true,
            };
            _swatch = new ColorSwatch(this);
            _swatch.Dock = Dock.Top;
            _pencilbtn = CreateTool(GameResources.pencil_icon, "Pencil Tool (Q)\nRe-press to swap to Smooth Pencil");
            _smpenbtn = CreateTool(GameResources.smoothpencil_icon, "Smooth Pencil Tool (Q)\nRe-press to swap to Pencil");
            _linebtn = CreateTool(GameResources.line_icon, "Line Tool (W)");
            _bezierbtn = CreateTool(GameResources.bezier_icon, "Bezier Tool");
            _eraserbtn = CreateTool(GameResources.eraser_icon, "Eraser Tool (E)");
            _selectbtn = CreateTool(GameResources.movetool_icon, "Select Tool (R)");
            _handbtn = CreateTool(GameResources.pantool_icon, "Hand Tool (Shift+Space) (T)");
            _start = CreateTool(GameResources.play_icon, "Start (Space) (Y)");
            _pause = CreateTool(GameResources.pause, "Pause (Space)");
            _stop = CreateTool(GameResources.stop_icon, "Stop (U)");
            _flag = CreateTool(GameResources.flag_icon, "Flag (I)");
            _generator = CreateTool(GameResources.generator_icon, "Generator (G)");
            _menu = CreateTool(GameResources.menu_icon, "Options");

            _smpenbtn.IsHidden = true;
        }
        private void SetupEvents()
        {
            _pencilbtn.Clicked += (o, e) =>
            {
                if (CurrentTools._selected != CurrentTools.PencilTool)
                {
                    CurrentTools.SetTool(CurrentTools.PencilTool);
                }
                else
                {
                    CurrentTools.SetTool(CurrentTools.SmoothPencilTool);
                    _smpenbtn.IsHidden = false;
                    _pencilbtn.IsHidden = true;
                    smPencil = true;
                }
            };
            _smpenbtn.Clicked += (o, e) =>
            {
                if (CurrentTools._selected != CurrentTools.SmoothPencilTool)
                {
                    CurrentTools.SetTool(CurrentTools.SmoothPencilTool);
                }
                else
                {
                    CurrentTools.SetTool(CurrentTools.PencilTool);
                    _smpenbtn.IsHidden = true;
                    _pencilbtn.IsHidden = false;
                    smPencil = false;
                }
            };
            _linebtn.Clicked += (o, e) => CurrentTools.SetTool(CurrentTools.LineTool);
            _eraserbtn.Clicked += (o, e) => CurrentTools.SetTool(CurrentTools.EraserTool);
            _selectbtn.Clicked += (o, e) => CurrentTools.SetTool(CurrentTools.MoveTool);
            _handbtn.Clicked += (o, e) => CurrentTools.SetTool(CurrentTools.HandTool);
            _bezierbtn.Clicked += (o, e) => CurrentTools.SetTool(CurrentTools.BezierTool);
            _flag.Clicked += (o, e) =>
            {
                _editor.Flag(_editor.Offset);
            };
            // _pause.IsHidden = true;
            _start.Clicked += (o, e) =>
            {
                CurrentTools.SelectedTool.Stop();
                if (UI.InputUtils.Check(Hotkey.PlayButtonIgnoreFlag))
                {
                    _editor.StartIgnoreFlag();
                    _editor.Scheduler.DefaultSpeed();
                }
                else
                {
                    if (_editor.Paused)
                    {
                        _editor.TogglePause();
                    }
                    else
                    {
                        _editor.StartFromFlag();
                        _editor.Scheduler.DefaultSpeed();
                    }
                }
                _pause.IsHidden = false;
                _start.IsHidden = true;
            };
            _stop.Clicked += (o, e) =>
            {
                CurrentTools.SelectedTool.Stop();
                _editor.Stop();
            };
            _pause.Clicked += (o, e) =>
            {
                CurrentTools.SelectedTool.Stop();
                _editor.TogglePause();
                _pause.IsHidden = true;
                _start.IsHidden = false;
            };
            _generator.Clicked += (o, e) =>
            {
                _canvas.ShowGeneratorWindow(new OpenTK.Vector2d(e.X, e.Y));
            };
            _menu.Clicked += (o, e) =>
            {
                Gwen.Controls.Menu menu = new Gwen.Controls.Menu(_canvas);
                menu.AddItem("Save As...").Clicked += (o2, e2) => { _canvas.ShowSaveDialog(); };
                menu.AddItem("Load").Clicked += (o2, e2) => { _canvas.ShowLoadDialog(); };
                menu.AddItem("New").Clicked += (o2, e2) =>
                {
                    if (_editor.TrackChanges != 0)
                    {
                        var save = Gwen.Controls.MessageBox.Show(
                            _canvas,
                            "You are creating a new track.\nDo you want to save your current changes?",
                            "Create New Track",
                            Gwen.Controls.MessageBox.ButtonType.YesNoCancel);
                        save.RenameButtonsYN("Save", "Discard", "Cancel");
                        save.Dismissed += (o3, result) =>
                          {
                              switch (result)
                              {
                                  case Gwen.DialogResult.Yes:
                                      _canvas.ShowSaveDialog();
                                      break;
                                  case Gwen.DialogResult.No:
                                      NewTrack();
                                      break;
                              }
                          };
                    }
                    else
                    {
                        var msg = Gwen.Controls.MessageBox.Show(
                            _canvas,
                            "Are you sure you want to create a new track?",
                            "Create New Track",
                            Gwen.Controls.MessageBox.ButtonType.OkCancel);
                        msg.RenameButtons("Create");
                        msg.Dismissed += (o3, result) =>
                          {
                              if (result == Gwen.DialogResult.OK)
                              {
                                  NewTrack();
                              }
                          };
                    }
                };
                menu.AddItem("Preferences").Clicked += (o2, e2) => _canvas.ShowPreferencesDialog();
                menu.AddItem("Track Properties").Clicked += (o2, e2) => _canvas.ShowTrackPropertiesDialog();
                menu.AddItem("Triggers").Clicked += (o2, e2) => _canvas.ShowTriggerWindow();
                menu.AddItem("Export Video").Clicked += (o2, e2) => _canvas.ShowExportVideoWindow();
                menu.AddItem("Capture Screen").Clicked += (o2, e2) => _canvas.ShowScreenCaptureWindow();
                var canvaspos = LocalPosToCanvas(new Point(_menu.X, _menu.Y));
                menu.SetPosition(canvaspos.X, canvaspos.Y + 32);
                menu.Show();
            };
        }
        private void NewTrack()
        {
            _editor.Stop();
            _editor.ChangeTrack(new Track() { Name = Utils.Constants.DefaultTrackName });
            Settings.LastSelectedTrack = "";
            Settings.Save();
            _editor.Invalidate();
        }
        protected override void PostLayout()
        {
            var w = Width;
            if (Parent.Width > w)
            {
                X = (Parent.Width / 2) - w / 2;
            }
            else
            {
                X = 0;
            }
            base.PostLayout();
        }
        public override void Think()
        {
            bool showplay = !_editor.Playing;
            _pause.IsHidden = showplay;
            _start.IsHidden = !showplay;
            base.Think();
        }
        private ImageButton CreateTool(Bitmap image, string tooltip)
        {
            var size = (Screen.PrimaryScreen.Bounds.Width / 1600 < Screen.PrimaryScreen.Bounds.Height / 1080) ? (Screen.PrimaryScreen.Bounds.Width / 1600) : (Screen.PrimaryScreen.Bounds.Height / 1080);
            if (size < 1) { size = 1; };
            ImageButton btn = new ImageButton(_buttoncontainer);
            btn.Dock = Dock.Left;
            btn.SetImage(image);
            btn.SetSize(size*32, size*32);
            btn.Tooltip = tooltip;
            return btn;
        }
    }
}