using Gwen;
using Gwen.Controls;
using linerider.Tools;
using linerider.UI.Components;
using System;

namespace linerider.UI.Widgets
{
    public class Toolbar : WidgetContainer
    {
        private readonly GameCanvas _canvas;
        private readonly MainWindow _game;
        private readonly Editor _editor;
        private WidgetButton _playpausebtn;
        private bool _paused;
        private Menu _menu;

        public Toolbar(ControlBase parent, MainWindow game) : base(parent)
        {
            ShouldDrawBackground = false;
            Padding = Padding.Zero;

            _canvas = (GameCanvas)parent.GetCanvas();
            _editor = game.Track;
            _game = game;

            MakeButtons();

            _menu = new Menu(_canvas)
            {
                AutoSizeToContents = false,
                IsHidden = true,
            };

            PopulateMenu();
        }

        private void MakeButtons()
        {
            bool hotkeyCondition() => !_editor.Playing;

            _ = new MultiToolButton(this, [CurrentTools.PencilTool, CurrentTools.SmoothPencilTool])
            {
                Hotkey = Hotkey.EditorPencilTool,
                HotkeyCondition = hotkeyCondition,
            };
            _ = new MultiToolButton(this, [CurrentTools.LineTool, CurrentTools.BezierTool])
            {
                Hotkey = Hotkey.EditorLineTool,
                HotkeyCondition = hotkeyCondition,
            };
            _ = new ToolButton(this, CurrentTools.EraserTool)
            {
                Hotkey = Hotkey.EditorEraserTool,
                HotkeyCondition = hotkeyCondition,
            };
            _ = new ToolButton(this, CurrentTools.SelectTool)
            {
                Hotkey = Hotkey.EditorSelectTool,
                HotkeyCondition = hotkeyCondition,
                Subtool = CurrentTools.SelectSubtool,
            };
            _ = new ToolButton(this, CurrentTools.PanTool)
            {
                Hotkey = Hotkey.EditorPanTool,
                HotkeyCondition = hotkeyCondition,
            };

            _playpausebtn = new WidgetButton(this)
            {
                Name = "Play",
                Icon = GameResources.icon_play.Bitmap,
                Action = (o, e) =>
                {
                    _game.StopTools();
                    _editor.TogglePause();
                },
                Hotkey = Hotkey.PlaybackTogglePause,
            };

            _ = new WidgetButton(this)
            {
                Name = "Stop",
                Icon = GameResources.icon_stop.Bitmap,
                Action = (o, e) =>
                {
                    _game.StopTools();
                    _editor.Stop();
                },
                Hotkey = Hotkey.PlaybackStop,
            };
            _ = new WidgetButton(this)
            {
                Name = "Flag",
                Icon = GameResources.icon_flag.Bitmap,
                Action = (o, e) => _editor.SetFlagFrame(_editor.Offset),
                Hotkey = Hotkey.PlaybackFlag,
            };
            _ = new WidgetButton(this)
            {
                Name = "Generators",
                Icon = GameResources.icon_generators.Bitmap,
                Action = (o, e) => _canvas.ShowGeneratorWindow(OpenTK.Mathematics.Vector2d.Zero),
                Hotkey = Hotkey.LineGeneratorWindow,
                HotkeyCondition = hotkeyCondition,
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
        private void PopulateMenu()
        {
            _ = AddMenuItem("Save As...", Hotkey.SaveAsWindow, () => _canvas.ShowSaveDialog());
            _ = AddMenuItem("Load...", Hotkey.LoadWindow, () => _canvas.ShowLoadDialog());
            _ = AddMenuItem("New", () => NewTrack());
            AddDivider();
            _ = AddMenuItem("Preferences", Hotkey.PreferencesWindow, () => _canvas.ShowPreferencesDialog());
            _ = AddMenuItem("Open User Directory", () => GameCanvas.OpenUrl(Settings.Local.UserDirPath));
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
            _editor.ChangeTrack(new Track() { Name = Utils.Constants.InternalDefaultTrackName });
            Settings.LastSelectedTrack = "";
            Settings.Save();
            _editor.Invalidate();
        }

        public override void Think()
        {
            if (_paused != _editor.Paused)
            {
                _paused = _editor.Paused;

                if (_paused)
                {
                    _playpausebtn.Name = "Play";
                    _playpausebtn.Icon = GameResources.icon_play.Bitmap;
                }
                else
                {
                    _playpausebtn.Name = "Pause";
                    _playpausebtn.Icon = GameResources.icon_pause.Bitmap;
                }
            }

            base.Think();
        }
    }
}