using Gwen;
using Gwen.Controls;
using linerider.IO.ffmpeg;
using linerider.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace linerider.UI
{
    public class FFmpegDownloadWindow : DialogBase
    {
        private readonly ProgressBar _progress;
        private readonly Label _text;
        private string ffmpeg_download
        {
            get
            {
                if (OperatingSystem.IsMacOS())
                    return $"{Constants.FfmpegHelperHeader}-mac.zip";
                else if (OperatingSystem.IsWindows())
                    return $"{Constants.FfmpegHelperHeader}-win.zip";
                else if (OperatingSystem.IsLinux())
                    return $"{Constants.FfmpegHelperHeader}-linux.zip";
                return null;
            }
        }
        private WebClient _webclient;
        private long _lastbytes = 0;
        private Stopwatch _downloadwatch;
        public FFmpegDownloadWindow(GameCanvas parent, Editor editor) : base(parent, editor)
        {
            Title = "Downloading FFmpeg";
            AutoSizeToContents = true;
            MakeModal(true);
            Setup();
            MinimumSize = new Size(250, MinimumSize.Height);
            _text = new Label(this)
            {
                Dock = Dock.Top,
                Text = "Downloading..."
            };
            _progress = new ProgressBar(this)
            {
                Dock = Dock.Bottom,
            };
        }
        protected override void CloseButtonPressed(ControlBase control, EventArgs args)
        {
            base.CloseButtonPressed(control, args);
            _webclient?.CancelAsync();
        }
        public override void Dispose()
        {
            base.Dispose();
            _webclient.Dispose();
        }
        private void DownloadComplete(string fn)
        {
            string dir = FFMPEG.ffmpeg_dir;
            string error = null;
            try
            {
                ZipArchive archive = ZipFile.OpenRead(fn);
                if (archive.GetEntry("ffmpeg.exe") == null &&
                    archive.GetEntry("ffmpeg") == null)
                {
                    error = "Unable to locate ffmpeg in archive";
                }
                else
                {
                    _ = Directory.CreateDirectory(dir);
                    ZipFile.ExtractToDirectory(fn, dir);
                }
            }
            catch (Exception e)
            {
                error = "An unknown error occured when extracting ffmpeg\n" + e.Message;
            }
            GameCanvas.QueuedActions.Enqueue(() =>
            {
                if (error == null)
                {
                    if (!File.Exists(FFMPEG.ffmpeg_path))
                    {
                        _canvas.ShowError("Download completed, but ffmpeg could not be found");
                    }
                    else
                    {
                        _ = MessageBox.Show(_canvas, "ffmpeg was successfully downloaded\nYou can now record tracks.", "Success!", true, true);
                    }
                }
                _ = Close();
            });
        }
        private void UpdateDownloadSpeed(long currentbytes)
        {
            TimeSpan elapsed = _downloadwatch.Elapsed;
            if (elapsed.TotalSeconds < 0.5)
                return;

            long diff = currentbytes - _lastbytes;
            double rate = diff / elapsed.TotalSeconds;
            int kbs = (int)(rate / 1024);
            int mbs = kbs / 1024;
            _text.Text = mbs > 0 ? $"Downloading... {mbs} mb/s" : $"Downloading... {kbs} kb/s";
            _lastbytes = currentbytes;
            _downloadwatch.Restart();
        }
        private void Setup()
        {
            try
            {
                string filename = Path.GetTempFileName();
                _webclient = new WebClient();
                _webclient.DownloadProgressChanged += (o, e) =>
                {
                    _progress.Value = e.ProgressPercentage / 100f;
                    UpdateDownloadSpeed(e.BytesReceived);
                };
                _webclient.DownloadFileCompleted += (o, e) =>
                {
                    if (e.Error != null)
                    {
                        if (!e.Cancelled)
                        {
                            GameCanvas.QueuedActions.Enqueue(() =>
                            {
                                _ = Close();
                                _canvas.ShowError("Download failed\n\n" + e.Error);
                            });
                        }
                    }
                    else if (!e.Cancelled)
                    {
                        DownloadComplete(filename);
                    }
                };
                string address = ffmpeg_download;
                if (address == null)
                {
                    _canvas.ShowError("Download failed:\r\nUnknown platform");
                    return;
                }
                _downloadwatch = Stopwatch.StartNew();
                _webclient.DownloadFileAsync(new Uri(address), filename);

            }
            catch (Exception e)
            {
                _webclient?.CancelAsync(); // Cleanup
                _canvas.ShowError("Download failed\n\n" + e);
                _ = Close();
            }
        }
    }
}