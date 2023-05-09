using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms.VisualStyles;
using Gwen;
using Gwen.Controls;
using linerider.Game;
using linerider.LRL;
using linerider.Tools;
using linerider.Utils;
using OpenTK;
using OpenTK.Input;

namespace linerider.UI
{
    public class PreferencesWindow : DialogBase
    {
        private CollapsibleList _prefcontainer;
        private ControlBase _focus;
        private int _tabscount = 0;
        public PreferencesWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = "Preferences";
            SetSize(550, 500);
            MinimumSize = Size;
            ControlBase bottom = new ControlBase(this)
            {
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
            };
            Button defaults = new Button(bottom)
            {
                Dock = Dock.Right,
                Margin = new Margin(0, 2, 0, 0),
                Text = "Restore Defaults"
            };
            defaults.Clicked += (o, e) => RestoreDefaults();
            _prefcontainer = new CollapsibleList(this)
            {
                Dock = Dock.Left,
                AutoSizeToContents = false,
                Width = 100,
                Margin = new Margin(0, 0, 5, 0)
            };
            MakeModal(true);
            Setup();
        }
        private void RestoreDefaults()
        {
            var mbox = MessageBox.Show(
                _canvas,
                "Are you sure? This cannot be undone.", "Restore Defaults",
                MessageBox.ButtonType.OkCancel,
                true);
            mbox.RenameButtons("Restore");
            mbox.Dismissed += (o, e) =>
            {
                if (e == DialogResult.OK)
                {
                    Settings.RestoreDefaultSettings();
                    Settings.Save();
                    _editor.InitCamera();
                    Close();// this is lazy, but i don't want to update the ui
                }
            };
        }
        private void PopulateAudio(ControlBase parent)
        {
            var opts = GwenHelper.CreateHeaderPanel(parent, "Sync options");
            var syncenabled = GwenHelper.AddCheckbox(opts, "Mute", Settings.MuteAudio, (o, e) =>
            {
                Settings.MuteAudio = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            HorizontalSlider vol = new HorizontalSlider(null)
            {
                Min = 0,
                Max = 100,
                Value = Settings.Volume,
                Width = 80,
            };
            vol.ValueChanged += (o, e) =>
            {
                Settings.Volume = (float)vol.Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(opts, "Volume", vol);
            vol.Width = 200;
        }
        private void PopulateKeybinds(ControlBase parent)
        {
            var hk = new HotkeyWidget(parent);
        }
        private void PopulateModes(ControlBase parent)
        {
            var background = GwenHelper.CreateHeaderPanel(parent, "Background Color");
            GwenHelper.AddCheckbox(background, "Night Mode", Settings.NightMode, (o, e) =>
            {
                Settings.NightMode = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var whitebg = GwenHelper.AddCheckbox(background, "Pure White Background", Settings.WhiteBG, (o, e) =>
            {
                Settings.WhiteBG = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var panelgeneral = GwenHelper.CreateHeaderPanel(parent, "General");
            var superzoom = GwenHelper.AddCheckbox(panelgeneral, "Superzoom", Settings.SuperZoom, (o, e) =>
            {
                Settings.SuperZoom = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            ComboBox scroll = GwenHelper.CreateLabeledCombobox(panelgeneral, "Scroll Sensitivity:");
            scroll.Margin = new Margin(0, 0, 0, 0);
            scroll.Dock = Dock.Bottom;
            scroll.AddItem("0.25x").Name = "0.25";
            scroll.AddItem("0.5x").Name = "0.5";
            scroll.AddItem("0.75x").Name = "0.75";
            scroll.AddItem("1x").Name = "1";
            scroll.AddItem("2x").Name = "2";
            scroll.AddItem("3x").Name = "3";
            scroll.SelectByName("1");//default if user setting fails.
            scroll.SelectByName(Settings.ScrollSensitivity.ToString(Program.Culture));
            scroll.ItemSelected += (o, e) =>
            {
                if (e.SelectedItem != null)
                {
                    Settings.ScrollSensitivity = float.Parse(e.SelectedItem.Name, Program.Culture);
                    Settings.Save();
                }
            };
            superzoom.Tooltip = "Allows the user to zoom in\nnearly 10x more than usual.";

            var invisibleRider = GwenHelper.AddCheckbox(panelgeneral, "Invisible Rider", Settings.InvisibleRider, (o, e) =>
            {
                Settings.InvisibleRider = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
        }

        private void PopulateLines(ControlBase parent)
        {
            Panel lineColors = GwenHelper.CreateHeaderPanel(parent, "Line Color Customization");
            var defaultRed = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.DefaultLine.R
            };
            defaultRed.ValueChanged += (o, e) =>
            {
                Settings.Lines.DefaultLine = Color.FromArgb((int)((Spinner)o).Value, Settings.Lines.DefaultLine.G, Settings.Lines.DefaultLine.B);
                Settings.Save();
            };

            var defaultGreen = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.DefaultLine.G
            };
            defaultGreen.ValueChanged += (o, e) =>
            {
                Settings.Lines.DefaultLine = Color.FromArgb(Settings.Lines.DefaultLine.R, (int)((Spinner)o).Value, Settings.Lines.DefaultLine.B);
                Settings.Save();
            };

            var defaultBlue = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.DefaultLine.B
            };
            defaultBlue.ValueChanged += (o, e) =>
            {
                Settings.Lines.DefaultLine = Color.FromArgb(Settings.Lines.DefaultLine.R, Settings.Lines.DefaultLine.G, (int)((Spinner)o).Value);
                Settings.Save();
            };

            GwenHelper.CreateLabeledControl(parent, "Default line color (R,G,B)", new ControlBase[3] { defaultRed, defaultGreen, defaultBlue });

            var nightRed = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.DefaultNightLine.R
            };
            nightRed.ValueChanged += (o, e) =>
            {
                Settings.Lines.DefaultNightLine = Color.FromArgb((int)((Spinner)o).Value, Settings.Lines.DefaultNightLine.G, Settings.Lines.DefaultNightLine.B);
                Settings.Save();
            };

            var nightGreen = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.DefaultNightLine.G
            };
            nightGreen.ValueChanged += (o, e) =>
            {
                Settings.Lines.DefaultNightLine = Color.FromArgb(Settings.Lines.DefaultNightLine.R, (int)((Spinner)o).Value, Settings.Lines.DefaultNightLine.B);
                Settings.Save();
            };

            var nightBlue = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.DefaultNightLine.B
            };
            nightBlue.ValueChanged += (o, e) =>
            {
                Settings.Lines.DefaultNightLine = Color.FromArgb(Settings.Lines.DefaultNightLine.R, Settings.Lines.DefaultNightLine.G, (int)((Spinner)o).Value);
                Settings.Save();
            };

            GwenHelper.CreateLabeledControl(parent, "Night line color (R,G,B)", new ControlBase[3] { nightRed, nightGreen, nightBlue });

            var accelRed = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.AccelerationLine.R
            };
            accelRed.ValueChanged += (o, e) =>
            {
                Settings.Lines.AccelerationLine = Color.FromArgb((int)((Spinner)o).Value, Settings.Lines.AccelerationLine.G, Settings.Lines.AccelerationLine.B);
                SaveLines();
            };

            var accelGreen = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.AccelerationLine.G
            };
            accelGreen.ValueChanged += (o, e) =>
            {
                Settings.Lines.AccelerationLine = Color.FromArgb(Settings.Lines.AccelerationLine.R, (int)((Spinner)o).Value, Settings.Lines.AccelerationLine.B);
                SaveLines();
            };

            var accelBlue = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.AccelerationLine.B
            };
            accelBlue.ValueChanged += (o, e) =>
            {
                Settings.Lines.AccelerationLine = Color.FromArgb(Settings.Lines.AccelerationLine.R, Settings.Lines.AccelerationLine.G, (int)((Spinner)o).Value);
                SaveLines();
            };

            GwenHelper.CreateLabeledControl(parent, "Acceleration line color (R,G,B)", new ControlBase[3] { accelRed, accelGreen, accelBlue });

            var sceneryRed = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.SceneryLine.R
            };
            sceneryRed.ValueChanged += (o, e) =>
            {
                Settings.Lines.SceneryLine = Color.FromArgb((int)((Spinner)o).Value, Settings.Lines.SceneryLine.G, Settings.Lines.SceneryLine.B);
                Settings.Save();
            };

            var sceneryGreen = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.SceneryLine.G
            };
            sceneryGreen.ValueChanged += (o, e) =>
            {
                Settings.Lines.SceneryLine = Color.FromArgb(Settings.Lines.SceneryLine.R, (int)((Spinner)o).Value, Settings.Lines.SceneryLine.B);
                Settings.Save();
            };

            var sceneryBlue = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.SceneryLine.B
            };
            sceneryBlue.ValueChanged += (o, e) =>
            {
                Settings.Lines.SceneryLine = Color.FromArgb(Settings.Lines.SceneryLine.R, Settings.Lines.SceneryLine.G, (int)((Spinner)o).Value);
                Settings.Save();
            };

