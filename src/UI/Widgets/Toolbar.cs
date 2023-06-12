using Gwen.Controls;
using linerider.UI.Components;
using linerider.Tools;
using System.Drawing;
using System;
using Gwen;

namespace linerider.UI
{
    public class Toolbar : WidgetContainer
    {
        private GameCanvas _canvas;
        private MainWindow _game;
        private Editor _editor;
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
            new MultiToolButton(this, new Tool[] { CurrentTools.PencilTool, CurrentTools.SmoothPencilTool }, Hotkey.EditorPencilTool);
            new MultiToolButton(this, new Tool[] { CurrentTools.LineTool, CurrentTools.BezierTool }, Hotkey.EditorLineTool);
            new ToolButton(this, CurrentTools.EraserTool);
            new ToolButton(this, CurrentTools.SelectTool)
            {
                Subtool = CurrentTools.SelectSubtool,
            };
            new ToolButton(this, CurrentTools.PanTool);

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
            new WidgetButton(this)
            {
                Name = "Stop",
                Icon = GameResources.icon_stop.Bitmap,
                Action = (o, e) => TogglePlayback(playpausebtn, Hotkey.PlaybackStop),
                Hotkey = Hotkey.PlaybackStop,
            };

            new WidgetButton(this)
            {
                Name = "Flag",
                Icon = GameResources.icon_flag.Bitmap,
                Action = (o, e) => _editor.Flag(_editor.Offset),
                Hotkey = Hotkey.PlaybackFlag,
            };

            new WidgetButton(this)
            {
                Name = "Generators",
                Icon = GameResources.icon_generators.Bitmap,
                Action = (o, e) => _canvas.ShowGeneratorWindow(OpenTK.Vector2d.Zero),
                Hotkey = Hotkey.LineGeneratorWindow,
            };

            new WidgetButton(this)
            {
                Name = "Menu",
                Icon = GameResources.icon_menu.Bitmap,
                Action = (o, e) =>
                {
                    WidgetButton button = (WidgetButton)o;
                    Point canvaspos = LocalPosToCanvas(new Point(button.X, button.Y));
                    _menu.SetPosition(canvaspos.X, canvaspos.Y + button.Height);
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
                IsHidden = true,
            };

            AddMenuItem("Save As...", Hotkey.SaveAsWindow)
                .Clicked += (o2, e2) => _canvas.ShowSaveDialog();
            AddMenuItem("Load...", Hotkey.LoadWindow)
                .Clicked += (o2, e2) => _canvas.ShowLoadDialog();
            AddMenuItem("New")
                .Clicked += (o2, e2) => NewTrack();

            AddDivider();

            AddMenuItem("Preferences", Hotkey.PreferencesWindow)
                .Clicked += (o2, e2) => _canvas.ShowPreferencesDialog();
            AddMenuItem("Open User Directory")
                .Clicked += (o2, e2) => GameCanvas.OpenUrl(Program.UserDirectory);

            AddDivider();

            AddMenuItem("Track Properties", Hotkey.TrackPropertiesWindow)
                .Clicked += (o2, e2) => _canvas.ShowTrackPropertiesDialog();
            AddMenuItem("Triggers", Hotkey.TriggerMenuWindow)
                .Clicked += (o2, e2) => _canvas.ShowTriggerWindow();

            AddDivider();

            AddMenuItem("Export Video")
                .Clicked += (o2, e2) => _canvas.ShowExportVideoWindow();
            AddMenuItem("Capture Screen")
                .Clicked += (o2, e2) => _canvas.ShowScreenCaptureWindow();
        }
        private void AddDivider()
        {
            _menu.AddDivider();
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
    }
}