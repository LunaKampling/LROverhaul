using Gwen;
using Gwen.Controls;
using linerider.Tools;
using linerider.UI.Components;
using System;
using System.Drawing;

namespace linerider.UI
{
    public class Toolbar : WidgetContainer
    {
        private readonly GameCanvas _canvas;
        private readonly MainWindow _game;
        private readonly Editor _editor;
        private Menu _menu;

        public Toolbar(ControlBase parent, MainWindow game) : base(parent)
        {
            ShouldDrawBackground = false;
            Padding = Padding.Zero;

            _canvas = (GameCanvas)parent.GetCanvas();
            _editor = game.Track;
            _game = game;
            MakeButtons();
            MakeMenu();
        }

        private void MakeButtons()
        {
            bool condition() => !_editor.Playing;

            _ = new MultiToolButton(this, new Tool[] { CurrentTools.PencilTool, CurrentTools.SmoothPencilTool }, Hotkey.EditorPencilTool) { HotkeyCondition = condition };
            _ = new MultiToolButton(this, new Tool[] { CurrentTools.LineTool, CurrentTools.BezierTool }, Hotkey.EditorLineTool) { HotkeyCondition = condition };
            _ = new ToolButton(this, CurrentTools.EraserTool) { HotkeyCondition = condition };
            _ = new ToolButton(this, CurrentTools.SelectTool) { HotkeyCondition = condition, Subtool = CurrentTools.SelectSubtool };
            _ = new ToolButton(this, CurrentTools.PanTool) { HotkeyCondition = condition };

            WidgetButton playpausebtn = new WidgetButton(this)
            {
                Name = "Play",
                Icon = GameResources.icon_play.Bitmap,
                Action = (o, e) => TogglePlayback((WidgetButton)o, ((WidgetButton)o).Hotkey),
                Hotkey = Hotkey.PlaybackTogglePause,
            };
            InputUtils.RegisterHotkey(Hotkey.PlaybackStart, () => true, () => TogglePlayback(playpausebtn, Hotkey.PlaybackStart));
            InputUtils.RegisterHotkey(Hotkey.PlaybackStartSlowmo, () => true, () => TogglePlayback(playpausebtn, Hotkey.PlaybackStartSlowmo));
            InputUtils.RegisterHotkey(Hotkey.PlaybackStartIgnoreFlag, () => true, () => TogglePlayback(playpausebtn, Hotkey.PlaybackStartIgnoreFlag));

            _ = new WidgetButton(this)
            {
                Name = "Stop",
                Icon = GameResources.icon_stop.Bitmap,
                Action = (o, e) => TogglePlayback(playpausebtn, Hotkey.PlaybackStop),
                Hotkey = Hotkey.PlaybackStop,
            };
            _ = new WidgetButton(this)
            {
                Name = "Flag",
                Icon = GameResources.icon_flag.Bitmap,
                Action = (o, e) => _editor.Flag(_editor.Offset),
                Hotkey = Hotkey.PlaybackFlag,
            };
            _ = new WidgetButton(this)
            {
                Name = "Generators",
                Icon = GameResources.icon_generators.Bitmap,
                Action = (o, e) => _canvas.ShowGeneratorWindow(OpenTK.Vector2d.Zero),
                Hotkey = Hotkey.LineGeneratorWindow,
                HotkeyCondition = condition,
            };
            _ = new WidgetButton(this)
            {
                Name = "Menu",
                Icon = GameResources.icon_menu.Bitmap,
                Action = (o, e) =>
                {
                    WidgetButton button = (WidgetButton)o;
                    Point canvaspos = LocalPosToCanvas(new Point(button.X, button.Y));
                    _menu.SetPosition(canvaspos.X, canvaspos.Y + button.Height);
                    _menu.Width = 250;
                    _menu.Show();
                },
            };
        }
        private void TogglePlayback(WidgetButton playpauseButton, Hotkey hotkey)
        {
            _game.StopTools();

            switch (hotkey)
            {
                case Hotkey.PlaybackTogglePause:
                    _editor.TogglePause();
                    break;
                case Hotkey.PlaybackStart:
                    _editor.StartFromFlag();
                    _editor.ResetSpeedDefault();
                    break;
                case Hotkey.PlaybackStartSlowmo:
                    _editor.StartFromFlag();
                    _editor.Scheduler.UpdatesPerSecond = Settings.SlowmoSpeed;
                    break;
                case Hotkey.PlaybackStartIgnoreFlag:
                    _editor.StartIgnoreFlag();
                    _editor.ResetSpeedDefault();
                    break;
                case Hotkey.PlaybackStop:
                    _editor.Stop();
                    break;
            }

            if (_editor.Paused)
            {
                playpauseButton.Name = "Play";
                playpauseButton.Icon = GameResources.icon_play.Bitmap;
            }
            else
            {
                playpauseButton.Name = "Pause";
                playpauseButton.Icon = GameResources.icon_pause.Bitmap;
            }
        }
        private void MakeMenu()
        {
            _menu = new Menu(_canvas)
            {
                AutoSizeToContents = false,
                IsHidden = true,
            };

            _ = AddMenuItem("Save As...", Hotkey.SaveAsWindow, () => _canvas.ShowSaveDialog());
            _ = AddMenuItem("Load...", Hotkey.LoadWindow, () => _canvas.ShowLoadDialog());
            _ = AddMenuItem("New", () => NewTrack());
            AddDivider();
            _ = AddMenuItem("Preferences", Hotkey.PreferencesWindow, () => _canvas.ShowPreferencesDialog());
            _ = AddMenuItem("Open User Directory", () => GameCanvas.OpenUrl(Program.UserDirectory));
            AddDivider();
            _ = AddMenuItem("Track Properties", Hotkey.TrackPropertiesWindow, () => _canvas.ShowTrackPropertiesDialog());
            _ = AddMenuItem("Triggers", Hotkey.TriggerMenuWindow, () => _canvas.ShowTriggerWindow());
            AddDivider();
            _ = AddMenuItem("Export Video", () => _canvas.ShowExportVideoWindow());
            _ = AddMenuItem("Capture Screen", () => _canvas.ShowScreenCaptureWindow());
        }
        private void AddDivider() => _menu.AddDivider();
        private MenuItem AddMenuItem(string caption, Action action) => AddMenuItem(caption, Hotkey.None, action);
        private MenuItem AddMenuItem(string caption, Hotkey hotkey, Action action)
        {
            string hotkeyStr = Settings.HotkeyToString(hotkey);
            MenuItem item = _menu.AddItem(caption, string.Empty, hotkeyStr);

            if (action != null)
                item.Clicked += (o2, e2) => action();

            return item;
        }
        private void NewTrack()
        {
            if (_editor.TrackChanges != 0)
            {
                MessageBox save = MessageBox.Show(
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
                MessageBox msg = MessageBox.Show(
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
    }
}