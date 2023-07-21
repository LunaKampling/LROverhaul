using Gwen;
using Gwen.Controls;
using linerider.Utils;
using System.Drawing;
using System.IO;

namespace linerider.UI
{
    public class UserFolderTransferrerWindow : DialogBase
    {
        private enum TransferMode
        {
            Nothing,
            JustSettings,
            AllData,
        }
        private bool IsPortable => Settings.Computed.IsUserDirPortable;
        private string CurrentUserFolder => Settings.Local.UserDirPath;

        private Label _nothingDescription;
        private Label _justSettingsDescription;
        private Label _allDataDescription;
        private TransferMode _mode;

        public UserFolderTransferrerWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            //Title = "User Folder Transferrer";
            Title = IsPortable ? "Switching from portable mode" : "Switching to portable mode";
            MinimumSize = new Size(200, MinimumSize.Height);
            AutoSizeToContents = true;

            DisableResizing();
            MakeModal(true);
            Setup();
        }

        private void Setup()
        {
            _ = new Label(this)
            {
                Dock = Dock.Top,
                Text = "Current user folder location:",
            };
            _ = new Label(this)
            {
                TextColor = Color.Gray,
                Dock = Dock.Top,
                Text = CurrentUserFolder,
            };

            _ = new Label(this)
            {
                Dock = Dock.Top,
                Text = "New user folder location:",
                Margin = new Margin(0, 10, 0, 0),
            };
            _ = new Label(this)
            {
                TextColor = Color.Gray,
                Dock = Dock.Top,
                Text = IsPortable ? Program.UserDirectory : Program.UserPortableDirectory,
            };

            _ = new Label(this)
            {
                Dock = Dock.Top,
                Text = "Which data you want to transfer?",
                Margin = new Margin(0, 20, 0, 5),
            };

            RadioButtonGroup transferOpts = new RadioButtonGroup(this)
            {
                Dock = Dock.Top,
                ShouldDrawBackground = false,
                Margin = new Margin(0, 0, 0, 5),
            };
            RadioButton nothingOpt = transferOpts.AddOption("Don't copy data");
            RadioButton justSettingsOpt = transferOpts.AddOption("Copy settings");
            RadioButton allDataOpt = transferOpts.AddOption("Move entire folder");

            _nothingDescription = new Label(this)
            {
                Dock = Dock.Top,
                TextColor = IsPortable ? Color.Red : Color.Gray,
                Text = IsPortable
                    ? $"All data in \"{Constants.UserDirPortableFolderName}\" WILL BE DELETED."
                    : $"This will create a \"{Constants.UserDirPortableFolderName}\" folder with default settings\nnext to the executable file.",
            };
            _justSettingsDescription = new Label(this)
            {
                Dock = Dock.Top,
                TextColor = IsPortable ? Color.Red : Color.Gray,
                Text = IsPortable
                    ? "This will copy just the settings file.\nAll other data (tracks, songs, etc.) WILL BE DELETED."
                    : "This will copy just the settings file.\nAll other data (tracks, songs, etc.) will stay in the old folder.",
            };
            _allDataDescription = new Label(this)
            {
                Dock = Dock.Top,
                TextColor = Color.Gray,
                Text = IsPortable
                    ? "This will move the entire folder to a new place.\nAll data (tracks, songs, etc.) will be merged."
                    : "This will move the entire folder to a new place.\n",
            };

            nothingOpt.CheckChanged += (o, e) => ProcessRadioButton(TransferMode.Nothing);
            justSettingsOpt.CheckChanged += (o, e) => ProcessRadioButton(TransferMode.JustSettings);
            allDataOpt.CheckChanged += (o, e) => ProcessRadioButton(TransferMode.AllData);

            allDataOpt.Select();

            Panel buttonsGroup = new Panel(this)
            {
                Dock = Dock.Bottom,
                AutoSizeToContents = true,
                ShouldDrawBackground = false,
                Margin = new Margin(0, 5, 0, 0),
            };

            Button cancelBtn = new Button(buttonsGroup)
            {
                Dock = Dock.Right,
                Text = "Cancel",
                Margin = new Margin(10, 0, 0, 0),
            };
            Button processBtn = new Button(buttonsGroup)
            {
                Dock = Dock.Right,
                Text = "Process"
            };
            cancelBtn.Clicked += (o, e) => Close();
            processBtn.Clicked += (o, e) =>
            {
                Disable();

                string msg = "Are you sure?";

                if (_mode == TransferMode.AllData)
                    msg = "Are you sure?\n\nDepending on folder size, it may take some time.";
                else if (IsPortable && _mode != TransferMode.AllData)
                    msg = "Are you sure?\n\nAll untransferred data will be lost forever, it cannot by undone.";

                MessageBox mbox = MessageBox.Show(_canvas, msg, "Confirm", MessageBox.ButtonType.OkCancel, true);
                mbox.Dismissed += async (o2, e2) =>
                {
                    if (e2 == DialogResult.OK)
                    {
                        processBtn.Text = "Processing...";
                        _ = mbox.Close();

                        await System.Threading.Tasks.Task.Delay(100);

                        if (IsPortable)
                            GoStatic();
                        else
                            GoPortable();

                        _ = Close();
                    }
                    else
                    {
                        Enable();
                        _ = mbox.Close();
                    }
                };
            };
        }

