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
using linerider.UI.Components;

namespace linerider.UI
{
    public class Toolbar : WidgetContainer
    {
        public static bool smPencilTool = false;
        public static bool bezierTool = false;

        public static ImageButton _pencilbtn;
        public static ImageButton _smpenbtn;
        public static ImageButton _linebtn;
        public static ImageButton _bezierbtn;
        private ImageButton _eraserbtn;
        private ImageButton _selectbtn;
        private ImageButton _handbtn;
        private ImageButton _startbtn;
        private ImageButton _pausebtn;
        private ImageButton _stopbtn;
        private ImageButton _flagbtn;
        private ImageButton _generatorbtn;
        private ImageButton _menubtn;
        private ColorSwatch _swatch;
        private ControlBase _buttoncontainer;
        private Menu _menu;
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
            MakeMenu();
            SetupEvents();
            OnThink += Think;
            Padding = new Padding(_canvas.InnerSpacing, _canvas.InnerSpacing, _canvas.InnerSpacing, _canvas.InnerSpacing);
            Y = _canvas.EdgesSpacing;
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
            _swatch.Dock = Dock.Left;
            _swatch.Padding = new Padding(0, _canvas.InnerSpacing, 0, 0);
            _pencilbtn = CreateTool(GameResources.pencil_icon, "Pencil / Smooth Pencil Tool", Hotkey.EditorPencilTool);
            _smpenbtn = CreateTool(GameResources.smoothpencil_icon, "Smooth Pencil / Pencil Tool", Hotkey.EditorPencilTool);
            _linebtn = CreateTool(GameResources.line_icon, "Line / Bezier Tool", Hotkey.EditorLineTool);
            _bezierbtn = CreateTool(GameResources.bezier_icon, "Bezier / Line Tool", Hotkey.EditorLineTool);
            _eraserbtn = CreateTool(GameResources.eraser_icon, "Eraser Tool", Hotkey.EditorEraserTool);
            _selectbtn = CreateTool(GameResources.movetool_icon, "Select Tool", Hotkey.EditorSelectTool);
            _handbtn = CreateTool(GameResources.pantool_icon, "Hand Tool", Hotkey.EditorPanTool);
            _startbtn = CreateTool(GameResources.play_icon, "Start", Hotkey.PlaybackTogglePause);
            _pausebtn = CreateTool(GameResources.pause, "Pause", Hotkey.PlaybackTogglePause);
            _stopbtn = CreateTool(GameResources.stop_icon, "Stop", Hotkey.PlaybackStop);
            _flagbtn = CreateTool(GameResources.flag_icon, "Flag", Hotkey.PlaybackFlag);
            _generatorbtn = CreateTool(GameResources.generator_icon, "Generators", Hotkey.LineGeneratorWindow);
            _menubtn = CreateTool(GameResources.menu_icon, "Options", Hotkey.None);