            GwenHelper.CreateLabeledControl(parent, "Scenery line color (R,G,B)", new ControlBase[3] { sceneryRed, sceneryGreen, sceneryBlue });

            var standardRed = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.StandardLine.R
            };
            standardRed.ValueChanged += (o, e) =>
            {
                Settings.Lines.StandardLine = Color.FromArgb((int)((Spinner)o).Value, Settings.Lines.StandardLine.G, Settings.Lines.StandardLine.B);
                SaveLines();
            };

            var standardGreen = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.StandardLine.G
            };
            standardGreen.ValueChanged += (o, e) =>
            {
                Settings.Lines.StandardLine = Color.FromArgb(Settings.Lines.StandardLine.R, (int)((Spinner)o).Value, Settings.Lines.StandardLine.B);
                SaveLines();
            };

            var standardBlue = new Spinner(lineColors)
            {
                Min = 0,
                Max = 255,
                Value = Settings.Lines.StandardLine.B
            };
            standardBlue.ValueChanged += (o, e) =>
            {
                Settings.Lines.StandardLine = Color.FromArgb(Settings.Lines.StandardLine.R, Settings.Lines.StandardLine.G, (int)((Spinner)o).Value);
                SaveLines();
            };

            GwenHelper.CreateLabeledControl(parent, "Standard line color (R,G,B)", new ControlBase[3] { standardRed, standardGreen, standardBlue });

        }

        private void SaveLines()
        {
            Settings.Save();
            linerider.Rendering.GameRenderer.Game.Track.RedrawAllLines();
        }

        private void PopulateCamera(ControlBase parent)
        {
            var camtype = GwenHelper.CreateHeaderPanel(parent, "Camera Type");
            var camprops = GwenHelper.CreateHeaderPanel(parent, "Camera Properties");
            RadioButtonGroup rbcamera = new RadioButtonGroup(camtype)
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false,
            };
            var soft = rbcamera.AddOption("Soft Camera");
            var predictive = rbcamera.AddOption("Predictive Camera");
            var legacy = rbcamera.AddOption("Legacy Camera");
            var round = GwenHelper.AddCheckbox(camprops, "Round Legacy Camera", Settings.RoundLegacyCamera, (o, e) =>
            {
                Settings.RoundLegacyCamera = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            if (Settings.SmoothCamera)
            {
                if (Settings.PredictiveCamera)
                    predictive.Select();
                else
                    soft.Select();
            }
            else
            {
                legacy.Select();
            }
            soft.Checked += (o, e) =>
            {
                Settings.SmoothCamera = true;
                Settings.PredictiveCamera = false;
                Settings.Save();
                round.IsDisabled = Settings.SmoothCamera;
                _editor.InitCamera();
            };
            predictive.Checked += (o, e) =>
            {
                Settings.SmoothCamera = true;
                Settings.PredictiveCamera = true;
                Settings.Save();
                round.IsDisabled = Settings.SmoothCamera;
                _editor.InitCamera();
            };
            legacy.Checked += (o, e) =>
            {
                Settings.SmoothCamera = false;
                Settings.PredictiveCamera = false;
                Settings.Save();
                round.IsDisabled = Settings.SmoothCamera;
                _editor.InitCamera();
            };
            predictive.Tooltip = "This is the camera that was added in 1.03\nIt moves relative to the future of the track";
        }
        private void PopulateEditor(ControlBase parent)
        {
            Panel advancedtools = GwenHelper.CreateHeaderPanel(parent, "Advanced Visualization");

            var contact = GwenHelper.AddCheckbox(advancedtools, "Contact Points", Settings.Editor.DrawContactPoints, (o, e) =>
            {
                Settings.Editor.DrawContactPoints = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var momentum = GwenHelper.AddCheckbox(advancedtools, "Momentum Vectors", Settings.Editor.MomentumVectors, (o, e) =>
            {
                Settings.Editor.MomentumVectors = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var hitbox = GwenHelper.AddCheckbox(advancedtools, "Line Hitbox", Settings.Editor.RenderGravityWells, (o, e) =>
            {
                Settings.Editor.RenderGravityWells = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var hittest = GwenHelper.AddCheckbox(advancedtools, "Hit Test", Settings.Editor.HitTest, (o, e) =>
            {
                Settings.Editor.HitTest = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var onion = GwenHelper.AddCheckbox(advancedtools, "Onion Skinning", Settings.OnionSkinning, (o, e) =>
            {
                Settings.OnionSkinning = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var pastOnionSkins = new Spinner(advancedtools)
            {
                Min = 0,
                Max = 1000,
                Value = Settings.PastOnionSkins,
            };
            pastOnionSkins.ValueChanged += (o, e) =>
            {
                Settings.PastOnionSkins = (int)((Spinner)o).Value;
                Settings.Save();
            };
            var futureOnionSkins = new Spinner(advancedtools)
            {
                Min = 0,
                Max = 1000,
                Value = Settings.FutureOnionSkins,
            };
            futureOnionSkins.ValueChanged += (o, e) =>
            {
                Settings.FutureOnionSkins = (int)((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(advancedtools, "#Onion Skins (Before, After)", new ControlBase[2] { pastOnionSkins, futureOnionSkins });
            var drawagws = GwenHelper.AddCheckbox(advancedtools, "Line Extensions (AGWs)", Settings.DrawAGWs, (o, e) =>
            {
                Settings.DrawAGWs = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var drawgrid = GwenHelper.AddCheckbox(advancedtools, "Simulation Grid", Settings.DrawCollisionGrid, (o, e) =>
            {
                Settings.DrawCollisionGrid = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var drawfloatgrid = GwenHelper.AddCheckbox(advancedtools, "Floating-point grid", Settings.DrawFloatGrid, (o, e) =>
            {
                Settings.DrawFloatGrid = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var drawcam = GwenHelper.AddCheckbox(advancedtools, "Camera", Settings.DrawCamera, (o, e) =>
            {
                Settings.DrawCamera = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var coordmenu = GwenHelper.AddCheckbox(advancedtools, "Show Coordinate Menu", Settings.Editor.ShowCoordinateMenu, (o, e) =>
            {
                Settings.Editor.ShowCoordinateMenu = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Panel pblifelock = GwenHelper.CreateHeaderPanel(parent, "Lifelock Conditions");
            GwenHelper.AddCheckbox(pblifelock, "Next frame constraints", Settings.Editor.LifeLockNoOrange, (o, e) =>
            {
                Settings.Editor.LifeLockNoOrange = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            GwenHelper.AddCheckbox(pblifelock, "No Fakie Death", Settings.Editor.LifeLockNoFakie, (o, e) =>
            {
                Settings.Editor.LifeLockNoFakie = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            var overlay = GwenHelper.CreateHeaderPanel(parent, "Frame Overlay");
            PopulateOverlay(overlay);

            onion.Tooltip = "Visualize the rider before/after\nthe current frame.";
            momentum.Tooltip = "Visualize the direction of\nmomentum for each contact point";
            contact.Tooltip = "Visualize the parts of the rider\nthat interact with lines.";
            hitbox.Tooltip = "Visualizes the hitbox of lines\nUsed for advanced editing";
            hittest.Tooltip = "Lines that have been hit by\nthe rider will glow.";
        }
        private void PopulatePlayback(ControlBase parent)
        {
            var playbackzoom = GwenHelper.CreateHeaderPanel(parent, "Playback Zoom");
            RadioButtonGroup pbzoom = new RadioButtonGroup(playbackzoom)
            {
                Dock = Dock.Left,
                ShouldDrawBackground = false,
            };
            pbzoom.AddOption("Default Zoom");
            pbzoom.AddOption("Current Zoom");
            pbzoom.AddOption("Specific Zoom");
            Spinner playbackspinner = new Spinner(pbzoom)
            {
                Dock = Dock.Bottom,
                Max = 24,
                Min = 1,
            };
            pbzoom.SelectionChanged += (o, e) =>
            {
                Settings.PlaybackZoomType = ((RadioButtonGroup)o).SelectedIndex;
                Settings.Save();
                playbackspinner.IsHidden = (((RadioButtonGroup)o).SelectedLabel != "Specific Zoom");
            };
            playbackspinner.ValueChanged += (o, e) =>
            {
                Settings.PlaybackZoomValue = (float)((Spinner)o).Value;
                Settings.Save();
            };
            pbzoom.SetSelection(Settings.PlaybackZoomType);
            playbackspinner.Value = Settings.PlaybackZoomValue;

            var zoomMultiplier = new Spinner(pbzoom)
            {
                Min = 0.01,
                Max = 100.0,
                Value = Settings.ZoomMultiplier,
                IncrementSize = 0.1
            };
            zoomMultiplier.ValueChanged += (o, e) =>
            {
                Settings.ZoomMultiplier = (float)((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(pbzoom, "Zoom Multiplier", zoomMultiplier);

            var playbackmode = GwenHelper.CreateHeaderPanel(parent, "Playback Color");
            GwenHelper.AddCheckbox(playbackmode, "Color Playback", Settings.ColorPlayback, (o, e) =>
            {
                Settings.ColorPlayback = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var preview = GwenHelper.AddCheckbox(playbackmode, "Preview Mode", Settings.PreviewMode, (o, e) =>
            {
                Settings.PreviewMode = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var recording = GwenHelper.AddCheckbox(playbackmode, "Recording Mode", Settings.Local.RecordingMode, (o, e) =>
            {
                Settings.Local.RecordingMode = ((Checkbox)o).IsChecked;
            });
            var framerate = GwenHelper.CreateHeaderPanel(parent, "Frame Control");
            var smooth = GwenHelper.AddCheckbox(framerate, "Smooth Playback", Settings.SmoothPlayback, (o, e) =>
            {
                Settings.SmoothPlayback = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            ComboBox pbrate = GwenHelper.CreateLabeledCombobox(framerate, "Playback Rate:");
            for (var i = 0; i < Constants.MotionArray.Length; i++)
            {
                var f = (Constants.MotionArray[i] / (float)Constants.PhysicsRate);
                pbrate.AddItem(f + "x", f.ToString(CultureInfo.InvariantCulture), f);
            }
            pbrate.SelectByName(Settings.DefaultPlayback.ToString(CultureInfo.InvariantCulture));
            pbrate.ItemSelected += (o, e) =>
            {
                Settings.DefaultPlayback = (float)e.SelectedItem.UserData;
                Settings.Save();
            };
            var cbslowmo = GwenHelper.CreateLabeledCombobox(framerate, "Slowmo FPS:");
            var fpsarray = new[] { 1, 2, 5, 10, 20 };
            for (var i = 0; i < fpsarray.Length; i++)
            {
                cbslowmo.AddItem(fpsarray[i].ToString(), fpsarray[i].ToString(CultureInfo.InvariantCulture),
                    fpsarray[i]);
            }
            cbslowmo.SelectByName(Settings.SlowmoSpeed.ToString(CultureInfo.InvariantCulture));
            cbslowmo.ItemSelected += (o, e) =>
            {
                Settings.SlowmoSpeed = (int)e.SelectedItem.UserData;
                Settings.Save();
            };
            smooth.Tooltip = "Interpolates frames from the base\nphysics rate of 40 frames/second\nup to 60 frames/second";
        }
        private void PopulateOverlay(ControlBase parent)
        {
            var offset = new Spinner(null)
            {
                Min = -999,
                Max = 999,
                Value = Settings.Local.TrackOverlayOffset,
            };
            offset.ValueChanged += (o, e) =>
            {
                Settings.Local.TrackOverlayOffset = (int)offset.Value;
            };
            var fixedspinner = new Spinner(null)
            {
                Min = 0,
                Max = _editor.FrameCount,
                Value = Settings.Local.TrackOverlayFixedFrame,
            };
            fixedspinner.ValueChanged += (o, e) =>
            {
                Settings.Local.TrackOverlayFixedFrame = (int)fixedspinner.Value;
            };
            void updatedisabled()
            {
                offset.IsDisabled = Settings.Local.TrackOverlayFixed;
                fixedspinner.IsDisabled = !Settings.Local.TrackOverlayFixed;
            }
            var enabled = GwenHelper.AddCheckbox(parent, "Enabled", Settings.Local.TrackOverlay, (o, e) =>
            {
                Settings.Local.TrackOverlay = ((Checkbox)o).IsChecked;
                updatedisabled();
            });
            GwenHelper.AddCheckbox(parent, "Fixed Frame", Settings.Local.TrackOverlayFixed, (o, e) =>
            {
                Settings.Local.TrackOverlayFixed = ((Checkbox)o).IsChecked;
                updatedisabled();
            });
            GwenHelper.CreateLabeledControl(parent, "Frame Offset", offset);
            GwenHelper.CreateLabeledControl(parent, "Frame ID", fixedspinner);
            updatedisabled();
            enabled.Tooltip = "Display an onion skin of the track\nat a specified offset for animation";
        }
        private void PopulateTools(ControlBase parent)
        {
            var bezier = GwenHelper.CreateHeaderPanel(parent, "Bezier Tool");

            var resolution = new Spinner(bezier)
            {
                Min = 5,
                Max = 100,
                Value = Settings.Bezier.Resolution,
                IncrementSize = 1
            };
            resolution.ValueChanged += (o, e) =>
            {
                Settings.Bezier.Resolution = (int) ((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(bezier, "Resolution (Lines per 100 pixels)", resolution);

            var nodeSize = new Spinner(bezier)
            {
                Min = 5,
                Max = 100,
                Value = Settings.Bezier.NodeSize,
                IncrementSize = 1
            };
            nodeSize.ValueChanged += (o, e) =>
            {
                Settings.Bezier.NodeSize = (int)((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(bezier, "Size of the bezier curve nodes", nodeSize);

            var bezierModeSelector = new RadioButtonGroup(bezier)
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false
            };
            var directType = bezierModeSelector.AddOption("Direct Visualization Mode");
            var traceType = bezierModeSelector.AddOption("Trace Visualization Mode");
            switch ((Settings.BezierMode) Settings.Bezier.Mode)
            {
                case Settings.BezierMode.Direct:
                    directType.Select();
                    break;
                case Settings.BezierMode.Trace:
                    traceType.Select();
                    break;
            }
            directType.CheckChanged += (o, e) =>
            {
                Settings.Bezier.Mode = (int)Settings.BezierMode.Direct;
                Settings.Save();
            };
            traceType.CheckChanged += (o, e) =>
            {
                Settings.Bezier.Mode = (int)Settings.BezierMode.Trace;
                Settings.Save();
            };

            var smpen = GwenHelper.CreateHeaderPanel(parent, "Smooth Pencil Configuration");
            var smUpdateSpeed = new Spinner(smpen)
            {
                Min = 0,
                Max = 1000,
                Value = Settings.SmoothPencil.smoothUpdateSpeed,
            };
            smUpdateSpeed.ValueChanged += (o, e) =>
            {
                Settings.SmoothPencil.smoothUpdateSpeed = (int)smUpdateSpeed.Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(smpen, "Update Speed in Milliseconds", smUpdateSpeed);
            smUpdateSpeed.Tooltip = "Determines how often the lines are dragged in milliseconds\nLeave at 0 to update as fast as your framerate allows";
            var smStabilizer = new Spinner(smpen)
            {
                Min = 1,
                Max = 24,
                Value = Settings.SmoothPencil.smoothStabilizer,
            };
            smStabilizer.ValueChanged += (o, e) =>
            {
                Settings.SmoothPencil.smoothStabilizer = (int)smStabilizer.Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(smpen, "Stabilizer", smStabilizer);
            smStabilizer.Tooltip = "Determines by how much your lines are dragged behind";

            var select = GwenHelper.CreateHeaderPanel(parent, "Select Tool -- Line Info");
            var length = GwenHelper.AddCheckbox(select, "Show Length", Settings.Editor.ShowLineLength, (o, e) =>
            {
                Settings.Editor.ShowLineLength = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var angle = GwenHelper.AddCheckbox(select, "Show Angle", Settings.Editor.ShowLineAngle, (o, e) =>
            {
                Settings.Editor.ShowLineAngle = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var showid = GwenHelper.AddCheckbox(select, "Show ID", Settings.Editor.ShowLineID, (o, e) =>
            {
                Settings.Editor.ShowLineID = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Panel panelSnap = GwenHelper.CreateHeaderPanel(parent, "Snapping");
            var linesnap = GwenHelper.AddCheckbox(panelSnap, "Snap New Lines", Settings.Editor.SnapNewLines, (o, e) =>
            {
                Settings.Editor.SnapNewLines = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var movelinesnap = GwenHelper.AddCheckbox(panelSnap, "Snap Line Movement", Settings.Editor.SnapMoveLine, (o, e) =>
            {
                Settings.Editor.SnapMoveLine = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var gridsnap = GwenHelper.AddCheckbox(panelSnap, "Snap to displayed grids", Settings.Editor.SnapToGrid, (o, e) =>
            {
                Settings.Editor.SnapToGrid = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var forcesnap = GwenHelper.AddCheckbox(panelSnap, "Force X/Y snap", Settings.Editor.ForceXySnap, (o, e) =>
            {
                Settings.Editor.ForceXySnap = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            var snapAngle = new Spinner(panelSnap)
            {
                Min = 0,
                Max = 180,
                Value = Settings.Editor.XySnapDegrees,
                IncrementSize = 1
            };
            snapAngle.ValueChanged += (o, e) =>
            {
                Settings.Editor.XySnapDegrees = (float)Math.Round((float)((Spinner)o).Value, 2, MidpointRounding.AwayFromZero);
                ((Spinner)o).Value = Settings.Editor.XySnapDegrees;  // Re-display the rounded value
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(panelSnap, "X/Y snap degrees", snapAngle);
            forcesnap.Tooltip = "Forces all lines drawn to\nsnap to multiples of a chosen angle";
            movelinesnap.Tooltip = "Snap to lines when using the\nselect tool to move a single line";
        }
        private void PopulateOther(ControlBase parent)
        {
            var updates = GwenHelper.CreateHeaderPanel(parent, "Updates");

            var showid = GwenHelper.AddCheckbox(updates, "Check For Updates", Settings.CheckForUpdates, (o, e) =>
            {
                Settings.CheckForUpdates = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            var showChangelog = GwenHelper.AddCheckbox(updates, "Enable Changelog", Settings.showChangelog, (o, e) =>
            {
                Settings.showChangelog = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            var mainWindowSettings = GwenHelper.CreateHeaderPanel(parent, "Window Launch Size");
            var mainWindowWidth = new Spinner(mainWindowSettings)
            {
                Min = 1,
                Max = int.MaxValue - 1,
                Value = Settings.mainWindowWidth,
            };
            mainWindowWidth.ValueChanged += (o, e) =>
            {
                Settings.mainWindowWidth = (int)((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(mainWindowSettings, "Main Window Width (Current: " + (Program.GetWindowWidth()) + ")", mainWindowWidth);
            var mainWindowHeight = new Spinner(mainWindowSettings)
            {
                Min = 1,
                Max = int.MaxValue - 1,
                Value = Settings.mainWindowHeight,
            };
            mainWindowHeight.ValueChanged += (o, e) =>
            {
                Settings.mainWindowHeight = (int)((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(mainWindowSettings, "Main Window Height (Current: " + (Program.GetWindowHeight()) + ")", mainWindowHeight);



            var saveSettings = GwenHelper.CreateHeaderPanel(parent, "Saves");
            var autosaveMinutes = new Spinner(saveSettings)
            {
                Min = 1,
                Max = int.MaxValue - 1,
                Value = Settings.autosaveMinutes,
            };
            autosaveMinutes.ValueChanged += (o, e) =>
            {
                Settings.autosaveMinutes = (int)((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(saveSettings, "Minutes between autosaves", autosaveMinutes);

            var autosaveChanges = new Spinner(saveSettings)
            {
                Min = 1,
                Max = int.MaxValue - 1,
                Value = Settings.autosaveChanges,
            };
            autosaveChanges.ValueChanged += (o, e) =>
            {
                Settings.autosaveChanges = (int)((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(saveSettings, "Min changes to start autosaving", autosaveChanges);

            ComboBox defaultSaveType = GwenHelper.CreateLabeledCombobox(saveSettings, "Default Save As Format:");
            defaultSaveType.AddItem(".trk", "", ".trk");
            defaultSaveType.AddItem(".json", "", ".json");
            defaultSaveType.AddItem(".sol", "", ".sol");
            defaultSaveType.ItemSelected += (o, e) =>
            {
                Settings.DefaultSaveFormat = (String)e.SelectedItem.UserData;
                Settings.Save();
            };

            ComboBox defaultQuicksaveType = GwenHelper.CreateLabeledCombobox(saveSettings, "Default Quicksave Format:");
            defaultQuicksaveType.AddItem(".trk", "", ".trk");
            defaultQuicksaveType.AddItem(".json", "", ".json");
            defaultQuicksaveType.AddItem(".sol", "", ".sol");
            defaultQuicksaveType.ItemSelected += (o, e) =>
            {
                Settings.DefaultQuicksaveFormat = (String)e.SelectedItem.UserData;
                Settings.Save();
            };

            ComboBox defaultAutosaveType = GwenHelper.CreateLabeledCombobox(saveSettings, "Default Autosave Format:");
            defaultAutosaveType.AddItem(".trk", "", ".trk");
            defaultAutosaveType.AddItem(".json", "", ".json");
            defaultAutosaveType.AddItem(".sol", "", ".sol");
            defaultAutosaveType.SelectByUserData(Settings.DefaultAutosaveFormat);
            defaultAutosaveType.ItemSelected += (o, e) =>
            {
                Settings.DefaultAutosaveFormat = (String)e.SelectedItem.UserData;
                Settings.Save();
            };

            ComboBox defaultCrashBackupType = GwenHelper.CreateLabeledCombobox(saveSettings, "Default Crash Backup Format:");
            defaultCrashBackupType.AddItem(".trk", "", ".trk");
            defaultCrashBackupType.AddItem(".json", "", ".json");
            defaultCrashBackupType.AddItem(".sol", "", ".sol");
            defaultCrashBackupType.SelectByUserData(Settings.DefaultCrashBackupFormat);
            defaultCrashBackupType.ItemSelected += (o, e) =>
            {
                Settings.DefaultCrashBackupFormat = (String)e.SelectedItem.UserData;
                Settings.Save();
            };

            defaultSaveType.SelectByUserData(Settings.DefaultSaveFormat);
            defaultQuicksaveType.SelectByUserData(Settings.DefaultQuicksaveFormat);
            defaultAutosaveType.SelectByUserData(Settings.DefaultAutosaveFormat);
            defaultAutosaveType.SelectByUserData(Settings.DefaultCrashBackupFormat);
        }
        private void PopulateRiderSettings(ControlBase parent)
        {
            var scarfSettingPanel = GwenHelper.CreateHeaderPanel(parent, "Scarf Settings");
            var riderSettingPanel = GwenHelper.CreateHeaderPanel(parent, "Rider Settings");

            ComboBox scarfCombobox = GwenHelper.CreateLabeledCombobox(scarfSettingPanel, "Selected Scarf:");
            scarfCombobox.AddItem("Default", "*default*", "*default*");
            string[] scarfPaths = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\LRA\\Scarves");
            for (int i = 0; i < scarfPaths.Length; i++)
            {
                string scarfNames = Path.GetFileName(scarfPaths[i]);
                scarfCombobox.AddItem(scarfNames, scarfNames, scarfNames);
            }

            scarfCombobox.ItemSelected += (o, e) =>
            {
                Settings.SelectedScarf = (String)e.SelectedItem.UserData;
                Debug.WriteLine("Selected Scarf: \"" + Settings.SelectedScarf + "\"");
                Settings.Save();
            };

            var scarfSegments = new Spinner(parent)
            {
                Min = 1,
                Max = int.MaxValue - 1,
                Value = Settings.ScarfSegments,
            };
            scarfSegments.ValueChanged += (o, e) =>
            {
                Settings.ScarfSegments = (int)((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(scarfSettingPanel, "Scarf Segments (Needs Restart)", scarfSegments);

            var multiScarfAmount = new Spinner(parent)
            {
                Min = 1,
                Max = int.MaxValue - 1,
                Value = Settings.multiScarfAmount,
            };
            multiScarfAmount.ValueChanged += (o, e) =>
            {
                Settings.multiScarfAmount = (int)((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(scarfSettingPanel, "Multi-Scarf Amount (Needs Restart)", multiScarfAmount);

            var multiScarfSegments = new Spinner(parent)
            {
                Min = 1,
                Max = int.MaxValue - 1,
                Value = Settings.multiScarfSegments,
            };
            multiScarfSegments.ValueChanged += (o, e) =>
            {
                Settings.multiScarfSegments = (int)((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(scarfSettingPanel, "Multi-Scarf Segments (Needs Restart)", multiScarfSegments);

            var showid = GwenHelper.AddCheckbox(scarfSettingPanel, "Apply Custom Scarf to Rider png", Settings.customScarfOnPng, (o, e) =>
            {
                Settings.customScarfOnPng = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            ComboBox boshSkinCombobox = GwenHelper.CreateLabeledCombobox(riderSettingPanel, "Selected Rider:");
            boshSkinCombobox.AddItem("Default", "*default*", "*default*");


            string[] riderPaths = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/LRA/Riders");
            for (int i = 0; i < riderPaths.Length; i++)
            {
                String riderNames = Path.GetFileName(riderPaths[i]);
                boshSkinCombobox.AddItem(riderNames, riderNames, riderNames);
            }
            boshSkinCombobox.ItemSelected += (o, e) =>
            {
                Settings.SelectedBoshSkin = (String)e.SelectedItem.UserData; ;
                Debug.WriteLine("Selected rider Skin: \"" + Settings.SelectedBoshSkin + "\"");
                Settings.Save();
            };

            scarfCombobox.SelectByUserData(Settings.SelectedScarf);
            boshSkinCombobox.SelectByUserData(Settings.SelectedBoshSkin);
        }
        private void PopulateRBLSettings(ControlBase parent)
        {
            var rblHeader = GwenHelper.CreateHeaderPanel(parent, "RatherBeLunar's Magic Animator Settings");

//            GwenHelper.AddCheckbox(rblHeader, "Reference frame based animation", Settings.velocityReferenceFrameAnimation, (o, e) =>
//            {
//                Settings.velocityReferenceFrameAnimation = ((Checkbox)o).IsChecked;
//                Settings.Save();
//            });

            GwenHelper.AddCheckbox(rblHeader, "Convert lines sent to previous frames to scenery", Settings.recededLinesAsScenery, (o, e) =>
            {
                Settings.recededLinesAsScenery = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            GwenHelper.AddCheckbox(rblHeader, "Convert lines sent to forward frames to scenery", Settings.forwardLinesAsScenery, (o, e) =>
            {
                Settings.forwardLinesAsScenery = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            Spinner animationVelXSpinner = new Spinner(rblHeader)
            {
                Dock = Dock.Bottom,
                Max = 1000,
                Min = -1000,
                Value = Settings.animationRelativeVelX
            };
            Spinner animationVelYSpinner = new Spinner(rblHeader)
            {
                Dock = Dock.Bottom,
                Max = 1000,
                Min = -1000,
                Value = Settings.animationRelativeVelY
            };
            animationVelXSpinner.ValueChanged += (o, e) =>
            {
                Settings.animationRelativeVelX = (float)((Spinner)o).Value;
                Settings.Save();
            };
            animationVelYSpinner.ValueChanged += (o, e) =>
            {
                Settings.animationRelativeVelY = (float)((Spinner)o).Value;
                Settings.Save();
            };
            GwenHelper.CreateLabeledControl(rblHeader, "Relative Animation X Velocity", animationVelXSpinner);
            GwenHelper.CreateLabeledControl(rblHeader, "Relative Animation Y Velocity", animationVelYSpinner);

        }

        private void Setup()
        {
            var cat = _prefcontainer.Add("Settings");
            var page = AddPage(cat, "Editor");
            PopulateEditor(page);
            page = AddPage(cat, "Playback");
            PopulatePlayback(page);
            page = AddPage(cat, "Tools");
            PopulateTools(page);
            page = AddPage(cat, "Environment");
            PopulateModes(page);
            page = AddPage(cat, "Camera");
            PopulateCamera(page);
            page = AddPage(cat, "Lines");
            PopulateLines(page);
            cat = _prefcontainer.Add("Tools");
            page = AddPage(cat, "Animation");
            PopulateRBLSettings(page);
            cat = _prefcontainer.Add("Application");
            page = AddPage(cat, "Audio");
            PopulateAudio(page);
            page = AddPage(cat, "Keybindings");
            PopulateKeybinds(page);
            page = AddPage(cat, "Other");
            PopulateOther(page);
            page = AddPage(cat, "Rider");
            PopulateRiderSettings(page);
            cat = _prefcontainer.Add("Mods");

            if (Settings.SettingsPane >= _tabscount && _focus == null)
            {
                Settings.SettingsPane = 0;
                _focus = page;
                page.Show();
            }

        }
        private void CategorySelected(object sender, ItemSelectedEventArgs e)
        {
            if (_focus != e.SelectedItem.UserData)
            {
                if (_focus != null)
                {
                    _focus.Hide();
                }
                _focus = (ControlBase)e.SelectedItem.UserData;
                _focus.Show();
                Settings.SettingsPane = (int)_focus.UserData;
                Settings.Save();
            }
        }
        private ControlBase AddPage(CollapsibleCategory category, string name)
        {
            var btn = category.Add(name);
            Panel panel = new Panel(this);
            panel.Dock = Dock.Fill;
            panel.Padding = Padding.Five;
            panel.Hide();
            panel.UserData = _tabscount;
            btn.UserData = panel;
            category.Selected += CategorySelected;
            if (_tabscount == Settings.SettingsPane)
                btn.Press();
            _tabscount += 1;
            return panel;
        }
    }
}