        private void ProcessRadioButton(TransferMode mode)
        {
            _mode = mode;

            _nothingDescription.IsHidden = mode != TransferMode.Nothing;
            _justSettingsDescription.IsHidden = mode != TransferMode.JustSettings;
            _allDataDescription.IsHidden = mode != TransferMode.AllData;
        }

        private void GoPortable()
        {
            string oldDir = CurrentUserFolder;
            string newDir = Program.UserPortableDirectory;
            string successMsg = "We are portable now.";

            Settings.Local.UserDirPath = newDir;

            if (_mode == TransferMode.Nothing || _mode == TransferMode.JustSettings)
            {
                if (!Directory.Exists(newDir))
                    Directory.CreateDirectory(newDir);
            }

            switch (_mode)
            {
                case TransferMode.Nothing:
                    Settings.Load(); // Load handler will prepare a new folder with fresh settings file
                    _editor.InitCamera();
                    _editor.RedrawAllLines();
                    break;
                case TransferMode.JustSettings:
                    Settings.LastSelectedTrack = "";
                    Settings.ForceSave(); // Save empty "LastSelectedTrack"
                    Settings.Load(); // Load handler will prepare a new folder
                    break;
                case TransferMode.AllData:
                    if (Directory.Exists(newDir))
                        Directory.Delete(newDir);

                    try
                    {
                        // Try to move folder the fast way
                        Directory.Move(oldDir, newDir);
                    }
                    catch
                    {
                        // Fallback to slow yet more reliable way
                        bool flawless = ForceMoveDirectory(new DirectoryInfo(oldDir), new DirectoryInfo(newDir));
                        if (!flawless)
                            successMsg += $"\n\nNote: Inaccessible files detected. We did best on trasferring their copies but you better check the old folder ({oldDir}) manually.";
                    }

                    break;
            }

            MessageBox.Show(_canvas, successMsg, "Hooray!", MessageBox.ButtonType.Ok, true, true);
        }
        private void GoStatic()
        {
            string oldDir = CurrentUserFolder;
            string newDir = Program.UserDirectory;
            bool flawless = true;
            string successMsg = "We are back to static now.";

            Settings.Local.UserDirPath = newDir;

            if (_mode == TransferMode.Nothing || _mode == TransferMode.JustSettings)
            {
                if (!Directory.Exists(newDir))
                    Directory.CreateDirectory(newDir);

                string ignoreIndicator = Path.Combine(oldDir, "TO_BE_DELETED");

                if (!File.Exists(ignoreIndicator))
                    File.Create(ignoreIndicator).Dispose();
            }

            switch (_mode)
            {
                case TransferMode.Nothing:
                    Settings.Load(); // Load handler will either prepare a new folder or read existing config
                    _editor.InitCamera();
                    _editor.RedrawAllLines();
                    break;
                case TransferMode.JustSettings:
                    Settings.LastSelectedTrack = "";
                    Settings.ForceSave(); // Save empty "LastSelectedTrack"
                    Settings.Load(); // Load handler will prepare a new folder if doesn't exist
                    break;
                case TransferMode.AllData:
                    if (Directory.Exists(newDir))
                    {
                        // Move file by file because there's no other way to merge two folders I guess
                        flawless = ForceMoveDirectory(new DirectoryInfo(oldDir), new DirectoryInfo(newDir));
                    }
                    else
                    {
                        try
                        {
                            // Try to move folder the fast way
                            Directory.Move(oldDir, newDir);
                        }
                        catch
                        {
                            // Fallback to slow yet more reliable way
                            flawless = ForceMoveDirectory(new DirectoryInfo(oldDir), new DirectoryInfo(newDir));
                            if (flawless)
                            {
                                try
                                {
                                    Directory.Delete(oldDir, true);
                                }
                                catch
                                {
                                    flawless = false;
                                }
                            }
                        }
                    }
                    break;
            }


            if (_mode == TransferMode.Nothing || _mode == TransferMode.JustSettings)
            {
                try
                {
                    Directory.Delete(oldDir, true);
                }
                catch
                {
                    flawless = false;
                }
            }

            if (!flawless)
                successMsg += $"\n\nWarning: Inaccessible files detected. We did best on trasferring their copies but you better close the game, check, and delete (!) the old folder ({oldDir}) manually.";

            MessageBox.Show(_canvas, successMsg, "Hooray!", MessageBox.ButtonType.Ok, true, true);
        }

