using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using Gwen;
using Gwen.Controls;
namespace linerider.UI
{
    public class HotkeyWidget : ControlBase
    {
        private PropertyTree _kbtree;
        private Button _btnreset;
        private GameCanvas _canvas;
        public HotkeyWidget(ControlBase parent) : base(parent)
        {
            _canvas = (GameCanvas)parent.GetCanvas();
            _kbtree = new PropertyTree(this)
            {
                Dock = Dock.Fill,
            };
            ControlBase container = new ControlBase(this)
            {
                Margin = new Margin(0, 5, 0, 0),
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
            };
            _btnreset = new Button(container)
            {
                Dock = Dock.Right,
                Text = "Default Keybindings"
            };
            _btnreset.Clicked += (o, e) =>
            {
                var box = MessageBox.Show(
                    _canvas,
                    "Are you sure you want to reset your keybindings to default settings?",
                    "Reset keybindings?",
                    MessageBox.ButtonType.OkCancel);
                box.RenameButtons("Reset");
                box.Dismissed += (_o, result) =>
                {
                    if (result == DialogResult.OK)
                    {
                        Settings.ResetKeybindings();
                        Settings.Save();
                        foreach (var kb in Settings.Keybinds)
                        {
                            var prop = GetLabel(kb.Key);
                            if (prop != null)
                            {
                                prop.Value = CreateBindingText(kb.Key);
                            }
                        }
                    }
                };
            };
            Dock = Dock.Fill;
            Setup();
        }
        private void Setup()
        {
            var editorTable = _kbtree.Add("Editor", 150);
            AddBinding(editorTable, "Pencil Tool", Hotkey.EditorPencilTool);
            AddBinding(editorTable, "Line Tool", Hotkey.EditorLineTool);
            AddBinding(editorTable, "Eraser", Hotkey.EditorEraserTool);
            AddBinding(editorTable, "Select Tool", Hotkey.EditorSelectTool);
            AddBinding(editorTable, "Hand Tool", Hotkey.EditorPanTool);
            AddBinding(editorTable, "Quick Pan", Hotkey.EditorQuickPan);
            AddBinding(editorTable, "Drag Canvas", Hotkey.EditorDragCanvas);
            AddBinding(editorTable, "Swatch Color Blue", Hotkey.EditorToolColor1);
            AddBinding(editorTable, "Swatch Color Red", Hotkey.EditorToolColor2);
            AddBinding(editorTable, "Swatch Color Green", Hotkey.EditorToolColor3);
            AddBinding(editorTable, "Cycle Tool Setting", Hotkey.EditorCycleToolSetting);

            AddBinding(editorTable, "Move Start Point", Hotkey.EditorMoveStart,
                "Hold and click the rider to move him");
            AddBinding(editorTable, "Focus on Rider", Hotkey.EditorFocusRider);
            AddBinding(editorTable, "Focus on Flag", Hotkey.EditorFocusFlag);
            AddBinding(editorTable, "Focus First Line", Hotkey.EditorFocusStart);
            AddBinding(editorTable, "Focus Last Line", Hotkey.EditorFocusLastLine);

            AddBinding(editorTable, "Toggle Onion Skinning", Hotkey.PreferenceOnionSkinning);
            AddBinding(editorTable, "Toggle Advanced Visuals", Hotkey.PreferenceAllCheckboxSettings);
            AddBinding(editorTable, "Toggle Rider Visibility", Hotkey.PreferenceInvisibleRider);
            AddBinding(editorTable, "Toggle Track Overlay", Hotkey.ToolToggleOverlay);
            AddBinding(editorTable, "Toggle Debug Grid", Hotkey.PreferenceDrawDebugGrid);
            AddBinding(editorTable, "Toggle Debug Camera", Hotkey.PreferenceDrawDebugCamera);
            AddBinding(editorTable, "Toggle Preview Mode", Hotkey.TogglePreviewMode);

            AddBinding(editorTable, "Remove Newest Line", Hotkey.EditorRemoveLatestLine);
            AddBinding(editorTable, "Undo Last Action", Hotkey.EditorUndo);
            AddBinding(editorTable, "Redo Last Undo Action", Hotkey.EditorRedo);

            var toolTable = _kbtree.Add("Tool", 150);
            AddBinding(toolTable, "Line Angle Snap", Hotkey.ToolXYSnap);
            AddBinding(toolTable, "Toggle Line Snap", Hotkey.ToolToggleSnap);
            AddBinding(toolTable, "Flip Line", Hotkey.LineToolFlipLine,
                "Hold before drawing a new line");

            var selecttoolTable = _kbtree.Add("Select Tool", 150);
            AddBinding(selecttoolTable, "Lock Angle", Hotkey.ToolAngleLock);
            AddBinding(selecttoolTable, "Move Whole Line", Hotkey.ToolSelectBothJoints);
            AddBinding(selecttoolTable, "Life Lock", Hotkey.ToolLifeLock,
                "While pressed moving the line will stop if the rider survives");
            AddBinding(selecttoolTable, "Move Along Axis", Hotkey.ToolAxisLock,
                "If you're moving a whole line,\nuse this to keep it on the same plane");
            AddBinding(selecttoolTable, "Move Along Right angle", Hotkey.ToolPerpendicularAxisLock,
            "If you're moving a whole line,\nuse this to keep perpendicular to its plane");
            AddBinding(selecttoolTable, "Lock Length", Hotkey.ToolLengthLock);
            AddBinding(selecttoolTable, "Copy Selection", Hotkey.ToolCopy);
            AddBinding(selecttoolTable, "Cut", Hotkey.ToolCut);
            AddBinding(selecttoolTable, "Paste", Hotkey.ToolPaste);
            AddBinding(selecttoolTable, "Delete Selection", Hotkey.ToolDelete);
            AddBinding(selecttoolTable, "Copy Selection Values", Hotkey.ToolCopyValues);
            AddBinding(selecttoolTable, "Paste Selection Values", Hotkey.ToolPasteValues);
            AddBinding(selecttoolTable, "Convert Selection (B)", Hotkey.ToolSwitchBlue,
                "Convert all selected lines to blue lines");
            AddBinding(selecttoolTable, "Convert Selection (R)", Hotkey.ToolSwitchRed,
                "Convert all selected lines to red lines");
            AddBinding(selecttoolTable, "Convert Selection (G)", Hotkey.ToolSwitchGreen,
                "Convert all selected lines to green lines");

            var playbackTable = _kbtree.Add("Playback", 150);
            AddBinding(playbackTable, "Toggle Flag", Hotkey.PlaybackFlag);
            AddBinding(playbackTable, "Reset Camera", Hotkey.PlaybackResetCamera);
            AddBinding(playbackTable, "Start Track", Hotkey.PlaybackStart);
            AddBinding(playbackTable, "Start Track before Flag", Hotkey.PlaybackStartIgnoreFlag);
            AddBinding(playbackTable, "Start Track in Slowmo", Hotkey.PlaybackStartSlowmo);
            AddBinding(playbackTable, "Stop Track", Hotkey.PlaybackStop);
            AddBinding(playbackTable, "Toggle Pause", Hotkey.PlaybackTogglePause);
            AddBinding(playbackTable, "Frame Next", Hotkey.PlaybackFrameNext);
            AddBinding(playbackTable, "Frame Previous", Hotkey.PlaybackFramePrev);
            AddBinding(playbackTable, "Iteration Next", Hotkey.PlaybackIterationNext);
            AddBinding(playbackTable, "Iteration Previous", Hotkey.PlaybackIterationPrev);
            AddBinding(playbackTable, "Hold -- Forward", Hotkey.PlaybackForward);
            AddBinding(playbackTable, "Hold -- Rewind", Hotkey.PlaybackBackward);
            AddBinding(playbackTable, "Increase Playback Rate", Hotkey.PlaybackSpeedUp);
            AddBinding(playbackTable, "Decrease Playback Rate", Hotkey.PlaybackSpeedDown);
            AddBinding(playbackTable, "Toggle Slowmo", Hotkey.PlaybackSlowmo);
            AddBinding(playbackTable, "Zoom In", Hotkey.PlaybackZoom);
            AddBinding(playbackTable, "Zoom Out", Hotkey.PlaybackUnzoom);
            AddBinding(playbackTable, "Play Button - Ignore Flag", Hotkey.PlayButtonIgnoreFlag);

            var menuTable = _kbtree.Add("Menus", 150);
            AddBinding(menuTable, "Quicksave", Hotkey.Quicksave);
            AddBinding(menuTable, "Save As Menu", Hotkey.SaveAsWindow);
            AddBinding(menuTable, "Open Preferences", Hotkey.PreferencesWindow);
            AddBinding(menuTable, "Open Game Menu", Hotkey.GameMenuWindow);
            AddBinding(menuTable, "Open Track Properties", Hotkey.TrackPropertiesWindow);
            AddBinding(menuTable, "Load Track", Hotkey.LoadWindow);
            AddBinding(menuTable, "Open Trigger Menu", Hotkey.TriggerMenuWindow);
            AddBinding(menuTable, "Open Generator Window", Hotkey.LineGeneratorWindow);

            var coordinateTable = _kbtree.Add("Clipboard Bindings", 150);
            AddBinding(coordinateTable, "CopyX0", Hotkey.CopyX0);
            AddBinding(coordinateTable, "CopyY0", Hotkey.CopyY0);
            AddBinding(coordinateTable, "CopyX1", Hotkey.CopyX1);
            AddBinding(coordinateTable, "CopyY1", Hotkey.CopyY1);
            AddBinding(coordinateTable, "CopyX2", Hotkey.CopyX2);
            AddBinding(coordinateTable, "CopyY2", Hotkey.CopyY2);
            AddBinding(coordinateTable, "CopyX3", Hotkey.CopyX3);
            AddBinding(coordinateTable, "CopyY3", Hotkey.CopyY3);
            AddBinding(coordinateTable, "CopyX4", Hotkey.CopyX4);
            AddBinding(coordinateTable, "CopyY4", Hotkey.CopyY4);
            AddBinding(coordinateTable, "CopyX5", Hotkey.CopyX5);
            AddBinding(coordinateTable, "CopyY5", Hotkey.CopyY5);
            AddBinding(coordinateTable, "CopyX6", Hotkey.CopyX6);
            AddBinding(coordinateTable, "CopyY6", Hotkey.CopyY6);
            AddBinding(coordinateTable, "CopyX7", Hotkey.CopyX7);
            AddBinding(coordinateTable, "CopyY7", Hotkey.CopyY7);
            AddBinding(coordinateTable, "CopyX8", Hotkey.CopyX8);
            AddBinding(coordinateTable, "CopyY8", Hotkey.CopyY8);
            AddBinding(coordinateTable, "CopyX9", Hotkey.CopyX9);
            AddBinding(coordinateTable, "CopyY9", Hotkey.CopyY9);
            _kbtree.ExpandAll();
        }
        private List<Keybinding> FetchBinding(Hotkey hotkey)
        {
            if (!Settings.Keybinds.ContainsKey(hotkey))
                Settings.Keybinds[hotkey] = new List<Keybinding>();
            var ret = Settings.Keybinds[hotkey];
            if (ret.Count == 0)
                ret.Add(new Keybinding());//empty
            return ret;
        }
        private string CreateBindingText(Hotkey hotkey)
        {
            var hk = FetchBinding(hotkey);
            string hkstring = "";
            for (int i = 0; i < hk.Count; i++)
            {
                if (hkstring.Length != 0)
                {
                    hkstring += " | ";
                }
                hkstring += hk[i].ToString();
            }
            return hkstring;
        }
        private void AddBinding(PropertyTable table, string label, Hotkey hotkey, string tooltip = null)
        {
            var hk = FetchBinding(hotkey);
            string hkstring = CreateBindingText(hotkey);
            LabelProperty prop = new LabelProperty(null)
            {
                Value = hkstring,
                Name = hotkey.ToString(),
            };
            var row = table.Add(label, prop);
            if (tooltip != null)
            {
                row.Tooltip = tooltip;
            }
            prop.Clicked += (o, e) =>
            {
                ShowHotkeyWindow(hotkey, prop, 0);
            };
            prop.RightClicked += (o, e) =>
            {
                Menu opt = new Menu(_canvas);
                opt.AddItem("Change Primary").Clicked += (_o, _e) =>
                {
                    ShowHotkeyWindow(hotkey, prop, 0);
                };
                opt.AddItem("Change Secondary").Clicked += (_o, _e) =>
                {
                    ShowHotkeyWindow(hotkey, prop, 1);
                };
                opt.AddItem("Remove Secondary").Clicked += (_o, _e) =>
                {
                    var k = Settings.Keybinds[hotkey];
                    if (k.Count > 1)
                    {
                        k.RemoveAt(1);
                        prop.Value = CreateBindingText(hotkey);
                        Settings.Save();
                    }
                };
                opt.AddItem("Restore Default").Clicked += (_o, _e) =>
                {
                    var def = Settings.GetHotkeyDefault(hotkey);
                    if (def != null && def.Count != 0)
                    {
                        int idx = 0;
                        var keys = Settings.Keybinds[hotkey];
                        keys.Clear();
                        foreach (var defaultbind in def)
                        {
                            var conflict = Settings.CheckConflicts(defaultbind, hotkey);
                            if (conflict != Hotkey.None)
                                RemoveKeybind(conflict, defaultbind);
                            ChangeKeybind(prop, hotkey, idx++, defaultbind);
                        }
                        Settings.Save();
                    }
                };
                opt.Open(Pos.Center);
            };
        }
        private LabelProperty GetLabel(Hotkey hotkey)
        {
            var ret = _kbtree.FindChildByName(hotkey.ToString(), true);
            if (ret != null)
                return (LabelProperty)ret;
            return null;
        }
        private void ShowHotkeyWindow(Hotkey hotkey, LabelProperty prop, int kbindex)
        {
            PropertyRow row = (PropertyRow)prop.Parent;
            var wnd = new RebindHotkeyWindow(_canvas, row.Label.ToString());
            wnd.KeybindChanged += (x, newbind) =>
            {
                TryNewKeybind(hotkey, newbind, kbindex);
            };
            wnd.ShowCentered();
        }
        private void RemoveKeybind(Hotkey hotkey, Keybinding binding)
        {
            var conflictkeys = Settings.Keybinds[hotkey];
            for (int i = 0; i < conflictkeys.Count; i++)
            {
                if (conflictkeys[i].IsBindingEqual(binding))
                {
                    var conflictprop = GetLabel(hotkey);
                    conflictkeys.RemoveAt(i);
                    conflictprop.Value = CreateBindingText(hotkey);
                    break;
                }
            }
        }
        private bool TryNewKeybind(Hotkey hotkey, Keybinding newbind, int kbindex)
        {
            var k = Settings.Keybinds[hotkey];
            var conflict = CheckConflicts(newbind, hotkey);
            if (conflict == hotkey)
                return true;
            var prop = GetLabel(hotkey);
            if (conflict != Hotkey.None)
            {
                var mbox = MessageBox.Show(_canvas,
                    $"Keybinding conflicts with {conflict}, If you proceed you will overwrite it.\nDo you want to continue?",
                    "Conflict detected", MessageBox.ButtonType.OkCancel);
                mbox.Dismissed += (o, e) =>
                {
                    if (e == DialogResult.OK)
                    {
                        RemoveKeybind(conflict, newbind);
                        ChangeKeybind(prop, hotkey, kbindex, newbind);
                    }
                };
                return false;
            }
            ChangeKeybind(prop, hotkey, kbindex, newbind);
            return true;
        }
        private void ChangeKeybind(LabelProperty prop, Hotkey hotkey, int kbindex, Keybinding kb)
        {
            var k = Settings.Keybinds[hotkey];
            if (kbindex >= k.Count)
            {
                k.Add(kb);
            }
            else
            {
                Settings.Keybinds[hotkey][kbindex] = kb;
            }
            prop.Value = CreateBindingText(hotkey);
            Settings.Save();
        }
        private Hotkey CheckConflicts(Keybinding keybinding, Hotkey hotkey)
        {
            if (!keybinding.IsEmpty)
            {
                var inputconflicts = Settings.KeybindConflicts[hotkey];
                foreach (var keybinds in Settings.Keybinds)
                {
                    var hk = keybinds.Key;
                    var conflicts = Settings.KeybindConflicts[hk];
                    //if the conflicts is equal to or below inputconflicts
                    //then we can compare for conflict
                    //if conflicts is above inputconflicts, ignore
                    if (inputconflicts.HasFlag(conflicts))
                    {
                        foreach (var keybind in keybinds.Value)
                        {
                            if (keybind.IsBindingEqual(keybinding))
                                return hk;
                        }
                    }
                }
            }
            return Hotkey.None;
        }
    }
}