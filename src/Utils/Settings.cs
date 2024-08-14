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

using linerider.UI;
using linerider.Utils;
using OpenTK;
using OpenTK.Input;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Key = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace linerider
{
    internal static class Settings
    {
        public enum BezierMode
        {
            Direct = 0,
            Trace = 1
        }
        public enum PlaybackZoomMode
        {
            AsIs = 0,
            Frame = 1
        }
        public static class Recording
        {
            public static bool ShowTools = false;
            public static bool ShowFps = true;
            public static bool ShowPpf = true;
            public static bool ShowHitTest = false;
            public static bool EnableColorTriggers = true;
            public static int RecordingWidth = 0;
            public static int RecordingHeight = 0;
            public static bool ResIndZoom = true; // Use resolution-independent zoom based on window size when recording
        }
        public static class Local
        {
            public static string Version;
            public static string UserDirPath;
            public static bool RecordingMode;
            public static bool TrackOverlay = false;
            public static bool TrackOverlayFixed = false;
            public static int TrackOverlayFixedFrame = 0;
            public static int TrackOverlayOffset = -1;
            public static bool LockCamera;
        }
        public static class Editor
        {
            public static bool ShowCoordinateMenu = false;
            public static bool HitTest;
            public static bool SnapNewLines;
            public static bool SnapMoveLine;
            public static bool SnapToGrid;
            public static bool ForceXySnap;
            public static float XySnapDegrees;
            public static bool MomentumVectors;
            public static bool RenderGravityWells;
            public static bool DrawContactPoints;
            public static bool LifeLockNoOrange;
            public static bool LifeLockNoFakie;
            public static bool ShowLineLength;
            public static bool ShowLineAngle;
            public static bool ShowLineID;
            public static bool NoHitSelect;
        }
        public static class Colors
        {
            public static Color ExportBg;
            public static Color EditorBg;
            public static Color EditorNightBg;
            public static Color ExportLine;
            public static Color EditorLine;
            public static Color EditorNightLine;
            public static Color StandardLine;
            public static Color AccelerationLine;
            public static Color SceneryLine;
        }
        public static class Bezier
        {
            public static int Resolution;
            public static int NodeSize;
            public static BezierMode Mode;
        }

        public static class SmoothPencil
        {
            public static int smoothStabilizer = 10;
            public static int smoothUpdateSpeed = 0;
        }
        public static PlaybackZoomMode PlaybackZoomType;
        public static float Volume;
        public static bool SuperZoom;
        public static bool NightMode;
        public static bool SmoothCamera;
        public static bool PredictiveCamera;
        public static bool RoundLegacyCamera;
        public static bool SmoothPlayback;
        public static bool CheckForUpdates;

        public static string RecordResolution;
        public static bool RecordSmooth;
        public static bool RecordMusic;
        public static bool RecordShowPpf;
        public static bool RecordShowFps;
        public static bool RecordShowTools;
        public static bool RecordShowHitTest;
        public static bool RecordShowColorTriggers;
        public static bool RecordResIndependentZoom;

        public static bool ScreenshotLockRatio;
        public static int ScreenshotWidth;
        public static int ScreenshotHeight;
        public static bool ScreenshotShowPpf;
        public static bool ScreenshotShowFps;
        public static bool ScreenshotShowTools;
        public static bool ScreenshotShowHitTest;
        public static bool ScreenshotResIndependentZoom;

        public static float UIScale;
        public static bool UIShowZoom;
        public static bool UIShowSpeedButtons;
        public static int DefaultTimelineLength;
        public static int DefaultTriggerLength;

        public static float ScrollSensitivity;
        public static bool LimitLineKnobsSize;
        public static int SettingsPane;
        public static bool MuteAudio;
        public static bool PreviewMode;
        public static int SlowmoSpeed;
        public static float DefaultPlayback;
        public static bool DrawCollisionGrid; // Draw the grid used in line collision detection
        public static bool DrawAGWs; // Draw the normally invisible line extensions used to smooth curve collisions
        public static bool DrawFloatGrid; // Draw the exponential grid of floating-point 'regions' (used for angled kramuals)
        public static bool DrawCamera; // Draw the camera's area
        public static float ZoomMultiplier; // A constant multiplier for the zoom

        // LRTran settings
        public static string SelectedScarf; // What custom scarf is selected
        public static string SelectedBoshSkin; // What bosh skin is selected
        public static int ScarfAmount; // How many scarves the rider has
        public static int ScarfSegmentsPrimary; // How many segments primary scarf has
        public static int ScarfSegmentsSecondary; // How many segments secondary scarves have
        public static int autosaveChanges; // Changes when autosave starts
        public static int autosaveMinutes; // Amount of minues per autosave
        public static string AutosavePrefix; // Name of autosave file
        public static bool startWindowMaximized; // Start window maximized
        public static string DefaultSaveFormat; // What the save menu auto picks 
        public static string DefaultAutosaveFormat; // What the autosave format is
        public static string DefaultQuicksaveFormat; // What the autosave format is
        public static string DefaultCrashBackupFormat; // Format crash backups are saved to

        // RatherBeLunar Addon Settings
        public static bool velocityReferenceFrameAnimation = true;
        public static bool recededLinesAsScenery;
        public static bool forwardLinesAsScenery;
        public static float animationRelativeVelX;
        public static float animationRelativeVelY;

        public static bool ColorPlayback;
        public static bool LockTrackDuration;
        public static bool SyncTrackAndSongDuration;
        public static bool OnionSkinning;
        public static int PastOnionSkins;
        public static int FutureOnionSkins;
        private static string _lastSelectedTrack = "";
        public static Dictionary<Hotkey, KeyConflicts> KeybindConflicts = new Dictionary<Hotkey, KeyConflicts>();
        public static Dictionary<Hotkey, List<Keybinding>> Keybinds = new Dictionary<Hotkey, List<Keybinding>>();
        private static readonly Dictionary<Hotkey, List<Keybinding>> DefaultKeybinds = new Dictionary<Hotkey, List<Keybinding>>();
        public static string LastSelectedTrack
        {
            get
            {
                if (!string.IsNullOrEmpty(_lastSelectedTrack) && _lastSelectedTrack.StartsWith(Constants.LastTrackRelativePrefix))
                    return Path.Combine(Local.UserDirPath, _lastSelectedTrack.Substring(Constants.LastTrackRelativePrefix.Length));
                else
                    return _lastSelectedTrack;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.StartsWith(Local.UserDirPath))
                    _lastSelectedTrack = Constants.LastTrackRelativePrefix + value.Substring(Local.UserDirPath.Length);
                else
                    _lastSelectedTrack = value;
            }
        }

        // Malizma Addon Settings
        public static bool InvisibleRider;

        // Computed settings
        public static class Computed
        {
            public static float MaxZoom => SuperZoom ? Constants.MaxSuperZoom : Constants.MaxZoom;
            public static double UIScale => Settings.UIScale > 0 ? Settings.UIScale : (float)Constants.ScreenScale;
            public static Color BGColor => NightMode ? Colors.EditorNightBg : Colors.EditorBg;
            public static Color LineColor => NightMode ? Colors.EditorNightLine : Colors.EditorLine;
            public static bool LockCamera => Local.RecordingMode ? false : Local.LockCamera;
            public static bool IsUserDirPortable => Local.UserDirPath != Program.UserDirectory;
            public static int DefaultTimelineLength => Settings.DefaultTimelineLength * Constants.PhysicsRate;
        }

        static Settings()
        {
            RestoreDefaultSettings();
            foreach (Hotkey hk in Enum.GetValues(typeof(Hotkey)))
            {
                if (hk == Hotkey.None)
                    continue;
                KeybindConflicts.Add(hk, KeyConflicts.General);
                Keybinds.Add(hk, new List<Keybinding>());
            }
            // Conflicts, for keybinds that depend on a state, so keybinds 
            // outside of its state can be set as long
            // as its dependant state (general) doesnt have a keybind set
            KeybindConflicts[Hotkey.PlaybackZoom] = KeyConflicts.Playback;
            KeybindConflicts[Hotkey.PlaybackUnzoom] = KeyConflicts.Playback;
            KeybindConflicts[Hotkey.PlaybackSpeedUp] = KeyConflicts.Playback;
            KeybindConflicts[Hotkey.PlaybackSpeedDown] = KeyConflicts.Playback;

            KeybindConflicts[Hotkey.LineToolFlipLine] = KeyConflicts.LineTool;

            KeybindConflicts[Hotkey.ToolXYSnap] = KeyConflicts.Tool;
            KeybindConflicts[Hotkey.ToolToggleSnap] = KeyConflicts.Tool;
            KeybindConflicts[Hotkey.EditorCancelTool] = KeyConflicts.Tool;

            KeybindConflicts[Hotkey.ToolLengthLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolAngleLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolAxisLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolPerpendicularAxisLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolLifeLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolLengthLock] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolCopy] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolCut] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolPaste] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolDelete] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolCopyValues] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolPasteValues] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolSwitchRed] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolSwitchGreen] = KeyConflicts.SelectTool;
            KeybindConflicts[Hotkey.ToolSwitchBlue] = KeyConflicts.SelectTool;

            KeybindConflicts[Hotkey.EditorCancelTool] = KeyConflicts.HardCoded;
            KeybindConflicts[Hotkey.ToolAddSelection] = KeyConflicts.HardCoded;
            KeybindConflicts[Hotkey.ToolToggleSelection] = KeyConflicts.HardCoded;
            KeybindConflicts[Hotkey.ToolScaleAspectRatio] = KeyConflicts.HardCoded;
            SetupDefaultKeybinds();
        }
        public static void RestoreDefaultSettings()
        {
            Editor.HitTest = false;
            Editor.SnapNewLines = true;
            Editor.SnapMoveLine = true;
            Editor.SnapToGrid = false;
            Editor.ForceXySnap = false;
            Editor.XySnapDegrees = 15;
            Editor.MomentumVectors = false;
            Editor.RenderGravityWells = false;
            Editor.DrawContactPoints = false;
            Editor.LifeLockNoOrange = false;
            Editor.LifeLockNoFakie = false;
            Editor.ShowLineLength = true;
            Editor.ShowLineAngle = true;
            Editor.ShowLineID = false;
            Editor.NoHitSelect = false;
            Colors.ExportBg = Constants.BgExportColor;
            Colors.EditorBg = Constants.BgEditorColor;
            Colors.EditorNightBg = Constants.BgEditorNightColor;
            Colors.ExportLine = Constants.ExportLineColor;
            Colors.EditorLine = Constants.DefaultLineColor;
            Colors.EditorNightLine = Constants.DefaultNightLineColor;
            Colors.AccelerationLine = Constants.RedLineColor;
            Colors.SceneryLine = Constants.SceneryLineColor;
            Colors.StandardLine = Constants.BlueLineColor;
            Bezier.Resolution = 30;
            Bezier.NodeSize = 15;
            Bezier.Mode = BezierMode.Direct;
            PlaybackZoomType = PlaybackZoomMode.AsIs;
            Volume = 100;
            SuperZoom = false;
            NightMode = false;
            SmoothCamera = true;
            PredictiveCamera = false;
            RoundLegacyCamera = true;
            SmoothPlayback = true;
            CheckForUpdates = true;

            RecordResolution = "720p";
            RecordSmooth = true;
            RecordMusic = true;
            RecordShowPpf = true;
            RecordShowFps = true;
            RecordShowTools = false;
            RecordShowHitTest = false;
            RecordShowColorTriggers = true;
            RecordResIndependentZoom = true;

            ScreenshotLockRatio = false;
            ScreenshotWidth = 1280;
            ScreenshotHeight = 720;
            ScreenshotShowPpf = true;
            ScreenshotShowFps = true;
            ScreenshotShowTools = false;
            ScreenshotShowHitTest = false;
            ScreenshotResIndependentZoom = true;

            UIScale = 0f;
            UIShowZoom = true;
            UIShowSpeedButtons = false;
            DefaultTimelineLength = 30;
            DefaultTriggerLength = Constants.PhysicsRate;

            LimitLineKnobsSize = false;
            ScrollSensitivity = 1;
            SettingsPane = 0;
            MuteAudio = false;
            PreviewMode = false;
            SlowmoSpeed = 2;
            DefaultPlayback = 1f;
            ColorPlayback = false;
            OnionSkinning = false;
            PastOnionSkins = 10;
            FutureOnionSkins = 20;
            ScarfSegmentsPrimary = 5;
            SelectedScarf = Constants.InternalDefaultName;
            SelectedBoshSkin = Constants.InternalDefaultName;
            ScarfAmount = 1;
            ScarfSegmentsSecondary = 5;
            autosaveChanges = 50;
            autosaveMinutes = 5;
            AutosavePrefix = "Autosave";
            startWindowMaximized = false;
            DefaultSaveFormat = ".trk";
            DefaultAutosaveFormat = ".trk";
            DefaultQuicksaveFormat = ".trk";
            DefaultCrashBackupFormat = ".trk";
            DrawCollisionGrid = false;
            DrawAGWs = false;
            DrawFloatGrid = false;
            DrawCamera = false;
            ZoomMultiplier = 1.0f;
            InvisibleRider = false;
        }
        public static void ResetKeybindings()
        {
            foreach (KeyValuePair<Hotkey, List<Keybinding>> kb in Keybinds)
            {
                kb.Value.Clear();
            }
            LoadDefaultKeybindings();
        }
        private static void SetupDefaultKeybinds()
        {
            SetupAddonDefaultKeybinds();

            SetupDefaultKeybind(Hotkey.EditorPencilTool, new Keybinding(Key.Q));
            SetupDefaultKeybind(Hotkey.EditorLineTool, new Keybinding(Key.W));
            SetupDefaultKeybind(Hotkey.EditorEraserTool, new Keybinding(Key.E));
            SetupDefaultKeybind(Hotkey.EditorSelectTool, new Keybinding(Key.R));
            SetupDefaultKeybind(Hotkey.EditorPanTool, new Keybinding(Key.T));
            SetupDefaultKeybind(Hotkey.EditorToolColor1, new Keybinding(Key.D1));
            SetupDefaultKeybind(Hotkey.EditorToolColor2, new Keybinding(Key.D2));
            SetupDefaultKeybind(Hotkey.EditorToolColor3, new Keybinding(Key.D3));
            SetupDefaultKeybind(Hotkey.EditorToolColor4, new Keybinding(Key.D4));

            SetupDefaultKeybind(Hotkey.EditorCycleToolSetting, new Keybinding(Key.Tab));
            SetupDefaultKeybind(Hotkey.EditorMoveStart, new Keybinding(Key.D));

            SetupDefaultKeybind(Hotkey.EditorRemoveLatestLine, new Keybinding(Key.Backspace));
            SetupDefaultKeybind(Hotkey.EditorFocusStart, new Keybinding(Key.Home));
            SetupDefaultKeybind(Hotkey.EditorFocusLastLine, new Keybinding(Key.End));
            SetupDefaultKeybind(Hotkey.EditorFocusRider, new Keybinding(Key.F1));
            SetupDefaultKeybind(Hotkey.EditorFocusFlag, new Keybinding(Key.F2));
            SetupDefaultKeybind(Hotkey.ToolLifeLock, new Keybinding(KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.ToolAngleLock, new Keybinding(KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolAxisLock, new Keybinding(KeyModifiers.Control | KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolPerpendicularAxisLock, new Keybinding(Key.X, KeyModifiers.Control | KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolLengthLock, new Keybinding(Key.L));
            SetupDefaultKeybind(Hotkey.ToolXYSnap, new Keybinding(Key.X));
            SetupDefaultKeybind(Hotkey.ToolToggleSnap, new Keybinding(Key.S));
            SetupDefaultKeybind(Hotkey.ToolSelectBothJoints, new Keybinding(KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.LineToolFlipLine, new Keybinding(KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.EditorUndo, new Keybinding(Key.Z, KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.EditorRedo,
                new Keybinding(Key.Y, KeyModifiers.Control),
                new Keybinding(Key.Z, KeyModifiers.Control | KeyModifiers.Shift));

            SetupDefaultKeybind(Hotkey.CopyX0, new Keybinding(Key.KeyPad0, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.CopyY0, new Keybinding(Key.KeyPad0, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.CopyX1, new Keybinding(Key.KeyPad1, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.CopyY1, new Keybinding(Key.KeyPad1, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.CopyX2, new Keybinding(Key.KeyPad2, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.CopyY2, new Keybinding(Key.KeyPad2, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.CopyX3, new Keybinding(Key.KeyPad3, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.CopyY3, new Keybinding(Key.KeyPad3, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.CopyX4, new Keybinding(Key.KeyPad4, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.CopyY4, new Keybinding(Key.KeyPad4, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.CopyX5, new Keybinding(Key.KeyPad5, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.CopyY5, new Keybinding(Key.KeyPad5, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.CopyX6, new Keybinding(Key.KeyPad6, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.CopyY6, new Keybinding(Key.KeyPad6, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.CopyX7, new Keybinding(Key.KeyPad7, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.CopyY7, new Keybinding(Key.KeyPad7, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.CopyX8, new Keybinding(Key.KeyPad8, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.CopyY8, new Keybinding(Key.KeyPad8, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.CopyX9, new Keybinding(Key.KeyPad9, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.CopyY9, new Keybinding(Key.KeyPad9, KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.PlaybackStartIgnoreFlag, new Keybinding(Key.Y, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.PlaybackStartGhostFlag, new Keybinding(Key.I, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackStartSlowmo, new Keybinding(Key.Y, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackFlag, new Keybinding(Key.I));
            SetupDefaultKeybind(Hotkey.PlaybackStart, new Keybinding(Key.Y));
            SetupDefaultKeybind(Hotkey.PlaybackStop, new Keybinding(Key.U));
            SetupDefaultKeybind(Hotkey.ToggleSlowmo, new Keybinding(Key.M));
            SetupDefaultKeybind(Hotkey.PlaybackZoom, new Keybinding(Key.Z));
            SetupDefaultKeybind(Hotkey.PlaybackUnzoom, new Keybinding(Key.X));

            SetupDefaultKeybind(Hotkey.PlaybackSpeedUp,
                new Keybinding(Key.Equal),
                new Keybinding(Key.KeyPadAdd));

            SetupDefaultKeybind(Hotkey.PlaybackSpeedDown,
                new Keybinding(Key.Minus),
                new Keybinding(Key.KeyPadSubtract));

            SetupDefaultKeybind(Hotkey.PlaybackFrameNext, new Keybinding(Key.Right));
            SetupDefaultKeybind(Hotkey.PlaybackFramePrev, new Keybinding(Key.Left));
            SetupDefaultKeybind(Hotkey.PlaybackForward, new Keybinding(Key.Right, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackBackward, new Keybinding(Key.Left, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PlaybackIterationNext, new Keybinding(Key.Right, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.PlaybackIterationPrev, new Keybinding(Key.Left, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.PlaybackTogglePause, new Keybinding(Key.Space));

            SetupDefaultKeybind(Hotkey.PreferencesWindow,
                new Keybinding(Key.P, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.GameMenuWindow, new Keybinding(Key.Escape));
            SetupDefaultKeybind(Hotkey.TrackPropertiesWindow, new Keybinding(Key.T, KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.PreferenceAllCheckboxSettings, new Keybinding(Key.O, KeyModifiers.Shift | KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.PreferenceInvisibleRider, new Keybinding(Key.I, KeyModifiers.Shift | KeyModifiers.Alt));

            SetupDefaultKeybind(Hotkey.PreferenceOnionSkinning, new Keybinding(Key.O, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.TogglePreviewMode, new Keybinding(Key.U, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToggleCameraLock, new Keybinding(Key.L, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.LoadWindow, new Keybinding(Key.O));
            SetupDefaultKeybind(Hotkey.Quicksave, new Keybinding(Key.S, KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.EditorQuickPan, new Keybinding(Key.Space, KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.EditorDragCanvas, new Keybinding(MouseButton.Middle));

            SetupDefaultKeybind(Hotkey.EditorCancelTool, new Keybinding(Key.Escape));
            SetupDefaultKeybind(Hotkey.PlaybackResetCamera, new Keybinding(Key.N));
            SetupDefaultKeybind(Hotkey.ToolCopy, new Keybinding(Key.C, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToolCut, new Keybinding(Key.X, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToolPaste, new Keybinding(Key.V, KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToolDelete, new Keybinding(Key.Delete));
            SetupDefaultKeybind(Hotkey.ToolCopyValues, new Keybinding(Key.C, KeyModifiers.Shift | KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToolPasteValues, new Keybinding(Key.V, KeyModifiers.Shift | KeyModifiers.Control));
            SetupDefaultKeybind(Hotkey.ToolSwitchBlue, new Keybinding(Key.D1, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.ToolSwitchRed, new Keybinding(Key.D2, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.ToolSwitchGreen, new Keybinding(Key.D3, KeyModifiers.Alt));
            SetupDefaultKeybind(Hotkey.ToolAddSelection, new Keybinding(KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.ToolToggleSelection, new Keybinding(KeyModifiers.Control));

            SetupDefaultKeybind(Hotkey.ToolScaleAspectRatio, new Keybinding(KeyModifiers.Shift));

            SetupDefaultKeybind(Hotkey.ToolToggleOverlay, new Keybinding(Key.V));

            SetupDefaultKeybind(Hotkey.TriggerMenuWindow, new Keybinding(Key.P));
            SetupDefaultKeybind(Hotkey.SaveAsWindow, new Keybinding(Key.S, KeyModifiers.Control | KeyModifiers.Shift));
            SetupDefaultKeybind(Hotkey.PreferenceDrawDebugCamera, new Keybinding(Key.Period));
            SetupDefaultKeybind(Hotkey.PreferenceDrawDebugGrid, new Keybinding(Key.Comma));
        }
        private static void SetupAddonDefaultKeybinds()
        {
            SetupDefaultKeybind(Hotkey.MagicAnimateAdvanceFrame, new Keybinding(Key.KeyPad0));
            SetupDefaultKeybind(Hotkey.MagicAnimateRecedeFrame, new Keybinding(Key.KeyPad1));
            SetupDefaultKeybind(Hotkey.MagicAnimateRecedeMultiFrame, new Keybinding(Key.KeyPad2));

            SetupDefaultKeybind(Hotkey.LineGeneratorWindow, new Keybinding(Key.G));
        }
        private static void SetupDefaultKeybind(Hotkey hotkey, Keybinding keybinding, Keybinding secondary = null)
        {
            if (keybinding.IsEmpty)
                return;
            DefaultKeybinds[hotkey] = new List<Keybinding>
            {
                keybinding
            };
            if (secondary != null)
            {
                DefaultKeybinds[hotkey].Add(secondary);
            }
        }
        private static void LoadDefaultKeybindings()
        {
            foreach (Hotkey hk in Enum.GetValues(typeof(Hotkey)))
            {
                if (hk == Hotkey.None)
                    continue;
                LoadDefaultKeybind(hk);
            }
        }
        public static List<Keybinding> GetHotkeyDefault(Hotkey hotkey) => !DefaultKeybinds.ContainsKey(hotkey) ? null : DefaultKeybinds[hotkey];
        private static void LoadDefaultKeybind(Hotkey hotkey)
        {
            if (DefaultKeybinds.ContainsKey(hotkey))
            {
                List<Keybinding> defaults = DefaultKeybinds[hotkey];
                if (defaults == null || defaults.Count == 0)
                    return;
                List<Keybinding> list = Keybinds[hotkey];
                if (list.Count == 0)
                    CreateKeybind(hotkey, defaults[0]);
                if (defaults.Count > 1)
                {
                    Keybinding secondary = defaults[1];
                    if (secondary != null && list.Count == 1 && list[0].IsBindingEqual(defaults[0]))
                        CreateKeybind(hotkey, secondary);
                }
            }
        }
        private static void CreateKeybind(Hotkey hotkey, Keybinding keybinding)
        {
            Hotkey conflict = CheckConflicts(keybinding, hotkey);
            if (keybinding.IsEmpty || conflict != Hotkey.None)
                return;
            Keybinds[hotkey].Add(keybinding);
        }
        public static List<Keybinding> FetchBinding(Hotkey hotkey)
        {
            if (!Keybinds.ContainsKey(hotkey))
                Keybinds[hotkey] = new List<Keybinding>();
            List<Keybinding> ret = Keybinds[hotkey];
            if (ret.Count == 0)
                ret.Add(new Keybinding()); // Empty
            return ret;
        }
        public static string HotkeyToString(Hotkey hotkey = Hotkey.None, bool addBrackets = false)
        {
            if (hotkey == Hotkey.None)
                return string.Empty;
            else
            {
                List<Keybinding> keybindings = FetchBinding(hotkey);
                List<string> keys = new List<string>();

                foreach (Keybinding keybinding in keybindings)
                {
                    if (!keybinding.IsEmpty)
                        keys.Add(keybinding.ToString());
                }

                string keysStr = string.Join(", ", keys);

                if (!string.IsNullOrEmpty(keysStr) && addBrackets)
                    keysStr = $" ({keysStr})";

                return keysStr;
            }
        }
        public static Hotkey CheckConflicts(Keybinding keybinding, Hotkey hotkey)
        {
            if (!keybinding.IsEmpty)
            {
                KeyConflicts inputconflicts = KeybindConflicts[hotkey];
                if (inputconflicts == KeyConflicts.HardCoded)
                    return Hotkey.None;
                foreach (KeyValuePair<Hotkey, List<Keybinding>> keybinds in Keybinds)
                {
                    Hotkey hk = keybinds.Key;
                    KeyConflicts conflicts = KeybindConflicts[hk];
                    // If the conflicts is equal to or below inputconflicts
                    // then we can compare for conflict.
                    // If conflicts is above inputconflicts, ignore.
                    // Also, if theyre both hardcoded they cannot conflict.
                    if (inputconflicts.HasFlag(conflicts))
                    {
                        foreach (Keybinding keybind in keybinds.Value)
                        {
                            if (keybind.IsBindingEqual(keybinding) &&
                                !(inputconflicts == KeyConflicts.HardCoded &&
                                  inputconflicts == conflicts))
                                return hk;
                        }
                    }
                }
            }
            return Hotkey.None;
        }
        public static void Load()
        {
            ValidateUserDataFolder();

            string[] lines = File.ReadAllLines(Path.Combine(Local.UserDirPath, Constants.ConfigFileName));

            LoadMainSettings(lines);
            LoadAddonSettings(lines);
            LoadKeybinds(lines);

            PostprocessValues();
        }
        public static void ValidateUserDataFolder()
        {
            Local.UserDirPath = Directory.Exists(Program.UserPortableDirectory) && !File.Exists(Path.Combine(Program.UserPortableDirectory, "TO_BE_DELETED"))
                ? Program.UserPortableDirectory
                : Program.UserDirectory;

            if (!Directory.Exists(Local.UserDirPath))
                Directory.CreateDirectory(Local.UserDirPath);

            if (!File.Exists(Path.Combine(Local.UserDirPath, Constants.ConfigFileName)))
            {
                RestoreDefaultSettings();
                SetupDefaultKeybinds();
                ForceSave();
            }

            TouchUserDir(Constants.RendersFolderName);
            TouchUserDir(Constants.RidersFolderName);
            TouchUserDir(Constants.ScarvesFolderName);
            TouchUserDir(Constants.SongsFolderName);
            TouchUserDir(Constants.TracksFolderName);
        }
        private static void TouchUserDir(string folderName)
        {
            if (!Directory.Exists(Path.Combine(Local.UserDirPath, folderName)))
                _ = Directory.CreateDirectory(Path.Combine(Local.UserDirPath, folderName));
        }
        public static void PostprocessValues()
        {
            if (DefaultSaveFormat == null)
                DefaultSaveFormat = ".trk";

            if (DefaultQuicksaveFormat == null)
                DefaultQuicksaveFormat = ".trk";

            if (DefaultAutosaveFormat == null)
                DefaultAutosaveFormat = ".trk";

            if (DefaultCrashBackupFormat == null)
                DefaultCrashBackupFormat = ".trk";

            if (AutosavePrefix == null)
                AutosavePrefix = "Autosave";

            if (SelectedBoshSkin == null)
                SelectedBoshSkin = Constants.InternalDefaultName;

            if (SelectedScarf == null)
                SelectedScarf = Constants.InternalDefaultName;

            Volume = MathHelper.Clamp(Volume, 0, 100);

            ScarfAmount = Math.Max(1, ScarfAmount);
            ScarfSegmentsPrimary = Math.Max(1, ScarfSegmentsPrimary);
            ScarfSegmentsSecondary = Math.Max(1, ScarfSegmentsSecondary);

            // Allow only odd numbers
            if (ScarfSegmentsPrimary % 2 == 0)
                ScarfSegmentsPrimary++;
            if (ScarfSegmentsSecondary % 2 == 0)
                ScarfSegmentsSecondary++;
        }
        public static void LoadMainSettings(string[] lines)
        {
            Local.Version = GetSetting(lines, nameof(Local.Version));
            LastSelectedTrack = GetSetting(lines, nameof(LastSelectedTrack));
            Enum.TryParse(GetSetting(lines, nameof(PlaybackZoomType)), out PlaybackZoomType);
            LoadFloat(GetSetting(lines, nameof(Volume)), ref Volume);
            LoadFloat(GetSetting(lines, nameof(ScrollSensitivity)), ref ScrollSensitivity);
            LoadBool(GetSetting(lines, nameof(SuperZoom)), ref SuperZoom);
            LoadBool(GetSetting(lines, nameof(NightMode)), ref NightMode);
            LoadBool(GetSetting(lines, nameof(SmoothCamera)), ref SmoothCamera);
            LoadBool(GetSetting(lines, nameof(PredictiveCamera)), ref PredictiveCamera);
            LoadBool(GetSetting(lines, nameof(CheckForUpdates)), ref CheckForUpdates);
            LoadBool(GetSetting(lines, nameof(SmoothPlayback)), ref SmoothPlayback);
            LoadBool(GetSetting(lines, nameof(RoundLegacyCamera)), ref RoundLegacyCamera);

            RecordResolution = GetSetting(lines, nameof(RecordResolution));
            LoadBool(GetSetting(lines, nameof(RecordSmooth)), ref RecordSmooth);
            LoadBool(GetSetting(lines, nameof(RecordMusic)), ref RecordMusic);
            LoadBool(GetSetting(lines, nameof(RecordShowPpf)), ref RecordShowPpf);
            LoadBool(GetSetting(lines, nameof(RecordShowFps)), ref RecordShowFps);
            LoadBool(GetSetting(lines, nameof(RecordShowTools)), ref RecordShowTools);
            LoadBool(GetSetting(lines, nameof(RecordShowHitTest)), ref RecordShowHitTest);
            LoadBool(GetSetting(lines, nameof(RecordShowColorTriggers)), ref RecordShowColorTriggers);
            LoadBool(GetSetting(lines, nameof(RecordResIndependentZoom)), ref RecordResIndependentZoom);

            LoadBool(GetSetting(lines, nameof(ScreenshotLockRatio)), ref ScreenshotLockRatio);
            LoadInt(GetSetting(lines, nameof(ScreenshotWidth)), ref ScreenshotWidth);
            LoadInt(GetSetting(lines, nameof(ScreenshotHeight)), ref ScreenshotHeight);
            LoadBool(GetSetting(lines, nameof(ScreenshotShowPpf)), ref ScreenshotShowPpf);
            LoadBool(GetSetting(lines, nameof(ScreenshotShowFps)), ref ScreenshotShowFps);
            LoadBool(GetSetting(lines, nameof(ScreenshotShowTools)), ref ScreenshotShowTools);
            LoadBool(GetSetting(lines, nameof(ScreenshotShowHitTest)), ref ScreenshotShowHitTest);
            LoadBool(GetSetting(lines, nameof(ScreenshotResIndependentZoom)), ref ScreenshotResIndependentZoom);

            LoadFloat(GetSetting(lines, nameof(UIScale)), ref UIScale);
            LoadBool(GetSetting(lines, nameof(UIShowZoom)), ref UIShowZoom);
            LoadBool(GetSetting(lines, nameof(UIShowSpeedButtons)), ref UIShowSpeedButtons);
            LoadInt(GetSetting(lines, nameof(DefaultTimelineLength)), ref DefaultTimelineLength);
            LoadInt(GetSetting(lines, nameof(DefaultTriggerLength)), ref DefaultTriggerLength);

            LoadBool(GetSetting(lines, nameof(Editor.ShowCoordinateMenu)), ref Editor.ShowCoordinateMenu);
            LoadBool(GetSetting(lines, nameof(Editor.LifeLockNoFakie)), ref Editor.LifeLockNoFakie);
            LoadBool(GetSetting(lines, nameof(Editor.LifeLockNoOrange)), ref Editor.LifeLockNoOrange);
            LoadBool(GetSetting(lines, nameof(LimitLineKnobsSize)), ref LimitLineKnobsSize);
            LoadInt(GetSetting(lines, nameof(SettingsPane)), ref SettingsPane);
            LoadBool(GetSetting(lines, nameof(MuteAudio)), ref MuteAudio);
            LoadBool(GetSetting(lines, nameof(Editor.HitTest)), ref Editor.HitTest);
            LoadBool(GetSetting(lines, nameof(Editor.SnapNewLines)), ref Editor.SnapNewLines);
            LoadBool(GetSetting(lines, nameof(Editor.SnapMoveLine)), ref Editor.SnapMoveLine);
            LoadBool(GetSetting(lines, nameof(Editor.SnapToGrid)), ref Editor.SnapToGrid);
            LoadBool(GetSetting(lines, nameof(Editor.ForceXySnap)), ref Editor.ForceXySnap);
            LoadFloat(GetSetting(lines, nameof(Editor.XySnapDegrees)), ref Editor.XySnapDegrees);
            LoadBool(GetSetting(lines, nameof(Editor.MomentumVectors)), ref Editor.MomentumVectors);
            LoadBool(GetSetting(lines, nameof(Editor.RenderGravityWells)), ref Editor.RenderGravityWells);
            LoadBool(GetSetting(lines, nameof(Editor.DrawContactPoints)), ref Editor.DrawContactPoints);
            LoadBool(GetSetting(lines, nameof(Editor.NoHitSelect)), ref Editor.NoHitSelect);
            LoadBool(GetSetting(lines, nameof(PreviewMode)), ref PreviewMode);
            LoadInt(GetSetting(lines, nameof(SlowmoSpeed)), ref SlowmoSpeed);
            LoadFloat(GetSetting(lines, nameof(DefaultPlayback)), ref DefaultPlayback);
            LoadBool(GetSetting(lines, nameof(ColorPlayback)), ref ColorPlayback);
            LoadBool(GetSetting(lines, nameof(SyncTrackAndSongDuration)), ref SyncTrackAndSongDuration);
            LoadBool(GetSetting(lines, nameof(LockTrackDuration)), ref LockTrackDuration);
            LoadBool(GetSetting(lines, nameof(OnionSkinning)), ref OnionSkinning);
            LoadInt(GetSetting(lines, nameof(PastOnionSkins)), ref PastOnionSkins);
            LoadInt(GetSetting(lines, nameof(FutureOnionSkins)), ref FutureOnionSkins);
            LoadBool(GetSetting(lines, nameof(Editor.ShowLineLength)), ref Editor.ShowLineLength);
            LoadBool(GetSetting(lines, nameof(Editor.ShowLineAngle)), ref Editor.ShowLineAngle);
            LoadBool(GetSetting(lines, nameof(Editor.ShowLineID)), ref Editor.ShowLineID);
            SelectedScarf = GetSetting(lines, nameof(SelectedScarf));
            LoadInt(GetSetting(lines, nameof(ScarfSegmentsPrimary)), ref ScarfSegmentsPrimary);
            SelectedBoshSkin = GetSetting(lines, nameof(SelectedBoshSkin));
            LoadInt(GetSetting(lines, nameof(ScarfSegmentsSecondary)), ref ScarfSegmentsSecondary);
            LoadInt(GetSetting(lines, nameof(ScarfAmount)), ref ScarfAmount);
            LoadInt(GetSetting(lines, nameof(autosaveMinutes)), ref autosaveMinutes);
            LoadInt(GetSetting(lines, nameof(autosaveChanges)), ref autosaveChanges);
            AutosavePrefix = GetSetting(lines, nameof(AutosavePrefix));
            LoadBool(GetSetting(lines, nameof(startWindowMaximized)), ref startWindowMaximized);
            DefaultSaveFormat = GetSetting(lines, nameof(DefaultSaveFormat));
            DefaultAutosaveFormat = GetSetting(lines, nameof(DefaultAutosaveFormat));
            DefaultQuicksaveFormat = GetSetting(lines, nameof(DefaultQuicksaveFormat));
            DefaultCrashBackupFormat = GetSetting(lines, nameof(DefaultCrashBackupFormat));
            LoadBool(GetSetting(lines, nameof(DrawCollisionGrid)), ref DrawCollisionGrid);
            LoadBool(GetSetting(lines, nameof(DrawAGWs)), ref DrawAGWs);
            LoadBool(GetSetting(lines, nameof(DrawFloatGrid)), ref DrawFloatGrid);
            LoadBool(GetSetting(lines, nameof(DrawCamera)), ref DrawCamera);
            LoadFloat(GetSetting(lines, nameof(ZoomMultiplier)), ref ZoomMultiplier);
            LoadColor(GetSetting(lines, nameof(Colors.ExportBg)), ref Colors.ExportBg);
            LoadColor(GetSetting(lines, nameof(Colors.EditorBg)), ref Colors.EditorBg);
            LoadColor(GetSetting(lines, nameof(Colors.EditorNightBg)), ref Colors.EditorNightBg);
            LoadColor(GetSetting(lines, nameof(Colors.ExportLine)), ref Colors.ExportLine);
            LoadColor(GetSetting(lines, nameof(Colors.EditorLine)), ref Colors.EditorLine);
            LoadColor(GetSetting(lines, nameof(Colors.EditorNightLine)), ref Colors.EditorNightLine);
            LoadColor(GetSetting(lines, nameof(Colors.AccelerationLine)), ref Colors.AccelerationLine);
            LoadColor(GetSetting(lines, nameof(Colors.SceneryLine)), ref Colors.SceneryLine);
            LoadColor(GetSetting(lines, nameof(Colors.StandardLine)), ref Colors.StandardLine);
            LoadInt(GetSetting(lines, nameof(Bezier.Resolution)), ref Bezier.Resolution);
            LoadInt(GetSetting(lines, nameof(Bezier.NodeSize)), ref Bezier.NodeSize);
            Enum.TryParse(GetSetting(lines, nameof(Bezier.Mode)), out Bezier.Mode);
            LoadInt(GetSetting(lines, nameof(SmoothPencil.smoothStabilizer)), ref SmoothPencil.smoothStabilizer);
            LoadInt(GetSetting(lines, nameof(SmoothPencil.smoothUpdateSpeed)), ref SmoothPencil.smoothUpdateSpeed);
            LoadBool(GetSetting(lines, nameof(InvisibleRider)), ref InvisibleRider);
        }
        public static void LoadAddonSettings(string[] lines)
        {
            LoadBool(GetSetting(lines, nameof(velocityReferenceFrameAnimation)), ref velocityReferenceFrameAnimation);
            LoadBool(GetSetting(lines, nameof(forwardLinesAsScenery)), ref forwardLinesAsScenery);
            LoadBool(GetSetting(lines, nameof(recededLinesAsScenery)), ref recededLinesAsScenery);
            LoadFloat(GetSetting(lines, nameof(animationRelativeVelX)), ref animationRelativeVelX);
            LoadFloat(GetSetting(lines, nameof(animationRelativeVelY)), ref animationRelativeVelY);
        }
        public static void LoadKeybinds(string[] lines)
        {
            foreach (Hotkey hk in Enum.GetValues(typeof(Hotkey)))
            {
                if (hk == Hotkey.None)
                    continue;
                LoadKeybinding(lines, hk);
            }

            LoadDefaultKeybindings();
        }

        public static void Save() => Debouncer.Debounce("Settings.Save", ForceSave, 1000);

        public static void ForceSave()
        {
            List<string> lines = new List<string>();

            lines.AddRange(BuildMainSettingsList());
            lines.AddRange(BuildAddonSettingsList());
            lines.AddRange(BuildKeybindsList());

            if (!Directory.Exists(Local.UserDirPath))
                Directory.CreateDirectory(Local.UserDirPath);

            string content = string.Join("\r\n", lines);
            File.WriteAllText(Path.Combine(Local.UserDirPath, Constants.ConfigFileName), content);
        }

        private static List<string> BuildMainSettingsList()
        {
            List<string> lines = new List<string>
            {
                MakeSetting(nameof(Local.Version), AssemblyInfo.Version),
                MakeSetting(nameof(LastSelectedTrack), _lastSelectedTrack),
                MakeSetting(nameof(Volume), Volume.ToString(Program.Culture)),
                MakeSetting(nameof(SuperZoom), SuperZoom.ToString(Program.Culture)),
                MakeSetting(nameof(NightMode), NightMode.ToString(Program.Culture)),
                MakeSetting(nameof(SmoothCamera), SmoothCamera.ToString(Program.Culture)),
                MakeSetting(nameof(PredictiveCamera), PredictiveCamera.ToString(Program.Culture)),
                MakeSetting(nameof(CheckForUpdates), CheckForUpdates.ToString(Program.Culture)),
                MakeSetting(nameof(SmoothPlayback), SmoothPlayback.ToString(Program.Culture)),
                //MakeSetting(nameof(PlaybackZoomType), PlaybackZoomType.ToString()),
                MakeSetting(nameof(PlaybackZoomType), ((int)PlaybackZoomType).ToString(Program.Culture)), // Temporarily force int value for backward compatibility
                MakeSetting(nameof(RoundLegacyCamera), RoundLegacyCamera.ToString(Program.Culture)),

                MakeSetting(nameof(RecordResolution), RecordResolution),
                MakeSetting(nameof(RecordSmooth), RecordSmooth.ToString(Program.Culture)),
                MakeSetting(nameof(RecordMusic), RecordMusic.ToString(Program.Culture)),
                MakeSetting(nameof(RecordShowPpf), RecordShowPpf.ToString(Program.Culture)),
                MakeSetting(nameof(RecordShowFps), RecordShowFps.ToString(Program.Culture)),
                MakeSetting(nameof(RecordShowTools), RecordShowTools.ToString(Program.Culture)),
                MakeSetting(nameof(RecordShowHitTest), RecordShowHitTest.ToString(Program.Culture)),
                MakeSetting(nameof(RecordShowColorTriggers), RecordShowColorTriggers.ToString(Program.Culture)),
                MakeSetting(nameof(RecordResIndependentZoom), RecordResIndependentZoom.ToString(Program.Culture)),

                MakeSetting(nameof(ScreenshotLockRatio), ScreenshotLockRatio.ToString(Program.Culture)),
                MakeSetting(nameof(ScreenshotWidth), ScreenshotWidth.ToString(Program.Culture)),
                MakeSetting(nameof(ScreenshotHeight), ScreenshotHeight.ToString(Program.Culture)),
                MakeSetting(nameof(ScreenshotShowPpf), ScreenshotShowPpf.ToString(Program.Culture)),
                MakeSetting(nameof(ScreenshotShowFps), ScreenshotShowFps.ToString(Program.Culture)),
                MakeSetting(nameof(ScreenshotShowTools), ScreenshotShowTools.ToString(Program.Culture)),
                MakeSetting(nameof(ScreenshotShowHitTest), ScreenshotShowHitTest.ToString(Program.Culture)),
                MakeSetting(nameof(ScreenshotResIndependentZoom), ScreenshotResIndependentZoom.ToString(Program.Culture)),

                MakeSetting(nameof(UIScale), UIScale.ToString(Program.Culture)),
                MakeSetting(nameof(UIShowZoom), UIShowZoom.ToString(Program.Culture)),
                MakeSetting(nameof(UIShowSpeedButtons), UIShowSpeedButtons.ToString(Program.Culture)),
                MakeSetting(nameof(DefaultTimelineLength), DefaultTimelineLength.ToString(Program.Culture)),
                MakeSetting(nameof(DefaultTriggerLength), DefaultTriggerLength.ToString(Program.Culture)),

                MakeSetting(nameof(ScrollSensitivity), ScrollSensitivity.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.ShowCoordinateMenu), Editor.ShowCoordinateMenu.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.LifeLockNoFakie), Editor.LifeLockNoFakie.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.LifeLockNoOrange), Editor.LifeLockNoOrange.ToString(Program.Culture)),
                MakeSetting(nameof(LimitLineKnobsSize), LimitLineKnobsSize.ToString(Program.Culture)),
                MakeSetting(nameof(SettingsPane), SettingsPane.ToString(Program.Culture)),
                MakeSetting(nameof(MuteAudio), MuteAudio.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.HitTest), Editor.HitTest.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.SnapNewLines), Editor.SnapNewLines.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.SnapMoveLine), Editor.SnapMoveLine.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.SnapToGrid), Editor.SnapToGrid.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.ForceXySnap), Editor.ForceXySnap.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.XySnapDegrees), Editor.XySnapDegrees.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.MomentumVectors), Editor.MomentumVectors.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.RenderGravityWells), Editor.RenderGravityWells.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.DrawContactPoints), Editor.DrawContactPoints.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.NoHitSelect), Editor.NoHitSelect.ToString(Program.Culture)),
                MakeSetting(nameof(PreviewMode), PreviewMode.ToString(Program.Culture)),
                MakeSetting(nameof(SlowmoSpeed), SlowmoSpeed.ToString(Program.Culture)),
                MakeSetting(nameof(DefaultPlayback), DefaultPlayback.ToString(Program.Culture)),
                MakeSetting(nameof(SyncTrackAndSongDuration), SyncTrackAndSongDuration.ToString(Program.Culture)),
                MakeSetting(nameof(ColorPlayback), ColorPlayback.ToString(Program.Culture)),
                MakeSetting(nameof(LockTrackDuration), LockTrackDuration.ToString(Program.Culture)),
                MakeSetting(nameof(OnionSkinning), OnionSkinning.ToString(Program.Culture)),
                MakeSetting(nameof(PastOnionSkins), PastOnionSkins.ToString(Program.Culture)),
                MakeSetting(nameof(FutureOnionSkins), FutureOnionSkins.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.ShowLineAngle), Editor.ShowLineAngle.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.ShowLineLength), Editor.ShowLineLength.ToString(Program.Culture)),
                MakeSetting(nameof(Editor.ShowLineID), Editor.ShowLineID.ToString(Program.Culture)),
                MakeSetting(nameof(SelectedScarf), SelectedScarf),
                MakeSetting(nameof(SelectedBoshSkin), SelectedBoshSkin),
                MakeSetting(nameof(ScarfAmount), ScarfAmount.ToString(Program.Culture)),
                MakeSetting(nameof(ScarfSegmentsPrimary), ScarfSegmentsPrimary.ToString(Program.Culture)),
                MakeSetting(nameof(ScarfSegmentsSecondary), ScarfSegmentsSecondary.ToString(Program.Culture)),
                MakeSetting(nameof(autosaveChanges), autosaveChanges.ToString(Program.Culture)),
                MakeSetting(nameof(autosaveMinutes), autosaveMinutes.ToString(Program.Culture)),
                MakeSetting(nameof(AutosavePrefix), AutosavePrefix),
                MakeSetting(nameof(startWindowMaximized), startWindowMaximized.ToString(Program.Culture)),
                MakeSetting(nameof(DefaultSaveFormat), DefaultSaveFormat),
                MakeSetting(nameof(DefaultAutosaveFormat), DefaultAutosaveFormat),
                MakeSetting(nameof(DefaultQuicksaveFormat), DefaultQuicksaveFormat),
                MakeSetting(nameof(DefaultCrashBackupFormat), DefaultCrashBackupFormat),
                MakeSetting(nameof(DrawCollisionGrid), DrawCollisionGrid.ToString(Program.Culture)),
                MakeSetting(nameof(DrawAGWs), DrawAGWs.ToString(Program.Culture)),
                MakeSetting(nameof(DrawFloatGrid), DrawFloatGrid.ToString(Program.Culture)),
                MakeSetting(nameof(DrawCamera), DrawCamera.ToString(Program.Culture)),
                MakeSetting(nameof(ZoomMultiplier), ZoomMultiplier.ToString(Program.Culture)),
                MakeSetting(nameof(Colors.ExportBg), SaveColor(Colors.ExportBg)),
                MakeSetting(nameof(Colors.EditorBg), SaveColor(Colors.EditorBg)),
                MakeSetting(nameof(Colors.EditorNightBg), SaveColor(Colors.EditorNightBg)),
                MakeSetting(nameof(Colors.ExportLine), SaveColor(Colors.ExportLine)),
                MakeSetting(nameof(Colors.EditorLine), SaveColor(Colors.EditorLine)),
                MakeSetting(nameof(Colors.EditorNightLine), SaveColor(Colors.EditorNightLine)),
                MakeSetting(nameof(Colors.AccelerationLine), SaveColor(Colors.AccelerationLine)),
                MakeSetting(nameof(Colors.SceneryLine), SaveColor(Colors.SceneryLine)),
                MakeSetting(nameof(Colors.StandardLine), SaveColor(Colors.StandardLine)),
                MakeSetting(nameof(Bezier.Resolution), Bezier.Resolution.ToString(Program.Culture)),
                MakeSetting(nameof(Bezier.NodeSize), Bezier.NodeSize.ToString(Program.Culture)),
                //MakeSetting(nameof(Bezier.Mode), Bezier.Mode.ToString()),
                MakeSetting(nameof(Bezier.Mode), ((int)Bezier.Mode).ToString(Program.Culture)), // Temporarily force int value for backward compatibility
                MakeSetting(nameof(SmoothPencil.smoothStabilizer), SmoothPencil.smoothStabilizer.ToString(Program.Culture)),
                MakeSetting(nameof(SmoothPencil.smoothUpdateSpeed), SmoothPencil.smoothUpdateSpeed.ToString(Program.Culture)),
                MakeSetting(nameof(InvisibleRider), InvisibleRider.ToString(Program.Culture)),
            };

            return lines;
        }
        private static List<string> BuildAddonSettingsList()
        {
            List<string> lines = new List<string>
            {
                MakeSetting(nameof(velocityReferenceFrameAnimation), velocityReferenceFrameAnimation.ToString()),
                MakeSetting(nameof(forwardLinesAsScenery), forwardLinesAsScenery.ToString()),
                MakeSetting(nameof(recededLinesAsScenery), recededLinesAsScenery.ToString()),
                MakeSetting(nameof(animationRelativeVelX), animationRelativeVelX.ToString()),
                MakeSetting(nameof(animationRelativeVelY), animationRelativeVelY.ToString()),
            };

            return lines;
        }
        private static List<string> BuildKeybindsList()
        {
            List<string> lines = new List<string>();

            foreach (KeyValuePair<Hotkey, List<Keybinding>> binds in Keybinds)
            {
                foreach (Keybinding bind in binds.Value)
                {
                    if (KeybindConflicts[binds.Key] == KeyConflicts.HardCoded)
                        continue;
                    if (!bind.IsEmpty)
                    {
                        string keybind = "";
                        if (bind.UsesModifiers)
                            keybind += bind.Modifiers.ToString();
                        if (bind.UsesKeys)
                        {
                            if (keybind.Length > 0)
                                keybind += "+";
                            keybind += bind.Key.ToString();
                        }
                        if (bind.UsesMouse)
                        {
                            if (keybind.Length > 0)
                                keybind += "+";
                            keybind += bind.MouseButton.ToString();
                        }

                        lines.Add(MakeSetting(binds.Key.ToString(), $"[{keybind}]"));
                    }
                }
            }

            return lines;
        }

        private static void LoadKeybinding(string[] config, Hotkey hotkey)
        {
            if (KeybindConflicts[hotkey] == KeyConflicts.HardCoded)
                return;
            int line = 0;
            string hotkeyname = hotkey.ToString();
            string setting = GetSetting(config, hotkeyname, ref line);
            if (setting != null)
                Keybinds[hotkey] = new List<Keybinding>();
            while (setting != null)
            {
                line++;
                string[] items = setting.Trim(' ', '\t', '[', ']').Split('+');
                Keybinding ret = new Keybinding();
                foreach (string item in items)
                {
                    if (!ret.UsesModifiers &&
                        Enum.TryParse(item, true, out KeyModifiers modifiers))
                    {
                        ret.Modifiers = modifiers;
                    }
                    else if (!ret.UsesKeys &&
                        Enum.TryParse(item, true, out Key key))
                    {
                        ret.Key = key;
                    }
                    else if (!ret.UsesMouse &&
                        Enum.TryParse(item, true, out MouseButton mouse))
                    {
                        ret.MouseButton = mouse;
                    }
                }

                try
                {
                    if (!ret.IsEmpty)
                        CreateKeybind(hotkey, ret);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"An error occured loading the hotkey {hotkey}\n{e}");
                }
                setting = GetSetting(config, hotkeyname, ref line);
            }
        }
        private static string GetSetting(string[] config, string name)
        {
            int start = 0;
            return GetSetting(config, name, ref start);
        }
        private static string GetSetting(string[] config, string name, ref int start)
        {
            for (int i = start; i < config.Length; i++)
            {
                int idx = config[i].IndexOf("=");
                if (idx != -1 && idx + 1 < config[i].Length && config[i].Substring(0, idx) == name)//split[0] == name && split.Length > 1)
                {

                    string split = config[i].Substring(idx + 1);
                    start = i;
                    return split;
                }
            }
            return null;
        }
        private static string MakeSetting(string name, string value) => name + "=" + value;
        private static void LoadInt(string setting, ref int var)
        {
            if (int.TryParse(setting, System.Globalization.NumberStyles.Integer, Program.Culture, out int val))
                var = val;
        }
        private static void LoadFloat(string setting, ref float var)
        {
            if (float.TryParse(setting, System.Globalization.NumberStyles.Float, Program.Culture, out float val))
                var = val;
        }
        private static void LoadBool(string setting, ref bool var)
        {
            if (bool.TryParse(setting, out bool val))
                var = val;
        }
        private static void LoadColor(string setting, ref Color var)
        {
            if (setting != null)
            {
                int[] vals = setting.Split(',').Select(int.Parse).ToArray();
                var = Color.FromArgb(vals[0], vals[1], vals[2]);
            }
        }

        private static string SaveColor(Color color)
        {
            int[] colorValues = { color.R, color.G, color.B };
            return string.Join(",", colorValues);
        }
    }
}