        /// <summary>
        /// Move directory file by file. If it meets locked file, it get copied. If copying fails, exception is thrown.
        /// </summary>
        /// <param name="source">Source directory</param>
        /// <param name="target">Traget directory</param>
        /// <returns>Whether moving was flawless</returns>
        private bool ForceMoveDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            bool isSourceSymlink = source.Attributes.HasFlag(FileAttributes.ReparsePoint);
            bool flawless = true;

            if (source.FullName.ToLower() == target.FullName.ToLower())
                return flawless;

            if (isSourceSymlink)
            {
                // .NET 4.8 can't create symbolic links.
                // For now, leave them unprocessed.
                flawless = false;

                // When ported to .NET 6+, uncomment this line instead:
                //Directory.CreateSymbolicLink(source.FullName, target.FullName);
            }
            else
            {
                if (!Directory.Exists(target.FullName))
                    Directory.CreateDirectory(target.FullName);

                // Move (or copy on fail) each file into its new directory
                foreach (FileInfo file in source.GetFiles())
                {
                    try
                    {
                        string targetFilePath = Path.Combine(target.ToString(), file.Name);
                        if (File.Exists(targetFilePath))
                            File.Delete(targetFilePath);

                        file.MoveTo(targetFilePath);
                    }
                    catch
                    {
                        flawless = false;
                        file.CopyTo(Path.Combine(target.ToString(), file.Name), true);
                    }
                }

                // Move each subdirectory using recursion
                foreach (DirectoryInfo nextSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(nextSourceSubDir.Name);
                    bool subdirFlawless = ForceMoveDirectory(nextSourceSubDir, nextTargetSubDir);
                    if (flawless)
                        flawless = subdirFlawless;
                }

                // Try to delete source directory if empty and successed
                try
                {
                    if (flawless)
                        source.Delete();
                }
                catch
                {
                    flawless = false;
                }
            }

            return flawless;
        }
    }
}
