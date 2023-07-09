using Gwen;
using Gwen.Controls;
using linerider.Drawing.RiderModel;
using linerider.Utils;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace linerider.UI
{
    public class PreferencesWindow : DialogBase
    {
        private readonly CollapsibleList _prefcontainer;
        private ControlBase _focus;
        private int _tabscount = 0;

        public PreferencesWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = "Preferences";
            _ = SetSize(500, 525);
            MinimumSize = Size;
            ControlBase bottom = new ControlBase(this)
            {
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                Margin = new Margin(0, 5, 0, 0),
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
            MessageBox mbox = MessageBox.Show(
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
                    _editor.RedrawAllLines();
                    _ = Close();
                    _canvas.ShowPreferencesDialog();
                }
            };
        }

        private void Setup()
        {
            CollapsibleCategory cat;
            ControlBase page;

            cat = _prefcontainer.Add("Editor");

            page = AddPage(cat, "Visualization");
            PopulateVisualization(page);
            page = AddPage(cat, "Playback");
            PopulatePlayback(page);
            page = AddPage(cat, "Camera");
            PopulateCamera(page);
            page = AddPage(cat, "Tools");
            PopulateTools(page);

            cat = _prefcontainer.Add("Interface");

            page = AddPage(cat, "General");
            PopulateInterfaceGeneral(page);
            page = AddPage(cat, "Colors");
            PopulateColors(page);
            page = AddPage(cat, "Rider");
            PopulateRider(page);

            cat = _prefcontainer.Add("Application");

            page = AddPage(cat, "Keybindings");
            PopulateKeybinds(page);
            page = AddPage(cat, "Audio");
            PopulateAudio(page);
            page = AddPage(cat, "Other");
            PopulateOther(page);

            cat = _prefcontainer.Add("Tools");

            page = AddPage(cat, "Animation");
            PopulateRBLAnimation(page);

            if (Settings.SettingsPane >= _tabscount && _focus == null)
            {
                Settings.SettingsPane = 0;
                _focus = page;
                page.Show();
            }
        }
        private ControlBase AddPage(CollapsibleCategory category, string name)
        {
            Panel panel = new Panel(this)
            {
                Dock = Dock.Fill,
                Padding = Padding.Five
            };
            panel.Hide();
            panel.UserData = _tabscount;

            Button btn = category.Add(name);
            btn.UserData = panel;

            category.Selected += CategorySelected;
            if (_tabscount == Settings.SettingsPane)
                btn.Press();
            _tabscount += 1;

            return panel;
        }
        private void CategorySelected(object sender, ItemSelectedEventArgs e)
        {
            if (_focus != e.SelectedItem.UserData)
            {
                _focus?.Hide();

                _focus = (ControlBase)e.SelectedItem.UserData;
                _focus.Show();
                Settings.SettingsPane = (int)_focus.UserData;
                Settings.Save();
            }
        }

        private void PopulateVisualization(ControlBase parent)
        {
            Panel advancedGroup = GwenHelper.CreateHeaderPanel(parent, "Advanced Visualization");

            Panel advancedGroupLeft = new Panel(advancedGroup)
            {
                Dock = Dock.Left,
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                Margin = Margin.Zero,
            };
            Checkbox contact = GwenHelper.AddCheckbox(advancedGroupLeft, "Contact Points", Settings.Editor.DrawContactPoints, (o, e) =>
            {
                Settings.Editor.DrawContactPoints = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox momentum = GwenHelper.AddCheckbox(advancedGroupLeft, "Momentum Vectors", Settings.Editor.MomentumVectors, (o, e) =>
            {
                Settings.Editor.MomentumVectors = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox hitbox = GwenHelper.AddCheckbox(advancedGroupLeft, "Line Hitbox", Settings.Editor.RenderGravityWells, (o, e) =>
            {
                Settings.Editor.RenderGravityWells = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox hittest = GwenHelper.AddCheckbox(advancedGroupLeft, "Hit Test", Settings.Editor.HitTest, (o, e) =>
            {
                Settings.Editor.HitTest = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox drawagws = GwenHelper.AddCheckbox(advancedGroupLeft, "Line Extensions (AGWs)", Settings.DrawAGWs, (o, e) =>
            {
                Settings.DrawAGWs = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            Panel advancedGroupRight = new Panel(advancedGroup)
            {
                Dock = Dock.Right,
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                Margin = Margin.Zero,
            };
            Checkbox drawgrid = GwenHelper.AddCheckbox(advancedGroupRight, "Simulation Grid", Settings.DrawCollisionGrid, (o, e) =>
            {
                Settings.DrawCollisionGrid = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox drawfloatgrid = GwenHelper.AddCheckbox(advancedGroupRight, "Floating-point grid", Settings.DrawFloatGrid, (o, e) =>
            {
                Settings.DrawFloatGrid = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox drawcam = GwenHelper.AddCheckbox(advancedGroupRight, "Camera", Settings.DrawCamera, (o, e) =>
            {
                Settings.DrawCamera = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox coordmenu = GwenHelper.AddCheckbox(advancedGroupRight, "Show Coordinate Menu", Settings.Editor.ShowCoordinateMenu, (o, e) =>
            {
                Settings.Editor.ShowCoordinateMenu = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            Panel onionGroup = GwenHelper.CreateHeaderPanel(parent, "Onion Skinning");
            Checkbox onion = GwenHelper.AddCheckbox(onionGroup, "Enabled", Settings.OnionSkinning, (o, e) =>
            {
                Settings.OnionSkinning = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Spinner pastOnionSkins = new Spinner(onionGroup)
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
            Spinner futureOnionSkins = new Spinner(onionGroup)
            {
                Margin = new Margin(3, 0, 0, 0),
                Min = 0,
                Max = 1000,
                Value = Settings.FutureOnionSkins,
            };
            futureOnionSkins.ValueChanged += (o, e) =>
            {
                Settings.FutureOnionSkins = (int)((Spinner)o).Value;
                Settings.Save();
            };
            _ = GwenHelper.CreateLabeledControl(onionGroup, "Onion Skins Before/After", new ControlBase[2] { pastOnionSkins, futureOnionSkins });

            Panel overlayGroup = GwenHelper.CreateHeaderPanel(parent, "Frame Overlay");

            ControlBase offsetbase = null;
            ControlBase fixedbase = null;

            Spinner offsetspinner = new Spinner(null)
            {
                Min = -999,
                Max = 999,
                Value = Settings.Local.TrackOverlayOffset,
            };
            offsetspinner.ValueChanged += (o, e) =>
            {
                Settings.Local.TrackOverlayOffset = (int)offsetspinner.Value;
            };
            Spinner fixedspinner = new Spinner(null)
            {
                Min = 0,
                Max = _editor.FrameCount,
                Value = Settings.Local.TrackOverlayFixedFrame,
            };
            fixedspinner.ValueChanged += (o, e) =>
            {
                Settings.Local.TrackOverlayFixedFrame = (int)fixedspinner.Value;
            };
            void updatestate()
            {
                offsetspinner.IsDisabled = !Settings.Local.TrackOverlay;
                fixedspinner.IsDisabled = !Settings.Local.TrackOverlay;
                offsetbase.IsHidden = Settings.Local.TrackOverlayFixed;
                fixedbase.IsHidden = !Settings.Local.TrackOverlayFixed;
            }
            Checkbox enabled = GwenHelper.AddCheckbox(overlayGroup, "Enabled", Settings.Local.TrackOverlay, (o, e) =>
            {
                Settings.Local.TrackOverlay = ((Checkbox)o).IsChecked;
                updatestate();
            });
            _ = GwenHelper.AddCheckbox(overlayGroup, "Fixed Frame", Settings.Local.TrackOverlayFixed, (o, e) =>
            {
                Settings.Local.TrackOverlayFixed = ((Checkbox)o).IsChecked;
                updatestate();
            });
            offsetbase = GwenHelper.CreateLabeledControl(overlayGroup, "Frame Offset", offsetspinner);
            fixedbase = GwenHelper.CreateLabeledControl(overlayGroup, "Frame ID", fixedspinner);
            updatestate();
            enabled.Tooltip = "Display an onion skin of the track\nat a specified offset for animation";

            onion.Tooltip = "Visualize the rider before/after\nthe current frame.";
            momentum.Tooltip = "Visualize the direction of\nmomentum for each contact point";
            contact.Tooltip = "Visualize the parts of the rider\nthat interact with lines.";
            hitbox.Tooltip = "Visualizes the hitbox of lines\nUsed for advanced editing";
            hittest.Tooltip = "Lines that have been hit by\nthe rider will glow.";

            Panel otherGroup = GwenHelper.CreateHeaderPanel(parent, "Other");
            Checkbox invisibleRider = GwenHelper.AddCheckbox(otherGroup, "Invisible Rider", Settings.InvisibleRider, (o, e) =>
            {
                Settings.InvisibleRider = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
        }
        private void PopulatePlayback(ControlBase parent)
        {
            Panel zoomGroup = GwenHelper.CreateHeaderPanel(parent, "Playback Zoom");

            RadioButtonGroup pbzoom = new RadioButtonGroup(zoomGroup)
            {
                Dock = Dock.Left,
                ShouldDrawBackground = false,
            };
            _ = pbzoom.AddOption("Default Zoom");
            _ = pbzoom.AddOption("Current Zoom");
            _ = pbzoom.AddOption("Specific Zoom");

            Spinner specificzoomspinner = new Spinner(zoomGroup)
            {
                Max = 24,
                Min = 1,
            };

            // A bit hacky but simplest way to show compact zoom spinner next to a radio button
            ControlBase specificzoomwrapper = GwenHelper.CreateLabeledControl(zoomGroup, "", specificzoomspinner);
            specificzoomwrapper.Dock = Dock.Bottom;

            pbzoom.SelectionChanged += (o, e) =>
            {
                Settings.PlaybackZoomType = ((RadioButtonGroup)o).SelectedIndex;
                Settings.Save();
                specificzoomwrapper.IsHidden = ((RadioButtonGroup)o).SelectedIndex != 2;
            };
            specificzoomspinner.ValueChanged += (o, e) =>
            {
                Settings.PlaybackZoomValue = (float)((Spinner)o).Value;
                Settings.Save();
            };
            pbzoom.SetSelection(Settings.PlaybackZoomType);
            specificzoomspinner.Value = Settings.PlaybackZoomValue;

            Panel framerateGroup = GwenHelper.CreateHeaderPanel(parent, "Frame Control");
            Checkbox smooth = GwenHelper.AddCheckbox(framerateGroup, "Smooth Playback", Settings.SmoothPlayback, (o, e) =>
            {
                Settings.SmoothPlayback = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            ComboBox pbrate = GwenHelper.CreateLabeledCombobox(framerateGroup, "Playback Rate:");
            for (int i = 0; i < Constants.MotionArray.Length; i++)
            {
                float f = Constants.MotionArray[i] / (float)Constants.PhysicsRate;
                _ = pbrate.AddItem(f + "x", f.ToString(CultureInfo.InvariantCulture), f);
            }
            pbrate.SelectByName(Settings.DefaultPlayback.ToString(CultureInfo.InvariantCulture));
            pbrate.ItemSelected += (o, e) =>
            {
                Settings.DefaultPlayback = (float)e.SelectedItem.UserData;
                Settings.Save();
            };
            ComboBox cbslowmo = GwenHelper.CreateLabeledCombobox(framerateGroup, "Slowmo FPS:");
            int[] fpsarray = new[] { 1, 2, 5, 10, 20 };
            for (int i = 0; i < fpsarray.Length; i++)
            {
                _ = cbslowmo.AddItem(fpsarray[i].ToString(), fpsarray[i].ToString(CultureInfo.InvariantCulture),
                    fpsarray[i]);
            }
            cbslowmo.SelectByName(Settings.SlowmoSpeed.ToString(CultureInfo.InvariantCulture));
            cbslowmo.ItemSelected += (o, e) =>
            {
                Settings.SlowmoSpeed = (int)e.SelectedItem.UserData;
                Settings.Save();
            };
            smooth.Tooltip = "Interpolates frames from the base\nphysics rate of 40 frames/second\nup to 60 frames/second";

            Panel otherGroup = GwenHelper.CreateHeaderPanel(parent, "Other");

            Checkbox colorplayback = GwenHelper.AddCheckbox(otherGroup, "Color Playback", Settings.ColorPlayback, (o, e) =>
            {
                Settings.ColorPlayback = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            colorplayback.Tooltip = "Show lines color during playback";
            Spinner timelinelength = new Spinner(parent)
            {
                Min = 1,
                Max = 600,
                Value = Settings.DefaultTimelineLength,
            };
            timelinelength.ValueChanged += (o, e) =>
            {
                Settings.DefaultTimelineLength = (int)((Spinner)o).Value;
                Settings.Save();
            };
            ControlBase timelinelengthbar = GwenHelper.CreateLabeledControl(otherGroup, "Default Timeline Length (Seconds)", timelinelength);
            timelinelengthbar.Tooltip = "Timeline length on game startup";

            Spinner triggerlength = new Spinner(parent)
            {
                Min = 1,
                Max = int.MaxValue - 1,
                Value = Settings.DefaultTriggerLength,
            };
            triggerlength.ValueChanged += (o, e) =>
            {
                Settings.DefaultTriggerLength = (int)((Spinner)o).Value;
                Settings.Save();
            };
            ControlBase triggerlengthbar = GwenHelper.CreateLabeledControl(otherGroup, "Default Trigger Length (Frames)", triggerlength);
            triggerlengthbar.Tooltip = "Length of new triggers\n40 frames = 1 second";
        }
        private void PopulateCamera(ControlBase parent)
        {
            Panel typeGroup = GwenHelper.CreateHeaderPanel(parent, "Camera Type");

            RadioButtonGroup rbcamera = new RadioButtonGroup(typeGroup)
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false,
            };
            RadioButton soft = rbcamera.AddOption("Soft Camera");
            RadioButton predictive = rbcamera.AddOption("Predictive Camera");
            RadioButton legacy = rbcamera.AddOption("Legacy Camera");

            Panel propsGroup = GwenHelper.CreateHeaderPanel(parent, "Camera Properties");
            Checkbox round = GwenHelper.AddCheckbox(propsGroup, "Round Legacy Camera", Settings.RoundLegacyCamera, (o, e) =>
            {
                Settings.RoundLegacyCamera = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            if (Settings.SmoothCamera)
            {
                round.IsDisabled = true;

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

            Panel editorGroup = GwenHelper.CreateHeaderPanel(parent, "Editor Camera Control");
            Checkbox superzoom = GwenHelper.AddCheckbox(editorGroup, "Superzoom", Settings.SuperZoom, (o, e) =>
            {
                Settings.SuperZoom = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            ComboBox scroll = GwenHelper.CreateLabeledCombobox(editorGroup, "Scroll Sensitivity:");
            scroll.Margin = new Margin(0, 0, 0, 0);
            scroll.Dock = Dock.Right;
            scroll.AddItem("0.25x").Name = "0.25";
            scroll.AddItem("0.5x").Name = "0.5";
            scroll.AddItem("0.75x").Name = "0.75";
            scroll.AddItem("1x").Name = "1";
            scroll.AddItem("2x").Name = "2";
            scroll.AddItem("3x").Name = "3";
            scroll.SelectByName("1"); // Default if user setting fails.
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

            Panel otherGroup = GwenHelper.CreateHeaderPanel(parent, "Other");

            Spinner zoomMultiplier = new Spinner(this)
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

            _ = GwenHelper.CreateLabeledControl(otherGroup, "Zoom Multiplier", zoomMultiplier);
        }
        private void PopulateTools(ControlBase parent)
        {
            Panel bezierGroup = GwenHelper.CreateHeaderPanel(parent, "Bezier Tool");

            Spinner resolution = new Spinner(bezierGroup)
            {
                Min = 5,
                Max = 100,
                Value = Settings.Bezier.Resolution,
                IncrementSize = 1
            };
            resolution.ValueChanged += (o, e) =>
            {
                Settings.Bezier.Resolution = (int)((Spinner)o).Value;
                Settings.Save();
            };
            _ = GwenHelper.CreateLabeledControl(bezierGroup, "Resolution (Lines Per 100 Pixels)", resolution);

            Spinner nodeSize = new Spinner(bezierGroup)
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
            _ = GwenHelper.CreateLabeledControl(bezierGroup, "Size of the bezier curve nodes", nodeSize);

            RadioButtonGroup bezierModeSelector = new RadioButtonGroup(bezierGroup)
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false
            };
            RadioButton directType = bezierModeSelector.AddOption("Direct Visualization Mode");
            RadioButton traceType = bezierModeSelector.AddOption("Trace Visualization Mode");
            switch ((Settings.BezierMode)Settings.Bezier.Mode)
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

            Panel smoothpencilGroup = GwenHelper.CreateHeaderPanel(parent, "Smooth Pencil");
            Spinner smUpdateSpeed = new Spinner(smoothpencilGroup)
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
            _ = GwenHelper.CreateLabeledControl(smoothpencilGroup, "Update Speed in Milliseconds", smUpdateSpeed);
            smUpdateSpeed.Tooltip = "Determines how often the lines are dragged in milliseconds\nLeave at 0 to update as fast as your framerate allows";
            Spinner smStabilizer = new Spinner(smoothpencilGroup)
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
            _ = GwenHelper.CreateLabeledControl(smoothpencilGroup, "Stabilizer", smStabilizer);
            smStabilizer.Tooltip = "Determines by how much your lines are dragged behind";

            Panel selectGroup = GwenHelper.CreateHeaderPanel(parent, "Select Tool");

            Panel lineGroup = GwenHelper.CreateHeaderPanel(selectGroup, "Line Info");
            lineGroup.Dock = Dock.Left;
            lineGroup.Margin = Margin.Zero;
            Checkbox length = GwenHelper.AddCheckbox(lineGroup, "Show Length", Settings.Editor.ShowLineLength, (o, e) =>
            {
                Settings.Editor.ShowLineLength = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox angle = GwenHelper.AddCheckbox(lineGroup, "Show Angle", Settings.Editor.ShowLineAngle, (o, e) =>
            {
                Settings.Editor.ShowLineAngle = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox showid = GwenHelper.AddCheckbox(lineGroup, "Show ID", Settings.Editor.ShowLineID, (o, e) =>
            {
                Settings.Editor.ShowLineID = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            Panel lifelockGroup = GwenHelper.CreateHeaderPanel(selectGroup, "Lifelock Conditions");
            lifelockGroup.Dock = Dock.Right;
            lifelockGroup.Margin = Margin.Zero;
            _ = GwenHelper.AddCheckbox(lifelockGroup, "Next Frame Constraints", Settings.Editor.LifeLockNoOrange, (o, e) =>
            {
                Settings.Editor.LifeLockNoOrange = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            _ = GwenHelper.AddCheckbox(lifelockGroup, "No Fakie Death", Settings.Editor.LifeLockNoFakie, (o, e) =>
            {
                Settings.Editor.LifeLockNoFakie = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            Panel snappingGroup = GwenHelper.CreateHeaderPanel(parent, "Snapping");

            Panel snappingGroupBottom = new Panel(snappingGroup)
            {
                Dock = Dock.Bottom,
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                Margin = Margin.Zero,
            };
            Spinner snapAngle = new Spinner(snappingGroupBottom)
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
            ControlBase snapdegrees = GwenHelper.CreateLabeledControl(snappingGroupBottom, "X/Y Snap Degrees", snapAngle);

            Panel snappingGroupLeft = new Panel(snappingGroup)
            {
                Dock = Dock.Left,
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                Margin = Margin.Zero,
            };
            Checkbox linesnap = GwenHelper.AddCheckbox(snappingGroupLeft, "Snap New Lines", Settings.Editor.SnapNewLines, (o, e) =>
            {
                Settings.Editor.SnapNewLines = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox movelinesnap = GwenHelper.AddCheckbox(snappingGroupLeft, "Snap Line Movement", Settings.Editor.SnapMoveLine, (o, e) =>
            {
                Settings.Editor.SnapMoveLine = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            movelinesnap.Tooltip = "Snap to lines when using the\nselect tool to move a single line";

            Panel snappingGroupRight = new Panel(snappingGroup)
            {
                Dock = Dock.Right,
                ShouldDrawBackground = false,
                AutoSizeToContents = true,
                Margin = Margin.Zero,
            };
            Checkbox gridsnap = GwenHelper.AddCheckbox(snappingGroupRight, "Snap to displayed grids", Settings.Editor.SnapToGrid, (o, e) =>
            {
                Settings.Editor.SnapToGrid = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox forcesnap = GwenHelper.AddCheckbox(snappingGroupRight, "Force X/Y snap", Settings.Editor.ForceXySnap, (o, e) =>
            {
                Settings.Editor.ForceXySnap = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            forcesnap.Tooltip = "Forces all lines drawn to\nsnap to multiples of a chosen angle";
        }
        private void PopulateInterfaceGeneral(ControlBase parent)
        {
            Panel generalGroup = GwenHelper.CreateHeaderPanel(parent, "General");
            _ = GwenHelper.AddCheckbox(generalGroup, "Night Mode", Settings.NightMode, (o, e) =>
            {
                Settings.NightMode = ((Checkbox)o).IsChecked;
                Settings.Save();
                _canvas.RefreshCursors();
            });
            Checkbox preview = GwenHelper.AddCheckbox(generalGroup, "Preview Mode", Settings.PreviewMode, (o, e) =>
            {
                Settings.PreviewMode = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            Checkbox recording = GwenHelper.AddCheckbox(generalGroup, "Recording Mode", Settings.Local.RecordingMode, (o, e) =>
            {
                Settings.Local.RecordingMode = ((Checkbox)o).IsChecked;
            });

            Panel uiGroup = GwenHelper.CreateHeaderPanel(parent, "UI");
            ComboBox uiScale = GwenHelper.CreateLabeledCombobox(uiGroup, "Scale *");

            _ = uiScale.AddItem("Auto", "", 0f);
            _ = uiScale.AddItem("100%", "", 1f);
            _ = uiScale.AddItem("125%", "", 1.25f);
            _ = uiScale.AddItem("150%", "", 1.5f);
            _ = uiScale.AddItem("175%", "", 1.75f);
            _ = uiScale.AddItem("200%", "", 2f);
            _ = uiScale.AddItem("300%", "", 3f);
            _ = uiScale.AddItem("400%", "", 4f);
            uiScale.SelectByUserData(Settings.UIScale);

            uiScale.ItemSelected += (o, e) =>
            {
                if (e.SelectedItem != null)
                {
                    Settings.UIScale = (float)e.SelectedItem.UserData;
                    Settings.Save();
                    _canvas.RefreshCursors();
                }
            };

            _ = GwenHelper.AddCheckbox(uiGroup, "Show Zoom Bar", Settings.UIShowZoom, (o, e) =>
            {
                Settings.UIShowZoom = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            _ = GwenHelper.AddCheckbox(uiGroup, "Show Speed Control Buttons", Settings.UIShowSpeedButtons, (o, e) =>
            {
                Settings.UIShowSpeedButtons = ((Checkbox)o).IsChecked;
                Settings.Save();
            });
            _ = GwenHelper.CreateHintLabel(uiGroup, "* Restart required");
            _ = GwenHelper.CreateHintLabel(uiGroup, "Scale is a Work In Progress feature,\nsome UI parts are not scalable yet.");
        }
        private void PopulateColors(ControlBase parent)
        {
            Panel editorColors = GwenHelper.CreateHeaderPanel(parent, "Editor");
            _ = GwenHelper.CreateLabeledColorInput(editorColors, "Background", Settings.Colors.EditorBg, (o, color) =>
            {
                Settings.Colors.EditorBg = color.Value;
                Settings.Save();
            });
            _ = GwenHelper.CreateLabeledColorInput(editorColors, "Lines", Settings.Colors.EditorLine, (o, color) =>
            {
                Settings.Colors.EditorLine = color.Value;
                Settings.Save();
            });

            Panel nightColors = GwenHelper.CreateHeaderPanel(parent, "Editor (Night Mode)");
            _ = GwenHelper.CreateLabeledColorInput(nightColors, "Background", Settings.Colors.EditorNightBg, (o, color) =>
            {
                Settings.Colors.EditorNightBg = color.Value;
                Settings.Save();
            });
            _ = GwenHelper.CreateLabeledColorInput(nightColors, "Lines", Settings.Colors.EditorNightLine, (o, color) =>
            {
                Settings.Colors.EditorNightLine = color.Value;
                Settings.Save();
            });

            Panel exportColors = GwenHelper.CreateHeaderPanel(parent, "Export / Preview Mode");
            _ = GwenHelper.CreateLabeledColorInput(exportColors, "Background", Settings.Colors.ExportBg, (o, color) =>
            {
                bool isDefaultColor = _editor.HasDefaultTrackBackground;

                Settings.Colors.ExportBg = color.Value;
                Settings.Save();

                if (isDefaultColor)
                    _editor.SetDefaultTrackColors();
            });
            _ = GwenHelper.CreateLabeledColorInput(exportColors, "Lines", Settings.Colors.ExportLine, (o, color) =>
            {
                bool isDefaultColor = _editor.HasDefaultTrackLineColor;

                Settings.Colors.ExportLine = color.Value;
                Settings.Save();

                if (isDefaultColor)
                    _editor.SetDefaultTrackColors();
            });

            Panel lineTypesColors = GwenHelper.CreateHeaderPanel(parent, "Line types");
            _ = GwenHelper.CreateLabeledColorInput(lineTypesColors, "Standard", Settings.Colors.StandardLine, (o, color) =>
            {
                Settings.Colors.StandardLine = color.Value;
                Settings.Save();
                _editor.RedrawAllLines();
            });
            _ = GwenHelper.CreateLabeledColorInput(lineTypesColors, "Acceleration", Settings.Colors.AccelerationLine, (o, color) =>
            {
                Settings.Colors.AccelerationLine = color.Value;
                Settings.Save();
                _editor.RedrawAllLines();
            });
            _ = GwenHelper.CreateLabeledColorInput(lineTypesColors, "Scenery", Settings.Colors.SceneryLine, (o, color) =>
            {
                Settings.Colors.SceneryLine = color.Value;
                Settings.Save();
                _editor.RedrawAllLines();
            });
        }
        private void PopulateRider(ControlBase parent)
        {
            string manualUrl = $"{Constants.GithubPageHeader}/tree/main/Examples";

            Panel generalGroup = GwenHelper.CreateHeaderPanel(parent, "General");

            ComboBox boshSkinCombobox = GwenHelper.CreateLabeledCombobox(generalGroup, "Rider:");
            _ = boshSkinCombobox.AddItem("Default", Constants.InternalDefaultName, Constants.InternalDefaultName);
            string[] riderPaths = Directory.GetDirectories(Path.Combine(Program.UserDirectory, "Riders"));
            foreach (string riderPath in riderPaths)
            {
                string riderName = Path.GetFileName(riderPath);
                _ = boshSkinCombobox.AddItem(riderName, riderName, riderName);
            }
            boshSkinCombobox.ItemSelected += (o, e) =>
            {
                Settings.SelectedBoshSkin = (string)e.SelectedItem.UserData;
                Debug.WriteLine($"Selected Rider: \"{Settings.SelectedBoshSkin}\"");
                Settings.Save();
            };

            ComboBox scarfCombobox = GwenHelper.CreateLabeledCombobox(generalGroup, "Scarf:");
            _ = scarfCombobox.AddItem("Default", Constants.InternalDefaultName, Constants.InternalDefaultName);
            string[] scarfPaths = Directory.GetFiles(Path.Combine(Program.UserDirectory, "Scarves"));
            foreach (string scarfPath in scarfPaths)
            {
                string ext = Path.GetExtension(scarfPath).ToLower();
                string scarfFilename = Path.GetFileName(scarfPath);
                string scarfName = ext == ".txt" || ext == ".png" ? Path.GetFileNameWithoutExtension(scarfPath) : Path.GetFileName(scarfPath);
                _ = scarfCombobox.AddItem(scarfName, scarfFilename, scarfFilename);
            }

            scarfCombobox.ItemSelected += (o, e) =>
            {
                Settings.SelectedScarf = (string)e.SelectedItem.UserData;
                Debug.WriteLine($"Selected Scarf: \"{Settings.SelectedScarf}\"");
                Settings.Save();
            };

            scarfCombobox.SelectByUserData(Settings.SelectedScarf);
            boshSkinCombobox.SelectByUserData(Settings.SelectedBoshSkin);

            Panel scarfGroup = GwenHelper.CreateHeaderPanel(parent, "Scarf Settings");

            Spinner scarfSegments = new Spinner(parent)
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
            _ = GwenHelper.CreateLabeledControl(scarfGroup, "Scarf Segments *", scarfSegments);

            Spinner multiScarfAmount = new Spinner(parent)
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
            _ = GwenHelper.CreateLabeledControl(scarfGroup, "Multi-Scarf Amount *", multiScarfAmount);

            Spinner multiScarfSegments = new Spinner(parent)
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
            _ = GwenHelper.CreateLabeledControl(scarfGroup, "Multi-Scarf Segments *", multiScarfSegments);
            _ = GwenHelper.CreateHintLabel(scarfGroup, "* Restart required");

            Button openManualBtn = new Button(parent)
            {
                Dock = Dock.Bottom,
                Text = "Open customization manual (GitHub)",
                Alignment = Pos.CenterH | Pos.CenterV,
                Margin = new Margin(10, 0, 10, 10),
            };
            openManualBtn.Clicked += (o, e) => GameCanvas.OpenUrl(manualUrl);

            Button forceReloadBtn = new Button(parent)
            {
                Dock = Dock.Bottom,
                Text = "Force reload rider and scarf",
                Alignment = Pos.CenterH | Pos.CenterV,
                Margin = new Margin(10, 0, 10, 10),
            };
            forceReloadBtn.Clicked += (o, e) =>
            {
                RiderLoader.ReloadAll();
            };
        }
        private void PopulateKeybinds(ControlBase parent) => _ = new HotkeyWidget(parent);
        private void PopulateAudio(ControlBase parent)
        {
            Panel generalGroup = GwenHelper.CreateHeaderPanel(parent, "General");
            Checkbox syncenabled = GwenHelper.AddCheckbox(generalGroup, "Mute", Settings.MuteAudio, (o, e) =>
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
            _ = GwenHelper.CreateLabeledControl(generalGroup, "Volume", vol);
            vol.Width = 200;
        }
        private void PopulateOther(ControlBase parent)
        {
            Panel updatesGroup = GwenHelper.CreateHeaderPanel(parent, "Updates");

            Checkbox showid = GwenHelper.AddCheckbox(updatesGroup, "Check For Updates", Settings.CheckForUpdates, (o, e) =>
            {
                Settings.CheckForUpdates = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            Checkbox showChangelog = GwenHelper.AddCheckbox(updatesGroup, "Enable Changelog", Settings.showChangelog, (o, e) =>
            {
                Settings.showChangelog = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            Panel windowGroup = GwenHelper.CreateHeaderPanel(parent, "Window");
            Checkbox maximizeWin = GwenHelper.AddCheckbox(windowGroup, "Maximize Window On Startup", Settings.showChangelog, (o, e) =>
            {
                Settings.startWindowMaximized = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            Panel savesGroup = GwenHelper.CreateHeaderPanel(parent, "Saves");
            Spinner autosaveMinutes = new Spinner(savesGroup)
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
            _ = GwenHelper.CreateLabeledControl(savesGroup, "Minutes between autosaves", autosaveMinutes);

            Spinner autosaveChanges = new Spinner(savesGroup)
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
            _ = GwenHelper.CreateLabeledControl(savesGroup, "Min changes to start autosaving", autosaveChanges);

            ComboBox defaultSaveType = GwenHelper.CreateLabeledCombobox(savesGroup, "Default Save As Format:");
            _ = defaultSaveType.AddItem(".trk", "", ".trk");
            _ = defaultSaveType.AddItem(".json", "", ".json");
            _ = defaultSaveType.AddItem(".sol", "", ".sol");
            defaultSaveType.ItemSelected += (o, e) =>
            {
                Settings.DefaultSaveFormat = (string)e.SelectedItem.UserData;
                Settings.Save();
            };

            ComboBox defaultQuicksaveType = GwenHelper.CreateLabeledCombobox(savesGroup, "Default Quicksave Format:");
            _ = defaultQuicksaveType.AddItem(".trk", "", ".trk");
            _ = defaultQuicksaveType.AddItem(".json", "", ".json");
            _ = defaultQuicksaveType.AddItem(".sol", "", ".sol");
            defaultQuicksaveType.ItemSelected += (o, e) =>
            {
                Settings.DefaultQuicksaveFormat = (string)e.SelectedItem.UserData;
                Settings.Save();
            };

            ComboBox defaultAutosaveType = GwenHelper.CreateLabeledCombobox(savesGroup, "Default Autosave Format:");
            _ = defaultAutosaveType.AddItem(".trk", "", ".trk");
            _ = defaultAutosaveType.AddItem(".json", "", ".json");
            _ = defaultAutosaveType.AddItem(".sol", "", ".sol");
            defaultAutosaveType.SelectByUserData(Settings.DefaultAutosaveFormat);
            defaultAutosaveType.ItemSelected += (o, e) =>
            {
                Settings.DefaultAutosaveFormat = (string)e.SelectedItem.UserData;
                Settings.Save();
            };

            ComboBox defaultCrashBackupType = GwenHelper.CreateLabeledCombobox(savesGroup, "Default Crash Backup Format:");
            _ = defaultCrashBackupType.AddItem(".trk", "", ".trk");
            _ = defaultCrashBackupType.AddItem(".json", "", ".json");
            _ = defaultCrashBackupType.AddItem(".sol", "", ".sol");
            defaultCrashBackupType.SelectByUserData(Settings.DefaultCrashBackupFormat);
            defaultCrashBackupType.ItemSelected += (o, e) =>
            {
                Settings.DefaultCrashBackupFormat = (string)e.SelectedItem.UserData;
                Settings.Save();
            };

            defaultSaveType.SelectByUserData(Settings.DefaultSaveFormat);
            defaultQuicksaveType.SelectByUserData(Settings.DefaultQuicksaveFormat);
            defaultAutosaveType.SelectByUserData(Settings.DefaultAutosaveFormat);
            defaultAutosaveType.SelectByUserData(Settings.DefaultCrashBackupFormat);
        }
        private void PopulateRBLAnimation(ControlBase parent)
        {
            Panel generalGroup = GwenHelper.CreateHeaderPanel(parent, "RatherBeLunar's Magic Animator Settings");

            //            GwenHelper.AddCheckbox(rblHeader, "Reference frame based animation", Settings.velocityReferenceFrameAnimation, (o, e) =>
            //            {
            //                Settings.velocityReferenceFrameAnimation = ((Checkbox)o).IsChecked;
            //                Settings.Save();
            //            });

            _ = GwenHelper.AddCheckbox(generalGroup, "Convert lines sent to previous frames to scenery", Settings.recededLinesAsScenery, (o, e) =>
            {
                Settings.recededLinesAsScenery = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            _ = GwenHelper.AddCheckbox(generalGroup, "Convert lines sent to forward frames to scenery", Settings.forwardLinesAsScenery, (o, e) =>
            {
                Settings.forwardLinesAsScenery = ((Checkbox)o).IsChecked;
                Settings.Save();
            });

            Spinner animationVelXSpinner = new Spinner(generalGroup)
            {
                Dock = Dock.Bottom,
                Max = 1000,
                Min = -1000,
                Value = Settings.animationRelativeVelX
            };
            Spinner animationVelYSpinner = new Spinner(generalGroup)
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
            _ = GwenHelper.CreateLabeledControl(generalGroup, "Relative Animation X Velocity", animationVelXSpinner);
            _ = GwenHelper.CreateLabeledControl(generalGroup, "Relative Animation Y Velocity", animationVelYSpinner);
        }
    }
}
