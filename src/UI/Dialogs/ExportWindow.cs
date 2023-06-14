using Gwen;
using Gwen.Controls;
using linerider.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace linerider.UI
{
    public class ExportWindow : DialogBase
    {
        private readonly RichLabel _descriptionlabel;
        private readonly Label _error;
        private readonly MainWindow _game;
        private const string howto = "You are about to export your track as a video file. Make sure the end of the track is marked by a flag. " +
            "It will be located in your line rider user directory (Documents/LRA/Renders).\n" +
            "Please allow some minutes depending on your computer speed. " +
            "The window will become unresponsive during this time.\n\n" +
            "After recording, a console window may open to encode the video. " +
            "Closing it will cancel the process and all progress will be lost.";

        private readonly Dictionary<string, Size> resolutions = new Dictionary<string, Size>
        {
            { "360p", new Size(640, 360)},
            { "480p", new Size(854, 480)},
            { "720p", new Size(1280, 720)},
            { "1080p", new Size(1920, 1080)},
            { "1440p", new Size(2560, 1440)},
            { "2160p (4k)", new Size(3840, 2160)},
            { "4320p (8k)", new Size(7680, 4320)}
        };

        public ExportWindow(GameCanvas parent, Editor editor, MainWindow window) : base(parent, editor)
        {
            _game = window;
            Title = "Export Video";
            _descriptionlabel = new RichLabel(this)
            {
                AutoSizeToContents = true
            };
            if (!SafeFrameBuffer.CanRecord)
            {
                _descriptionlabel.AddText(
                    "Video export is not supported on this machine.\n\nSorry.",
                    Skin.Colors.Text.Foreground);
            }
            else
            {
                _descriptionlabel.AddText(howto, Skin.Colors.Text.Foreground);
            }
            _descriptionlabel.Dock = Dock.Top;
            _error = new Label(this)
            {
                Dock = Dock.Top,
                TextColor = Color.Red,
                IsHidden = true,
                Margin = new Margin(0, 0, 0, 10)
            };
            AutoSizeToContents = true;
            MinimumSize = new Size(400, 300);
            MakeModal(true);
            Setup();
        }
        private void SetError(string error)
        {
            _error.IsHidden = false;
            _error.Text = "\n" + error;
        }
        private CheckProperty AddPropertyCheckbox(PropertyTable prop, string label, bool value)
        {
            CheckProperty check = new CheckProperty(null);
            _ = prop.Add(label, check);
            check.IsChecked = value;
            return check;
        }
        private void Setup()
        {
            Panel content = new Panel(this)
            {
                Dock = Dock.Fill,
                AutoSizeToContents = true,
                ShouldDrawBackground = false
            };
            Panel bottomrow = new Panel(content)
            {
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                ShouldDrawBackground = false,

            };
            PropertyTree proptree = new PropertyTree(content)
            {
                Dock = Dock.Top,
                AutoSizeToContents = true,
                Width = 200,
                Margin = new Margin(0, 0, 0, 10)
            };
            PropertyTable table = proptree.Add("Output Settings", 150);
            ComboBoxProperty qualitycb = new ComboBoxProperty(table);

            foreach (KeyValuePair<string, Size> item in resolutions)
            {
                _ = qualitycb.AddItem(item.Key);
            }
            qualitycb.SetValue(Settings.RecordResolution);
            qualitycb.ValueChanged += (o, e) =>
            {
                Settings.RecordResolution = qualitycb.Value;
                Settings.Save();
            };

            _ = table.Add("Quality", qualitycb);

            CheckProperty smoothcheck = AddPropertyCheckbox(
                table,
                "Smooth Playback",
                Settings.RecordSmooth);
            smoothcheck.ValueChanged += (o, e) =>
            {
                Settings.RecordSmooth = smoothcheck.IsChecked;
                Settings.Save();
            };

            CheckProperty music = AddPropertyCheckbox(
                table,
                "Save Music",
                !Settings.MuteAudio && Settings.RecordMusic);
            if (Settings.MuteAudio)
            {
                music.Disable();
            }
            music.ValueChanged += (o, e) =>
            {
                Settings.RecordMusic = music.IsChecked;
                Settings.Save();
            };

            table = proptree.Add("Overlay settings", 150);
            CheckProperty ppf = AddPropertyCheckbox(
                table,
                "Show P/f",
                Settings.RecordShowPpf);
            ppf.ValueChanged += (o, e) =>
            {
                Settings.RecordShowPpf = ppf.IsChecked;
                Settings.Save();
            };
            CheckProperty fps = AddPropertyCheckbox(
                table,
                "Show FPS",
                Settings.RecordShowFps);
            fps.ValueChanged += (o, e) =>
            {
                Settings.RecordShowFps = fps.IsChecked;
                Settings.Save();
            };
            CheckProperty tools = AddPropertyCheckbox(
                table,
                "Show Tools",
                Settings.RecordShowTools);
            tools.ValueChanged += (o, e) =>
            {
                Settings.RecordShowTools = tools.IsChecked;
                Settings.Save();
            };
            CheckProperty hitTest = AddPropertyCheckbox(
               table,
               "Show Hit Test",
               Settings.RecordShowHitTest);
            hitTest.ValueChanged += (o, e) =>
            {
                Settings.RecordShowHitTest = hitTest.IsChecked;
                Settings.Save();
            };
            CheckProperty colorTriggers = AddPropertyCheckbox(
                table,
                "Enable Color Triggers",
                Settings.RecordShowColorTriggers);
            colorTriggers.ValueChanged += (o, e) =>
            {
                Settings.RecordShowColorTriggers = colorTriggers.IsChecked;
                Settings.Save();
            };
            CheckProperty resIndZoom = AddPropertyCheckbox(
                table,
                "Res-Independent Zoom",
                Settings.RecordResIndependentZoom);
            resIndZoom.ValueChanged += (o, e) =>
            {
                Settings.RecordResIndependentZoom = resIndZoom.IsChecked;
                Settings.Save();
            };
            proptree.ExpandAll();
            Button Cancel = new Button(bottomrow)
            {
                Dock = Dock.Right,
                Text = "Cancel",
                Margin = new Margin(10, 0, 0, 0),
            };
            Cancel.Clicked += (o, e) =>
            {
                _ = Close();
            };
            Button ok = new Button(bottomrow)
            {
                Dock = Dock.Right,
                Text = "Export"
            };
            if (!SafeFrameBuffer.CanRecord || !CheckRecord())
            {
                ok.Disable();
            }
            ok.Clicked += (o, e) =>
                {
                    _ = Close();
                    Settings.Recording.ShowPpf = ppf.IsChecked;
                    Settings.Recording.ShowFps = fps.IsChecked;
                    Settings.Recording.ShowTools = tools.IsChecked;
                    Settings.Recording.EnableColorTriggers = colorTriggers.IsChecked;
                    Settings.Recording.ResIndZoom = resIndZoom.IsChecked;
                    Settings.Recording.ShowHitTest = hitTest.IsChecked;

                    Settings.RecordSmooth = smoothcheck.IsChecked;
                    if (!music.IsDisabled)
                    {
                        Settings.RecordMusic = music.IsChecked;
                    }

                    try
                    {
                        Size size = resolutions[qualitycb.SelectedItem.Text];
                        Settings.Recording.RecordingWidth = size.Width;
                        Settings.Recording.RecordingHeight = size.Height;
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new Exception("Invalid resolution: " + qualitycb.SelectedItem.Text);
                    }

                    Settings.Save();
                    Record();
                };
        }
        private bool CheckRecord()
        {
            if (_editor.GetFlag() == null)
            {
                SetError("No flag detected. Place one at the end of the track\nso the recorder knows where to stop.");
                return false;
            }
            else if (_editor.Name == Utils.Constants.DefaultTrackName)
            {
                SetError("Please save your track before recording.");
                return false;
            }
            return true;
        }
        private void Record() => IO.TrackRecorder.RecordTrack(
                _game,
                Settings.RecordSmooth,
                Settings.RecordMusic && !Settings.MuteAudio);
    }
}