            _smpenbtn.IsHidden = true;
            _bezierbtn.IsHidden = true;
        }
        private void MakeMenu()
        {
            _menu = new Menu(_canvas)
            {
                IsHidden = true,
            };

            MenuItem saveAsItem = AddMenuItem("Save As...", Hotkey.SaveAsWindow);
            MenuItem loadItem = AddMenuItem("Load...", Hotkey.LoadWindow);
            MenuItem newItem = AddMenuItem("New");
            _menu.AddDivider();
            MenuItem preferencesItem = AddMenuItem("Preferences", Hotkey.PreferencesWindow);
            MenuItem userDirItem = AddMenuItem("Open User Directory");
            _menu.AddDivider();
            MenuItem trackPropsItem = AddMenuItem("Track Properties", Hotkey.TrackPropertiesWindow);
            MenuItem triggersItem = AddMenuItem("Triggers", Hotkey.TriggerMenuWindow);
            _menu.AddDivider();
            MenuItem exportItem = AddMenuItem("Export Video");
            MenuItem screenshotItem = AddMenuItem("Capture Screen");

            saveAsItem.Clicked += (o2, e2) => _canvas.ShowSaveDialog();
            loadItem.Clicked += (o2, e2) => _canvas.ShowLoadDialog();
            newItem.Clicked += (o2, e2) => NewTrack();
            preferencesItem.Clicked += (o2, e2) => _canvas.ShowPreferencesDialog();
            trackPropsItem.Clicked += (o2, e2) => _canvas.ShowTrackPropertiesDialog();
            triggersItem.Clicked += (o2, e2) => _canvas.ShowTriggerWindow();
            userDirItem.Clicked += (o2, e2) => GameCanvas.OpenUrl(Program.UserDirectory);
            exportItem.Clicked += (o2, e2) => _canvas.ShowExportVideoWindow();
            screenshotItem.Clicked += (o2, e2) => _canvas.ShowScreenCaptureWindow();
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
                    smPencilTool = true;
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
                    smPencilTool = false;
                }
            };
            _linebtn.Clicked += (o, e) =>
            {
                if (CurrentTools._selected != CurrentTools.LineTool)
                {
                    CurrentTools.SetTool(CurrentTools.LineTool);
                }
                else
                {
                    CurrentTools.SetTool(CurrentTools.BezierTool);
                    _bezierbtn.IsHidden = false;
                    _linebtn.IsHidden = true;
                }
            };
            _bezierbtn.Clicked += (o, e) =>
            {
                if (CurrentTools._selected != CurrentTools.BezierTool)
                {
                    CurrentTools.SetTool(CurrentTools.BezierTool);
                }
                else
                {
                    CurrentTools.SetTool(CurrentTools.LineTool);
                    _linebtn.IsHidden = false;
                    _bezierbtn.IsHidden = true;
                }
            };

            _eraserbtn.Clicked += (o, e) => CurrentTools.SetTool(CurrentTools.EraserTool);
            _selectbtn.Clicked += (o, e) => CurrentTools.SetTool(CurrentTools.MoveTool);
            _handbtn.Clicked += (o, e) => CurrentTools.SetTool(CurrentTools.HandTool);
            _flagbtn.Clicked += (o, e) =>
            {
                _editor.Flag(_editor.Offset);
            };
            _startbtn.Clicked += (o, e) =>
            {
                CurrentTools.SelectedTool.Stop();
                if (InputUtils.Check(Hotkey.PlayButtonIgnoreFlag))
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
                _pausebtn.IsHidden = false;
                _startbtn.IsHidden = true;
            };
            _stopbtn.Clicked += (o, e) =>
            {
                CurrentTools.SelectedTool.Stop();
                _editor.Stop();
            };
            _pausebtn.Clicked += (o, e) =>
            {
                CurrentTools.SelectedTool.Stop();
                _editor.TogglePause();
                _pausebtn.IsHidden = true;
                _startbtn.IsHidden = false;
            };
            _generatorbtn.Clicked += (o, e) =>
            {
                _canvas.ShowGeneratorWindow(new OpenTK.Vector2d(e.X, e.Y));
            };
            _menubtn.Clicked += (o, e) =>
            {
                Point canvaspos = LocalPosToCanvas(new Point(_menubtn.X, _menubtn.Y));
                _menu.SetPosition(canvaspos.X, canvaspos.Y + _menubtn.Height);
                _menu.Show();
            };
        }
        private MenuItem AddMenuItem(string caption, Hotkey hotkey = Hotkey.None)
        {
            string hotkeyStr = Settings.HotkeyToString(hotkey);

            // Gwen's hotkey field (accelerator) doesn't affect menu width
            // (there's a "todo" comment there) so expanding menu this hacky way :(
            if (hotkey != Hotkey.None)
                caption += new string(' ', (int)Math.Round(hotkeyStr.Length * 2.5));

            MenuItem item = _menu.AddItem(caption, string.Empty, hotkeyStr);

            return item;
        }
        private void NewTrack()
        {
            if (_editor.TrackChanges != 0)
            {
                var save = MessageBox.Show(
                    _canvas,
                    "You are creating a new track.\nDo you want to save your current changes?",
                    "Create New Track",
                    MessageBox.ButtonType.YesNoCancel);
                save.RenameButtonsYN("Save", "Discard", "Cancel");
                save.Dismissed += (o3, result) =>
                {
                    switch (result)
                    {
                        case DialogResult.Yes:
                            _canvas.ShowSaveDialog();
                            break;
                        case DialogResult.No:
                            SetNewTrack();
                            break;
                    }
                };
            }
            else
            {
                var msg = MessageBox.Show(
                    _canvas,
                    "Are you sure you want to create a new track?",
                    "Create New Track",
                    MessageBox.ButtonType.OkCancel);
                msg.RenameButtons("Create");
                msg.Dismissed += (o3, result) =>
                {
                    if (result == DialogResult.OK)
                        SetNewTrack();
                };
            }
        }
        private void SetNewTrack()
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
            _pausebtn.IsHidden = showplay;
            _startbtn.IsHidden = !showplay;
            base.Think();
        }
        private ImageButton CreateTool(Bitmap image, string tooltip, Hotkey hotkey = Hotkey.None)
        {
            Rectangle ScreenSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var size = (ScreenSize.Width / 1600 < ScreenSize.Height / 1080) ? (ScreenSize.Width / 1600) : (ScreenSize.Height / 1080);
            if (size < 1) { size = 1; };
            ImageButton btn = new ImageButton(_buttoncontainer);
            btn.Dock = Dock.Left;
            btn.SetImage(image);
            btn.SetSize(size*32, size*32);
            btn.Tooltip = tooltip + Settings.HotkeyToString(hotkey, true);
            btn.Margin = new Margin(3, 0, 3, 0);
            return btn;
        }
    }
}