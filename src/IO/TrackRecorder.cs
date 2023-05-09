﻿//  Author:
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

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gwen;
using Gwen.Controls;
using linerider.Audio;
using linerider.Drawing;
using linerider.Rendering;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using linerider.IO.ffmpeg;
using Key = OpenTK.Input.Key;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using System.Runtime.InteropServices;
using linerider.Utils;

namespace linerider.IO
{
    internal static class TrackRecorder
    {
        private static byte[] _screenshotbuffer;
        public static byte[] GrabScreenshot(MainWindow game, int frontbuffer, bool yflip=false)
        {
            if (GraphicsContext.CurrentContext == null)
                throw new GraphicsContextMissingException();
            var backbuffer = game.MSAABuffer.Framebuffer;
            SafeFrameBuffer.BindFramebuffer(FramebufferTarget.ReadFramebuffer, backbuffer);
            SafeFrameBuffer.BindFramebuffer(FramebufferTarget.DrawFramebuffer, frontbuffer);
            if (yflip) //Screenshots are captured upside-down for some reason but this is corrected during video encoding, so flip here for a correctly oriented screenshot
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
                OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, _screenshotbuffer);
            SafeFrameBuffer.BindFramebuffer(FramebufferTarget.Framebuffer, backbuffer);
            return _screenshotbuffer;
        }
        public static void SaveScreenshot(int width, int height, byte[] arr, string name)
        {
            var output = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = output.LockBits(rect,
                ImageLockMode.ReadWrite, output.PixelFormat);
            var ptr = bmpData.Scan0;
            Marshal.Copy(arr, 0, ptr, arr.Length);

            output.UnlockBits(bmpData);
            output.Save(name, ImageFormat.Png);
            output.Dispose();
        }

        public static bool Recording;
        public static bool RecordingScreenshot;

