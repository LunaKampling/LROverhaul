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

using Gwen.Controls;
using linerider.Audio;
using linerider.Drawing;
using linerider.IO.ffmpeg;
using linerider.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using Key = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace linerider.IO
{
    internal static class TrackRecorder
    {
        private static byte[] _screenshotbuffer;
        public static unsafe byte[] GrabScreenshot(MainWindow game, int frontbuffer, bool yflip = false)
        {
            if (new GLFWGraphicsContext(game.WindowPtr) == null)
                throw new Exception("missing graphics context");
            int backbuffer = game.MSAABuffer.Framebuffer;
            SafeFrameBuffer.BindFramebuffer(FramebufferTarget.ReadFramebuffer, backbuffer);
            SafeFrameBuffer.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frontbuffer);
            if (yflip) // Screenshots are captured upside-down for some reason but this is corrected during video encoding, so flip here for a correctly oriented screenshot
            {
                SafeFrameBuffer.BlitFramebuffer(0, 0, game.RenderSize.Width, game.RenderSize.Height,
                0, game.RenderSize.Height, game.RenderSize.Width, 0,
                ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            }
            else
            {
                SafeFrameBuffer.BlitFramebuffer(0, 0, game.RenderSize.Width, game.RenderSize.Height,
                0, 0, game.RenderSize.Width, game.RenderSize.Height,
                ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            }
            SafeFrameBuffer.BindFramebuffer(FramebufferTarget.ReadFramebuffer, frontbuffer);

            GL.ReadPixels(0, 0, game.RenderSize.Width, game.RenderSize.Height,
                OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, _screenshotbuffer);
            SafeFrameBuffer.BindFramebuffer(FramebufferTarget.Framebuffer, backbuffer);
            return _screenshotbuffer;
        }
        public static unsafe void SaveScreenshot(int width, int height, byte[] arr, string name)
        {
            SKBitmap output = new(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            fixed (byte* data = arr)
                output.InstallPixels(output.Info, (IntPtr)data);

            var stream = new FileStream(name, FileMode.Create);
            output.Encode(stream, SKEncodedImageFormat.Png, 100);
            output.Dispose();
            stream.Dispose();
        }

        public static bool Recording;
        public static bool RecordingScreenshot;

        public static void RecordScreenshot(MainWindow game)
        {
            Size resolution = new(Settings.ScreenshotWidth, Settings.ScreenshotHeight);
            Size oldsize = game.RenderSize;
            float oldZoomMultiplier = Settings.ZoomMultiplier;
            bool oldHitTest = Settings.Editor.HitTest;

            if (Settings.Recording.ResIndZoom)
                Settings.ZoomMultiplier *= game.ClientSize.X > game.ClientSize.Y * 16 / 9 ? Settings.ScreenshotWidth / (float)game.ClientSize.X : Settings.ScreenshotHeight / (float)game.ClientSize.Y;
            Settings.Editor.HitTest = Settings.Recording.ShowHitTest;
            game.Canvas.Scale = Settings.ZoomMultiplier / oldZoomMultiplier; // Divide just in case anyone modifies the zoom multiplier to not be 1

            using TrackReader trk = game.Track.CreateTrackReader();
            RecordingScreenshot = true;

            game.Canvas.SetCanvasSize(game.RenderSize.Width, game.RenderSize.Height);
            game.Canvas.Layout();

            int frontbuffer = SafeFrameBuffer.GenFramebuffer();
            SafeFrameBuffer.BindFramebuffer(FramebufferTarget.Framebuffer, frontbuffer);

            int rbo2 = SafeFrameBuffer.GenRenderbuffer();
            SafeFrameBuffer.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo2);
            SafeFrameBuffer.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, resolution.Width, resolution.Height);
            SafeFrameBuffer.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, rbo2);

            SafeFrameBuffer.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            _screenshotbuffer = new byte[game.RenderSize.Width * game.RenderSize.Height * 4];// 4 bytes per pixel
            game.Title = Program.WindowTitle + " [Capturing Screenshot]";
            game.ProcessEvents(0.0);

            string date = DateTime.UtcNow.ToString("yyyy-MM-dd hh.mm.ss");
            string dirPath = Path.Combine(Settings.Local.UserDirPath, Constants.RendersFolderName);
            string fileName = game.Track.Name == Constants.InternalDefaultTrackName ? $"${Constants.DefaultTrackName} {date}.png" : $"{game.Track.Name} {date}.png";
            bool recmodesave = Settings.Local.RecordingMode;
            Settings.Local.RecordingMode = true;
            game.Render();
            byte[] screenshotframe = GrabScreenshot(game, frontbuffer, true);
            SaveScreenshot(game.RenderSize.Width, game.RenderSize.Height, screenshotframe, Path.Combine(dirPath, fileName));

            SafeFrameBuffer.BindFramebuffer(FramebufferTarget.Framebuffer, 0); // Delete the FBOs
            SafeFrameBuffer.DeleteFramebuffer(frontbuffer);
            SafeFrameBuffer.DeleteRenderbuffers(1, [rbo2]);

            RecordingScreenshot = false;

            game.RenderSize = oldsize;
            Settings.ZoomMultiplier = oldZoomMultiplier;
            game.Title = Program.WindowTitle;
            Settings.Local.RecordingMode = recmodesave;
            Settings.Editor.HitTest = oldHitTest;

            _ = game.Canvas.SetSize(game.RenderSize.Width, game.RenderSize.Height);
            game.Canvas.Scale = 1.0f;
            _screenshotbuffer = null;
        }

        public static void RecordTrack(MainWindow game, bool smooth, bool music, uint startingFrame = 0)
        {
            if (smooth) startingFrame = startingFrame * Constants.FrameRate / Constants.PhysicsRate;
            Game.RiderFrame flag = game.Track.Flag;
            if (flag == null && !Settings.LockTrackDuration)
                return;
            Size resolution = new(Settings.Recording.RecordingWidth, Settings.Recording.RecordingHeight);
            Size oldsize = game.RenderSize;
            float oldZoom = game.Track.BaseZoom;
            float oldZoomMultiplier = Settings.ZoomMultiplier;
            bool oldHitTest = Settings.Editor.HitTest;
            bool invalid = false;

            if (Settings.Recording.ResIndZoom)
                Settings.ZoomMultiplier *= game.ClientSize.X > game.ClientSize.Y * 16 / 9 ? Settings.Recording.RecordingWidth / (float)game.ClientSize.X : Settings.Recording.RecordingHeight / (float)game.ClientSize.Y;

            Settings.Editor.HitTest = Settings.Recording.ShowHitTest;
            game.Canvas.Scale = Settings.ZoomMultiplier / oldZoomMultiplier; // Divide just in case anyone modifies the zoom multiplier to not be 1

            using TrackReader trk = game.Track.CreateTrackReader();
            Recording = true;
            game.Track.Reset();

            // Set colors back to default for triggers
            Constants.TriggerBGColor = new Color4((byte)game.Track.StartingBGColorR, (byte)game.Track.StartingBGColorG, (byte)game.Track.StartingBGColorB, 255);
            Constants.StaticTriggerBGColor = new Color4((byte)game.Track.StartingBGColorR, (byte)game.Track.StartingBGColorG, (byte)game.Track.StartingBGColorB, 255);
            Constants.StaticTriggerLineColorChange = Color.FromArgb(255, game.Track.StartingLineColorR, game.Track.StartingLineColorG, game.Track.StartingLineColorB);
            Constants.TriggerLineColorChange = Color.FromArgb(255, game.Track.StartingLineColorR, game.Track.StartingLineColorG, game.Track.StartingLineColorB);

            Game.Rider state = game.Track.GetStart();
            int frame = Settings.LockTrackDuration ? game.Canvas.TrackDuration : flag.Moment.Frame;
            game.Canvas.SetCanvasSize(game.RenderSize.Width, game.RenderSize.Height);
            game.Canvas.Layout();

            if (frame > 400) // Many frames, will likely lag the game. Update the window as a fallback.
            {
                game.Title = Program.WindowTitle + " [Validating flag]";
                game.ProcessEvents(0.0);
            }
            for (int i = 0; i < frame; i++)
            {
                state = trk.TickBasic(state);
            }
            if (flag != null)
            {
                for (int i = 0; i < state.Body.Length; i++)
                {
                    if (state.Body[i].Location != flag.State.Body[i].Location ||
                        state.Body[i].Previous != flag.State.Body[i].Previous)
                    {
                        invalid = true;
                        break;
                    }
                }
            }
            int frontbuffer = SafeFrameBuffer.GenFramebuffer();
            SafeFrameBuffer.BindFramebuffer(FramebufferTarget.Framebuffer, frontbuffer);

            int rbo2 = SafeFrameBuffer.GenRenderbuffer();
            SafeFrameBuffer.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo2);
            SafeFrameBuffer.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, resolution.Width, resolution.Height);
            SafeFrameBuffer.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, rbo2);

            SafeFrameBuffer.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            if (!invalid)
            {
                _screenshotbuffer = new byte[game.RenderSize.Width * game.RenderSize.Height * 4];// 4 bytes per pixel
                string errormessage = "An unknown error occured during recording.";
                if (frame <= startingFrame)
                {
                    errormessage = "Starting frame was greater than or equal to the flag frame";
                }
                game.Title = Program.WindowTitle + " [Recording | Hold ESC to cancel]";
                game.ProcessEvents(0.0);
                string filename = Path.Combine(Settings.Local.UserDirPath, Constants.RendersFolderName, $"{game.Track.Name}.mp4");
                Game.RiderFrame flagbackup = flag;
                bool hardexit = false;
                bool recmodesave = Settings.Local.RecordingMode;
                Settings.Local.RecordingMode = true;
                game.Track.StartIgnoreFlag();
                game.Render();
                string dir = Path.Combine(Settings.Local.UserDirPath, Constants.RendersFolderName, $"{game.Track.Name}_rec");
                if (!Directory.Exists(dir))
                {
                    _ = Directory.CreateDirectory(dir);
                }
                byte[] firstframe = GrabScreenshot(game, frontbuffer);
                SaveScreenshot(game.RenderSize.Width, game.RenderSize.Height, firstframe, Path.Combine(dir, "tmp0.png"));
                int framecount = smooth ? (frame + 1) * Constants.FrameRate / Constants.PhysicsRate : frame + 1; // Add an extra frame

                double frametime = 0;
                Stopwatch sw = Stopwatch.StartNew();
                var skipRender = true;
                for (int i = 0; i < framecount; i++)
                {
                    if (hardexit)
                        break;
                    if (i >= startingFrame)
                        skipRender = false;
                    if (smooth)
                    {
                        double oldspot = frametime;
                        frametime += (float)Constants.PhysicsRate / Constants.FrameRate;
                        if (i == 0)
                        {
                            // BUGFIX:
                            // Frame blending uses the previous frame.
                            // So the first frame would be recorded twice,
                            // instead of blended
                            game.Track.Update(1);
                        }
                        else if ((int)frametime != (int)oldspot)
                        {
                            game.Track.Update(1);
                        }
                        double blend = frametime - Math.Truncate(frametime);
                        if (!skipRender) game.Render((float)blend);
                    }
                    else
                    {
                        game.Track.Update(1);
                        if (!skipRender) game.Render();
                    }
                    if (skipRender) continue;
                    try
                    {
                        byte[] screenshot = GrabScreenshot(game, frontbuffer);
                        SaveScreenshot(game.RenderSize.Width, game.RenderSize.Height, screenshot, Path.Combine(dir, $"tmp{i + 1}.png"));
                    }
                    catch (Exception)
                    {
                        hardexit = true;
                        errormessage = "An error occured when saving the frame.\n(Perhaps the resolution chosen is too large?)";
                    }

                    if (game.KeyboardState[Key.Escape] && game.IsFocused)
                    {
                        hardexit = true;
                        errormessage = "The user manually cancelled recording.";
                    }
                    if (sw.ElapsedMilliseconds > 500)
                    {
                        game.Title = string.Format("{0} [Recording {1:P} | Hold ESC to cancel]", Program.WindowTitle, i / (double)framecount);
                        game.ProcessEvents(0.0);
                    }
                }
                game.ProcessEvents(0.0);

                if (!hardexit)
                {
                    double skippedTime = startingFrame * (smooth ? 1.0 / 60 : 1.0 / 40);
                    FFMPEGParameters parameters = new();
                    parameters.AddOption("framerate", smooth ? Constants.FrameRate.ToString() : Constants.PhysicsRate.ToString());
                    if (startingFrame > 0) parameters.AddOption("start_number", startingFrame.ToString());
                    parameters.AddOption("i", $"\"{Path.Combine(dir, "tmp%d.png")}\"");
                    if (music && !string.IsNullOrEmpty(game.Track.Song.Location) && game.Track.Song.Enabled)
                    {
                        string fn = Path.Combine(Settings.Local.UserDirPath, Constants.SongsFolderName, game.Track.Song.Location);

                        parameters.AddOption("ss", (game.Track.Song.Offset + skippedTime).ToString(Program.Culture));
                        parameters.AddOption("i", "\"" + fn + "\"");
                        parameters.AddOption("c:a", "aac");
                    }
                    double duration = (double)framecount / (smooth ? Constants.FrameRate : Constants.PhysicsRate);

                    parameters.AddOption("t", (duration - skippedTime).ToString(Program.Culture));
                    parameters.AddOption("vf", "vflip"); // We save images upside down expecting FFmpeg to flip more efficiently.

                    // FFmpeg x264 encoding doc:
                    // https://trac.ffmpeg.org/wiki/Encode/H.264
                    parameters.AddOption("c:v", "libx264");

                    // We don't care _too_ much about filesize
                    parameters.AddOption("preset", "fast");
                    parameters.AddOption("crf", "17");

                    // Increase player compatibility
                    parameters.AddOption("pix_fmt", "yuv420p");

                    // This optimizes the encoding for animation.
                    // How well lr fits into that category i'm not sure.
                    parameters.AddOption("tune", "animation");

                    parameters.OutputFilePath = filename;
                    bool failed = false;
                    if (File.Exists(filename))
                    {
                        try
                        {
                            File.Delete(filename);
                        }
                        catch
                        {
                            Program.NonFatalError("A file with the name " + game.Track.Name + ".mp4 already exists");
                            failed = true;
                            errormessage = "Cannot replace a file of the existing name " + game.Track.Name + ".mp4.";
                        }
                    }
                    if (!failed)
                    {
                        game.Title = Program.WindowTitle + " [Encoding Video | 0%]";
                        game.ProcessEvents(0.0);
                        try
                        {
                            FFMPEG.Execute(parameters, (string s) =>
                            {
                                int idx = s.IndexOf("frame=", StringComparison.InvariantCulture);
                                if (idx != -1)
                                {
                                    idx += "frame=".Length;
                                    for (; idx < s.Length; idx++)
                                    {
                                        if (char.IsNumber(s[idx]))
                                            break;
                                    }
                                    int space = s.IndexOf(" ", idx, StringComparison.InvariantCulture);
                                    if (space != -1)
                                    {
                                        string sub = s.Substring(idx, space - idx);
                                        int parsedint = -1;
                                        if (int.TryParse(sub, out parsedint))
                                        {
                                            game.Title = Program.WindowTitle + string.Format(" [Encoding Video | {0:P} | Hold ESC to cancel]", parsedint / (double)framecount);
                                            game.ProcessEvents(0.0);
                                            if (game.KeyboardState[Key.Escape] && game.IsFocused)
                                            {
                                                hardexit = true;
                                                errormessage = "The user manually cancelled recording.";
                                                return false;
                                            }
                                        }
                                    }
                                }
                                return true;
                            });
                        }
                        catch (Exception e)
                        {
                            ErrorLog.WriteLine(
                                "ffmpeg error" + Environment.NewLine + e);
                            hardexit = true;
                            errormessage =
                                "An ffmpeg error occured.\n" + e.Message;
                        }
                    }
                }
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {
                    Program.NonFatalError("Unable to delete " + dir);
                }
                if (hardexit)
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch
                    {
                        Program.NonFatalError("Unable to delete " + filename);
                    }
                }
                Settings.Local.RecordingMode = recmodesave;
                game.Title = Program.WindowTitle;
                game.Track.Stop();
                game.ProcessEvents(0.0);
                System.Collections.Generic.List<ControlBase> openwindows = game.Canvas.GetOpenWindows();
                foreach (ControlBase window in openwindows)
                {
                    WindowControl w = window as WindowControl;
                    _ = (w?.Close());
                }
                if (File.Exists(filename))
                {
                    AudioService.Beep();
                }
                else
                {
                    game.Canvas.ShowError(errormessage);
                }
            }
            SafeFrameBuffer.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            SafeFrameBuffer.DeleteFramebuffer(frontbuffer);
            SafeFrameBuffer.DeleteRenderbuffers(1, [rbo2]);
            game.RenderSize = oldsize;
            Recording = false;
            Settings.ZoomMultiplier = oldZoomMultiplier;
            Settings.Editor.HitTest = oldHitTest;
            game.Track.Zoom = oldZoom;

            _ = game.Canvas.SetSize(game.RenderSize.Width, game.RenderSize.Height);
            game.Canvas.Scale = 1.0f;
            _screenshotbuffer = null;

            game.Track.Stop();
        }
    }
}