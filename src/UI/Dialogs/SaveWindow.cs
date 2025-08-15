using Gwen;
using Gwen.Controls;
using linerider.IO;
using linerider.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace linerider.UI
{
    public class SaveWindow : DialogBase
    {
        private readonly ComboBox _savelist;
        private readonly TextBox _namebox;
        private readonly Label _errorbox;
        private readonly DropDownButton _savebutton;
        private const string CreateNewTrack = "<create new track>";
        public SaveWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = "Save Track As...";
            RichLabel l = new(this)
            {
                Dock = Dock.Top,
                AutoSizeToContents = true
            };
            l.AddText($"Files are saved to the \"{Constants.TracksFolderName}\" folder.", Skin.Colors.Text.Foreground);
            _errorbox = new Label(this)
            {
                Dock = Dock.Top,
                TextColor = Color.Red,
                Text = "",
                Margin = new Margin(0, 0, 0, 5)
            };
            ControlBase bottomcontainer = new(this)
            {
                Margin = Margin.Zero,
                Dock = Dock.Bottom,
                AutoSizeToContents = true
            };
            _savelist = new ComboBox(bottomcontainer)
            {
                Dock = Dock.Top,
                Margin = new Margin(0, 0, 0, 5)
            };
            _namebox = new TextBox(bottomcontainer)
            {
                Dock = Dock.Fill,
            };
            _savebutton = new DropDownButton(bottomcontainer)
            {
                Dock = Dock.Right,
                Text = "Save (" + Settings.DefaultSaveFormat + ")",
                UserData = Settings.DefaultSaveFormat,
                Margin = new Margin(2, 0, 0, 0),
            };
            _savebutton.DropDownClicked += (o, e) =>
              {
                  Menu pop = new(_canvas);
                  pop.AddItem(".track.json").Clicked += (o2, e2) =>
                  {
                      _savebutton.Text = "Save (.json)";
                      _savebutton.UserData = ".json";
                  };
                  pop.AddItem(".sol (outdated)").Clicked += (o2, e2) =>
                  {
                      _savebutton.Text = "Save (.sol)";
                      _savebutton.UserData = ".sol";
                  };
                  pop.AddItem(".trk (outdated)").Clicked += (o2, e2) =>
                  {
                      _savebutton.Text = "Save (.trk)";
                      _savebutton.UserData = ".trk";
                  };
                  pop.Open(Pos.Center);
              };
            _savebutton.Clicked += (o, e) =>
            {
                Save();
            };
            Padding = Padding.Zero;
            AutoSizeToContents = true;
            MakeModal(true);
            Setup();
            MinimumSize = new Size(250, MinimumSize.Height);
        }
        private void Setup()
        {
            _ = _savelist.AddItem(CreateNewTrack);
            _savelist.SelectByText(CreateNewTrack);
            List<string> directories = GetDirectories();
            foreach (string dir in directories)
            {
                _ = _savelist.AddItem(dir);
            }
            _savelist.SelectByText(_editor.Name);
        }
        private void Save()
        {
            string filetype = (string)_savebutton.UserData;
            string filename = _namebox.Text;
            string folder = _savelist.SelectedItem.Text;
            if (folder == CreateNewTrack)
            {
                folder = filename;
            }
            if (
                !TrackIO.CheckValidFilename(
                    folder + filename) ||
                    filename == Constants.InternalDefaultTrackName ||
                    (folder.Length == 0))
            {
                _errorbox.Text = "Error\n* Save name is invalid";
                return;
            }
            using (TrackWriter trk = _editor.CreateTrackWriter())
            {
                Game.GameLine l = trk.GetOldestLine();
                if (l == null)
                {
                    _errorbox.Text = "Track must have at least one line";
                    return;
                }
                trk.Name = folder;
                try
                {
                    string filepath;
                    switch (filetype)
                    {
                        case ".trk":
                        {
                            filepath = TrackIO.SaveTrackToFile(trk.Track, filename);
                            Settings.LastSelectedTrack = filepath;
                            Settings.ForceSave();
                        }
                        break;
                        case ".sol":
                        {
                            if (!CheckSol(trk))
                                return;
                            // Purposely do not set this to lastselectedtrack
                            // Currently it's deemed non-performant and slow
                            _ = TrackIO.SaveToSOL(trk.Track, filename);
                        }
                        break;
                        case ".json":
                        {
                            filepath = TrackIO.SaveTrackToJsonFile(trk.Track, filename);
                            Settings.LastSelectedTrack = filepath;
                            Settings.ForceSave();
                        }
                        break;
                        default:
                            throw new InvalidOperationException("Unknown save filetype");
                    }
                    _editor.ResetTrackChangeCounter();
                }
                catch (Exception e)
                {
                    _errorbox.Text = "Error\n* An unknown error occured...\n" + e.Message;
                    return;
                }
            }
            _ = Close();
        }
        private bool CheckSol(TrackReader trk)
        {
            Dictionary<string, bool> features;
            features = trk.GetFeatures();
            _ = features.TryGetValue(TrackFeatures.six_one, out bool six_one);
            _ = features.TryGetValue(TrackFeatures.redmultiplier, out bool redmultiplier);
            _ = features.TryGetValue(TrackFeatures.scenerywidth, out bool scenerywidth);
            if (six_one || redmultiplier || scenerywidth)
            {
                string msg = "*Error\nThe following features are incompatible with .sol:\n";
                if (six_one)
                {
                    msg += "\n* The track is based on 6.1";
                }
                if (redmultiplier)
                {
                    msg += "\n* Red line multipliers";
                }
                if (scenerywidth)
                {
                    msg += "\n* Variable width scenery";
                }
                _errorbox.Text = msg;
                return false;
            }
            return true;
        }
        private List<string> GetDirectories()
        {
            List<string> ret = [];
            string dir = Path.Combine(Settings.Local.UserDirPath, Constants.TracksFolderName);
            if (!Directory.Exists(dir))
                _ = Directory.CreateDirectory(dir);
            string[] folders = Directory.GetDirectories(dir);
            foreach (string folder in folders)
            {
                string trackname = Path.GetFileName(folder);
                if (trackname != Constants.InternalDefaultTrackName)
                {
                    ret.Add(trackname);
                }
            }
            return ret;
        }
    }
}