        public static void RecordScreenshot(MainWindow game)
        {
            var resolution = new Size(Settings.ScreenshotWidth, Settings.ScreenshotHeight);
            var oldsize = game.RenderSize;
            var oldZoomMultiplier = Settings.ZoomMultiplier;
            var oldHitTest = Settings.Editor.HitTest;

            if(Settings.Recording.ResIndZoom)
                Settings.ZoomMultiplier *= game.ClientSize.Width > game.ClientSize.Height * 16 / 9 ? (float)Settings.ScreenshotWidth / (float)game.ClientSize.Width : (float)Settings.ScreenshotHeight / (float)game.ClientSize.Height;
            Settings.Editor.HitTest = Settings.Recording.ShowHitTest;
            game.Canvas.Scale = Settings.ZoomMultiplier / oldZoomMultiplier; //Divide just in case anyone modifies the zoom multiplier to not be 1

            using (var trk = game.Track.CreateTrackReader())
            {
                RecordingScreenshot = true;

                game.Canvas.SetCanvasSize(game.RenderSize.Width, game.RenderSize.Height);
                game.Canvas.Layout();

                var frontbuffer = SafeFrameBuffer.GenFramebuffer();
                SafeFrameBuffer.BindFramebuffer(FramebufferTarget.Framebuffer, frontbuffer);

                var rbo2 = SafeFrameBuffer.GenRenderbuffer();
                SafeFrameBuffer.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo2);
                SafeFrameBuffer.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgb8, resolution.Width, resolution.Height);
                SafeFrameBuffer.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, rbo2);

                SafeFrameBuffer.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

                _screenshotbuffer = new byte[game.RenderSize.Width * game.RenderSize.Height * 3];// 3 bytes per pixel
                game.Title = Program.WindowTitle + " [Capturing Screenshot]";
                game.ProcessEvents();
                
                string filename;

                if(game.Track.Name == Constants.DefaultTrackName)
                    filename = Program.UserDirectory + "Untitled Track" + ".png";
                else
                    filename = Program.UserDirectory + game.Track.Name + ".png";

                var recmodesave = Settings.Local.RecordingMode;
                Settings.Local.RecordingMode = true;
                game.Render();
                var screenshotframe = GrabScreenshot(game, frontbuffer, true);
                SaveScreenshot(game.RenderSize.Width, game.RenderSize.Height, screenshotframe, filename);

                SafeFrameBuffer.BindFramebuffer(FramebufferTarget.Framebuffer, 0); //Delete the FBOs
                SafeFrameBuffer.DeleteFramebuffer(frontbuffer);
                SafeFrameBuffer.DeleteRenderbuffers(1, new[] { rbo2 });

                RecordingScreenshot = false;

                game.RenderSize = oldsize;
                Settings.ZoomMultiplier = oldZoomMultiplier;
                game.Title = Program.WindowTitle;
                Settings.Local.RecordingMode = recmodesave;
                Settings.Editor.HitTest = oldHitTest;

                game.Canvas.SetSize(game.RenderSize.Width, game.RenderSize.Height);
                game.Canvas.Scale = 1.0f;
                _screenshotbuffer = null;
            }
        }

        public static void RecordTrack(MainWindow game, bool smooth, bool music)
        {
            var flag = game.Track.GetFlag();
            if (flag == null) return;
            var resolution = new Size(Settings.RecordingWidth, Settings.RecordingHeight);
            var oldsize = game.RenderSize;
            var oldZoomMultiplier = Settings.ZoomMultiplier;
            var oldHitTest = Settings.Editor.HitTest;
            var invalid = false;

            if(Settings.Recording.ResIndZoom)
                Settings.ZoomMultiplier *= game.ClientSize.Width > game.ClientSize.Height * 16 / 9 ? (float)Settings.RecordingWidth / (float)game.ClientSize.Width : (float)Settings.RecordingHeight / (float)game.ClientSize.Height;
            Settings.Editor.HitTest = Settings.Recording.ShowHitTest;
            game.Canvas.Scale = Settings.ZoomMultiplier /oldZoomMultiplier; //Divide just in case anyone modifies the zoom multiplier to not be 1

            using (var trk = game.Track.CreateTrackReader())
            {
                Recording = true;
                game.Track.Reset();

                //Set colors back to default for triggers
                linerider.Utils.Constants.TriggerBGColor = new Color4((byte)game.Track.StartingBGColorR, (byte)game.Track.StartingBGColorG, (byte)game.Track.StartingBGColorB, (byte)255);
                linerider.Utils.Constants.StaticTriggerBGColor = new Color4((byte)game.Track.StartingBGColorR, (byte)game.Track.StartingBGColorG, (byte)game.Track.StartingBGColorB, (byte)255);
                linerider.Utils.Constants.StaticTriggerLineColorChange = Color.FromArgb(255, game.Track.StartingLineColorR, game.Track.StartingLineColorG, game.Track.StartingLineColorB);
                linerider.Utils.Constants.TriggerLineColorChange = Color.FromArgb(255, game.Track.StartingLineColorR, game.Track.StartingLineColorG, game.Track.StartingLineColorB);

                var state = game.Track.GetStart();
                var frame = flag.FrameID;
                game.Canvas.SetCanvasSize(game.RenderSize.Width, game.RenderSize.Height);
                game.Canvas.Layout();

                if (frame > 400) //many frames, will likely lag the game. Update the window as a fallback.
                {
                    game.Title = Program.WindowTitle + " [Validating flag]";
                    game.ProcessEvents();
                }
                for (var i = 0; i < frame; i++)
                {
                    state = trk.TickBasic(state);
                }
                for (var i = 0; i < state.Body.Length; i++)
                {
                    if (state.Body[i].Location != flag.State.Body[i].Location ||
                        state.Body[i].Previous != flag.State.Body[i].Previous)
                    {
                        invalid = true;
                        break;
                    }
                }
                var frontbuffer = SafeFrameBuffer.GenFramebuffer();
                SafeFrameBuffer.BindFramebuffer(FramebufferTarget.Framebuffer, frontbuffer);

                var rbo2 = SafeFrameBuffer.GenRenderbuffer();
                SafeFrameBuffer.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo2);
                SafeFrameBuffer.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgb8, resolution.Width, resolution.Height);
                SafeFrameBuffer.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, rbo2);

                SafeFrameBuffer.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                if (!invalid)
                {
                    _screenshotbuffer = new byte[game.RenderSize.Width * game.RenderSize.Height * 3];// 3 bytes per pixel
                    string errormessage = "An unknown error occured during recording.";
                    game.Title = Program.WindowTitle + " [Recording | Hold ESC to cancel]";
                    game.ProcessEvents();
                    var filename = Program.UserDirectory + game.Track.Name + ".mp4";
                    var flagbackup = flag;
                    var hardexit = false;
                    var recmodesave = Settings.Local.RecordingMode;
                    Settings.Local.RecordingMode = true;
                    game.Track.StartIgnoreFlag();
                    game.Render();
                    var dir = Program.UserDirectory + game.Track.Name + "_rec";
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    var firstframe = GrabScreenshot(game, frontbuffer);
                    SaveScreenshot(game.RenderSize.Width, game.RenderSize.Height, firstframe, dir + Path.DirectorySeparatorChar + "tmp" + 0 + ".png");
                    int framecount = smooth ? ((frame + 1) * 60) / 40 : frame + 1; //Add a extra frame

                    double frametime = 0;
                    Stopwatch sw = Stopwatch.StartNew();
                    for (var i = 0; i < framecount; i++)
                    {
                        if (hardexit)
                            break;
                        if (smooth)
                        {
                            var oldspot = frametime;
                            frametime += 40f / 60f;
                            if (i == 0)
                            {
                                //bugfix:
                                //frame blending uses the previous frame.
                                //so the first frame would be recorded twice,
                                //instead of blended
                                game.Track.Update(1);
                            }
                            else if ((int)frametime != (int)oldspot)
                            {
                                game.Track.Update(1);
                            }
                            var blend = frametime - Math.Truncate(frametime);
                            game.Render((float)blend);
                        }
                        else
                        {
                            game.Track.Update(1);
                            game.Render();
                        }
                        try
                        {
                            var screenshot = GrabScreenshot(game, frontbuffer);
                            SaveScreenshot(game.RenderSize.Width, game.RenderSize.Height, screenshot, dir + Path.DirectorySeparatorChar + "tmp" + (i + 1) + ".png");
                        }
                        catch (Exception e)
                        {
                            hardexit = true;
                            errormessage = "An error occured when saving the frame.\n(Perhaps the resolution chosen is too large?)";
                        }

                        if (Keyboard.GetState()[Key.Escape] && game.Focused)
                        {
                            hardexit = true;
                            errormessage = "The user manually cancelled recording.";
                        }
                        if (sw.ElapsedMilliseconds > 500)
                        {
                            game.Title = string.Format("{0} [Recording {1:P} | Hold ESC to cancel]", Program.WindowTitle, i / (double)framecount);
                            game.ProcessEvents();
                        }
                    }
                    game.ProcessEvents();

                    if (!hardexit)
                    {
                        var parameters = new FFMPEGParameters();
                        parameters.AddOption("framerate", smooth ? "60" : "40");
                        parameters.AddOption("i", "\"" + dir + Path.DirectorySeparatorChar + "tmp%d.png" + "\"");
                        if (music && !string.IsNullOrEmpty(game.Track.Song.Location) && game.Track.Song.Enabled)
                        {
                            var fn = Program.UserDirectory + "Songs" +
                                     Path.DirectorySeparatorChar +
                                     game.Track.Song.Location;

                            parameters.AddOption("ss", game.Track.Song.Offset.ToString(Program.Culture));
                            parameters.AddOption("i", "\"" + fn + "\"");
                            parameters.AddOption("c:a", "aac");
                        }
                        double duration = framecount / (smooth ? 60.0 : 40.0);
                        parameters.AddOption("t", duration.ToString(Program.Culture));
                        parameters.AddOption("vf", "vflip");//we save images upside down expecting ffmpeg to flip more efficiently.
                        // ffmpeg x264 encoding doc:
                        // https://trac.ffmpeg.org/wiki/Encode/H.264
                        parameters.AddOption("c:v", "libx264");
                        // we don't care _too_ much about filesize
                        parameters.AddOption("preset", "fast");
                        parameters.AddOption("crf", "17");
                        // increase player compatibility:
                        parameters.AddOption("pix_fmt", "yuv420p");
                        // this optimizes the encoding for animation
                        // how well lr fits into that category i'm not sure.
                        parameters.AddOption("tune", "animation");

                        parameters.OutputFilePath = filename;
                        var failed = false;
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
                            game.ProcessEvents();
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
                                        var space = s.IndexOf(" ", idx, StringComparison.InvariantCulture);
                                        if (space != -1)
                                        {
                                            var sub = s.Substring(idx, space - idx);
                                            var parsedint = -1;
                                            if (int.TryParse(sub, out parsedint))
                                            {
                                                game.Title = Program.WindowTitle + string.Format(" [Encoding Video | {0:P} | Hold ESC to cancel]", parsedint / (double)framecount);
                                                game.ProcessEvents();
                                                if (Keyboard.GetState()[Key.Escape] && game.Focused)
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
                                linerider.Utils.ErrorLog.WriteLine(
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
                    game.ProcessEvents();
                    var openwindows = game.Canvas.GetOpenWindows();
                    foreach (var window in openwindows)
                    {
                        var w = window as WindowControl;
                        w?.Close();
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
                SafeFrameBuffer.DeleteRenderbuffers(1, new[] { rbo2 });
                game.RenderSize = oldsize;
                Recording = false;
                Settings.ZoomMultiplier = oldZoomMultiplier;
                Settings.Editor.HitTest = oldHitTest;

                game.Canvas.SetSize(game.RenderSize.Width, game.RenderSize.Height);
                game.Canvas.Scale = 1.0f;
                _screenshotbuffer = null;
            }
        }
    }
}