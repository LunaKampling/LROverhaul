//#define CATCHERRORS
using Gwen;
using Gwen.Controls;
using linerider.IO;
using linerider.IO.SOL;
using linerider.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace linerider.UI
{
    public class LoadWindow : DialogBase
    {
        private readonly string title = "Load Track";
        private readonly TreeControl _tree;
        private readonly Button _load;
        private readonly Button _delete;
        // While there should only be one load window at a time, in extreme 
        // cases the user could load a track, the window closes, the track
        // is loading, and opens another one. we want to consider the other
        // loading window a sister, and wait for its actions to complete.
        private static readonly object _loadsync = new object();
        public LoadWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = title;
            _tree = new TreeControl(this)
            {
                Dock = Dock.Fill
            };
            Panel bottom = new Panel(this)
            {
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                ShouldDrawBackground = false,
                Margin = new Margin(0, 5, 0, 0)
            };

            _load = new Button(bottom)
            {
                Dock = Dock.Left,
                Text = "Load Track",
                Padding = new Padding(10, 0, 10, 0)
            };
            _delete = new Button(bottom)
            {
                Dock = Dock.Right,
                Text = "Delete",
                Padding = new Padding(10, 0, 10, 0)
            };
            _delete.Clicked += (o, e) =>
            {
                if (_tree.SelectedChildren.Count == 1)
                {
                    Delete(_tree.SelectedChildren[0]);
                }
            };
            _load.Clicked += (o, e) =>
            {
                if (_tree.SelectedChildren.Count == 1)
                {
                    Load(_tree.SelectedChildren[0]);
                }
            };
            _ = SetSize(400, 400);
            // AutoSizeToContents = true;
            MakeModal(true);
            Setup();
        }
        private void Setup()
        {
            _tree.SelectionChanged += (o, e) =>
            {
                if (_tree.SelectedChildren.Count == 1)
                {
                    ItemData itemdata = (ItemData)_tree.SelectedChildren[0].UserData;
                    if (itemdata.ItemType == ItemType.Folder)
                    {
                        string[] tracks = TrackIO.EnumerateTrackFiles(itemdata.Path);
                        if (tracks.Length > 0)
                        {
                            Title = title + " - " + Path.GetFileName(tracks[0]);
                        }
                    }
                    else
                    {
                        Title = title + " - " + Path.GetFileName(itemdata.Path);
                    }
                }
                else
                {
                    Title = title;
                }
            };
            string loadedfn;
            using (TrackReader trk = _editor.CreateTrackReader())
            {
                loadedfn = trk.Filename;
            }
            List<string> dirs = GetDirectories();
            List<string> rootfiles = GetTracks(Path.Combine(Settings.Local.UserDirPath, Constants.TracksFolderName));
            foreach (string file in rootfiles)
            {
                TreeNode filenode = _tree.AddNode(Path.GetFileName(file));
                filenode.UserData = new ItemData(ItemType.TrackFile, file, Path.GetFileNameWithoutExtension(file));
                if (loadedfn != null && file == loadedfn)
                {
                    filenode.IsSelected = true;
                }
            }
            foreach (string dir in dirs)
            {
                string dirpath = Path.Combine(Settings.Local.UserDirPath, Constants.TracksFolderName, dir) + Path.DirectorySeparatorChar;
                List<string> tracks = GetTracks(dirpath);
                if (tracks.Count != 0)
                {
                    TreeNode node = _tree.AddNode(dir);
                    node.UserData = new ItemData(ItemType.Folder, dirpath, dir);
                    foreach (string file in tracks)
                    {
                        TreeNode filenode = node.AddNode(Path.GetFileName(file));
                        filenode.UserData = new ItemData(ItemType.TrackFile, file, dir);
                        if (loadedfn != null && file == loadedfn)
                        {
                            node.ExpandAll();
                            filenode.IsSelected = true;
                        }
                    }
                }
            }
        }
        private void Delete(TreeNode node)
        {
            ItemData itemdata = (ItemData)node.UserData;
            switch (itemdata.ItemType)
            {
                case ItemType.Folder:
                {
                    MessageBox box = MessageBox.Show(
                        _canvas,
                        "Are you sure you want to delete this track folder?\nAll files inside of it will be permanently lost.",
                        "Are you sure?",
                        MessageBox.ButtonType.OkCancel);
                    box.RenameButtons("Delete Folder");
                    box.Dismissed +=
                    (o, e) =>
                    {
                        if (e == DialogResult.OK)
                        {
                            Directory.Delete(itemdata.Path, true);
                            node.Parent.RemoveChild(node, false);
                        }
                    };
                }
                break;
                case ItemType.TrackFile:
                {
                    MessageBox box = MessageBox.Show(
                        _canvas,
                        "Are you sure you want to delete this file?\nAll progress will be permanently lost.",
                        $"Delete {Path.GetFileName(itemdata.Path)}?",
                        MessageBox.ButtonType.OkCancel);
                    box.RenameButtons("Delete File");
                    box.Dismissed +=
                    (o, e) =>
                    {
                        if (e == DialogResult.OK)
                        {
                            File.Delete(itemdata.Path);
                            node.Parent.RemoveChild(node, false);
                        }
                    };
                }
                break;
                case ItemType.Sol:
                {
                    MessageBox box = MessageBox.Show(
                        _canvas,
                        "Are you sure you want to delete this sol file?\nAll tracks contained within it will be permanently lost.",
                        $"Delete {Path.GetFileName(itemdata.Path)}?",
                        MessageBox.ButtonType.OkCancel);
                    box.RenameButtons("Delete File");
                    box.Dismissed +=
                    (o, e) =>
                    {
                        if (e == DialogResult.OK)
                        {
                            File.Delete(itemdata.Path);
                            TreeNode parentnode = (TreeNode)node.Parent;
                            parentnode.Parent.RemoveChild(parentnode, false);
                        }
                    };
                }
                break;
            }
        }
        private void Load(TreeNode node)
        {
            ItemData itemdata = (ItemData)node.UserData;
            switch (itemdata.ItemType)
            {
                case ItemType.Folder:
                {
                    string[] tracks = TrackIO.EnumerateTrackFiles(itemdata.Path);
                    if (tracks.Length > 0)
                    {
                        LoadTrack(tracks[0], itemdata.Name);
                        _ = Close();
                    }
                }
                break;
                case ItemType.TrackFile:
                {
                    string path = itemdata.Path;
                    if (path.EndsWith(".sol", StringComparison.OrdinalIgnoreCase))
                    {
                        List<sol_track> sol = SOLLoader.LoadSol(path);
                        if (sol.Count == 0)
                            return;
                        if (sol.Count == 1)
                        {
                            LoadTrackSol(sol[0]);
                            _ = Close();
                        }
                        else if (node.Children.Count == 0)
                        {
                            foreach (sol_track track in sol)
                            {
                                TreeNode solnode = node.AddNode(track.name);
                                solnode.UserData = new ItemData(ItemType.Sol, path, track);
                            }
                            node.ExpandAll();
                        }
                    }
                    else
                    {
                        LoadTrack(itemdata.Path, itemdata.Name);
                        _ = Close();
                    }
                }
                break;
                case ItemType.Sol:
                {
                    LoadTrackSol(itemdata.sol);
                    _ = Close();
                }
                break;
            }
        }
        private List<string> GetTracks(string directory)
        {
            string[] tracks = TrackIO.EnumerateTrackFiles(directory);
            string[] sol = TrackIO.EnumerateSolFiles(directory);
            List<string> ret = new List<string>();
            ret.AddRange(sol);
            ret.AddRange(tracks);
            return ret;
        }
        private List<string> GetDirectories()
        {
            List<string> ret = new List<string>();
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
        private void LoadTrack(string filepath, string name)
        {
            if (!ThreadPool.QueueUserWorkItem((o) => LoadTrackWorker(filepath, name)))
            {
                LoadTrackWorker(filepath, name);
            }
        }
        private void LoadTrackSol(sol_track track)
        {
            if (!ThreadPool.QueueUserWorkItem((o) => LoadSolWorker(track)))
            {
                LoadSolWorker(track);
            }
        }
        private void LoadSolWorker(sol_track sol)
        {
            lock (_loadsync)
            {
                try
                {
                    _editor.ChangeTrack(SOLLoader.LoadTrack(sol));
                }
                catch (Exception e)
                {
                    GameCanvas.QueuedActions.Enqueue(() =>
                    _canvas.ShowError(
                        "Failed to load the track:" +
                        Environment.NewLine +
                        e.Message));
                    return;
                }
            }
        }
        private void LoadTrackWorker(string file, string name)
        {
            lock (_loadsync)
            {
                try
                {
                    _canvas.Loading = true;
                    Track track = file.EndsWith(".trk", StringComparison.InvariantCultureIgnoreCase)
                        ? TRKLoader.LoadTrack(file, name)
                        : file.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase)
                            ? JSONLoader.LoadTrack(file)
                            : throw new Exception("Filetype unknown");
                    _editor.ChangeTrack(track);
                    Settings.LastSelectedTrack = file;
                    Settings.Save();
                }
#if CATCHERRORS
                catch (TrackIO.TrackLoadException e)
                {
                    GameCanvas.QueuedActions.Enqueue(() =>
                    _canvas.ShowError(
                        "Failed to load the track:" +
                        Environment.NewLine +
                        e.Message));
                    return;
                }
                catch (Exception e)
                {
                    GameCanvas.QueuedActions.Enqueue(() =>
                    _canvas.ShowError(
                        "An unknown error occured while loading the track:\n" +
                        Environment.NewLine +
                        e.Message));
                    return;
                }
#endif
                finally
                {
                    _canvas.Loading = false;
                }
            }
        }
        private enum ItemType
        {
            Sol,
            TrackFile,
            Folder
        }
        private class ItemData
        {
            public ItemType ItemType;
            public string Path;
            public string Name;
            public sol_track sol;
            public ItemData(ItemType type, string path, string name)
            {
                ItemType = type;
                Path = path;
                Name = name;
            }
            public ItemData(ItemType type, string path, sol_track data)
            {
                ItemType = type;
                Path = path;
                sol = data;
            }
        }
    }
}
