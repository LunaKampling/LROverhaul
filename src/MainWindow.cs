//#define debuggrid
//#define debugcamera
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
using linerider.Addons;
using linerider.Audio;
using linerider.Drawing;
using linerider.Drawing.RiderModel;
using linerider.IO;
using linerider.Rendering;
using linerider.Tools;
using linerider.UI;
using linerider.Utils;
using linerider.Game;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Key = OpenTK.Input.Key;
using MessageBox = Gwen.Controls.MessageBox;

namespace linerider
{
    public class MainWindow : GameWindow
    {
        public bool firstGameUpdate = true; //Run this only on the first update (probably a better way to do this, this is probably bad)

        public CursorsHandler Cursors = new CursorsHandler();
        public MsaaFbo MSAABuffer;
        public GameCanvas Canvas;
        public bool ReversePlayback = false;

        public Vector2d MouseGamePos = new Vector2d(0.0, 0.0);
        public Size RenderSize
        {
            get
            {
                if (TrackRecorder.Recording)
                {
                    return new Size(Settings.Recording.RecordingWidth, Settings.Recording.RecordingHeight);
                }
                else if (TrackRecorder.RecordingScreenshot)
                {
                    return new Size(Settings.ScreenshotWidth, Settings.ScreenshotHeight);
                }
                return ClientSize;
            }
            set => ClientSize = value;
        }
        public Vector2d ScreenTranslation => -ScreenPosition;
        public Vector2d ScreenPosition
            => Track.Camera.GetViewport(
                Track.Zoom,
                RenderSize.Width,
                RenderSize.Height).Vector;

        public Editor Track { get; }
        private bool _uicursor = false;
        private Gwen.Input.OpenTK _input;
        private bool _dragRider;
        private bool _invalidated;
        private readonly Stopwatch _autosavewatch = Stopwatch.StartNew();
        private Rectangle _previouswindowpos;
        public MainWindow()
            : base(
                Constants.WindowSize.Width,
                Constants.WindowSize.Height,
                new GraphicsMode(new ColorFormat(24), 0, 0, 0, ColorFormat.Empty),
                   string.Empty,
                   GameWindowFlags.Default,
                   DisplayDevice.Default,
                   2,
                   0,
                   GraphicsContextFlags.Default)
        {
            SafeFrameBuffer.Initialize();
            Track = new Editor();
            VSync = VSyncMode.On;
            Context.ErrorChecking = false;
            WindowBorder = WindowBorder.Resizable;
            RenderFrame += (o, e) => { Render(); };
            UpdateFrame += (o, e) => { GameUpdate(); };
            new Thread(AutosaveThreadRunner) { IsBackground = true, Name = "Autosave" }.Start();
            GameService.Initialize(this);
            AddonManager.Initialize(this);
            RegisterHotkeys();
            if (Settings.startWindowMaximized)
                WindowState = WindowState.Maximized;
        }

        public override void Dispose()
        {
            if (Canvas != null)
            {
                Canvas.Dispose();
                Canvas.Skin.Dispose();
                Canvas.Skin.DefaultFont.Dispose();
                Canvas.Renderer.Dispose();
                Canvas = null;
            }
            base.Dispose();
        }

        public bool ShouldXySnap() => Settings.Editor.ForceXySnap || InputUtils.CheckPressed(Hotkey.ToolXYSnap);
        public void Render(float blend = 1)
        {
            if (Settings.LockTrackDuration && Track.Playing && !TrackRecorder.Recording)
            {
                if (Track.Offset >= Canvas.TrackDuration)
                    Track.TogglePause();
                if (Track.Offset > Canvas.TrackDuration)
                {
                    Track.SetFrame(Canvas.TrackDuration);
                    Track.UpdateCamera();
                }
            }

            bool shouldrender = _invalidated ||
            Canvas.NeedsRedraw ||
            Track.Playing ||
            Canvas.Loading ||
            Track.NeedsDraw ||
            CurrentTools.CurrentTool.NeedsRender;
            if (shouldrender)
            {
                _invalidated = false;
                BeginOrtho();
                if (blend == 1 && Settings.SmoothPlayback && Track.Playing && !Canvas.Scrubbing)
                {
                    blend = Math.Min(1, (float)Track.Scheduler.ElapsedPercent);
                    if (ReversePlayback)
                        blend = 1 - blend;
                    Track.Camera.BeginFrame(blend, Track.Zoom);
                }
                else
                {
                    Track.Camera.BeginFrame(blend, Track.Zoom);
                }
                if (Track.Playing && CurrentTools.PencilTool.Active)
                {
                    CurrentTools.PencilTool.OnMouseMoved(InputUtils.GetMouse());
                }
                if (Track.Playing && CurrentTools.SmoothPencilTool.Active)
                {
                    CurrentTools.SmoothPencilTool.OnMouseMoved(InputUtils.GetMouse());
                }
                if ((Settings.PreviewMode || TrackRecorder.Recording) && !(TrackRecorder.Recording && !Settings.Recording.EnableColorTriggers))
                {
                    /* BG triggers and Line trigger updates */
                    if (Track.Offset == 0)
                    {
                        Constants.TriggerBGColor = new Color4((byte)Track.StartingBGColorR, (byte)Track.StartingBGColorG, (byte)Track.StartingBGColorB, 255);
                        Constants.StaticTriggerBGColor = new Color4((byte)Track.StartingBGColorR, (byte)Track.StartingBGColorG, (byte)Track.StartingBGColorB, 255);
                        Constants.StaticTriggerLineColorChange = Color.FromArgb(255, Track.StartingLineColorR, Track.StartingLineColorG, Track.StartingLineColorB);
                        Constants.TriggerLineColorChange = Color.FromArgb(255, Track.StartingLineColorR, Track.StartingLineColorG, Track.StartingLineColorB);
                        GL.ClearColor(Constants.TriggerBGColor);
                    }
                    else
                    {
                        Constants.TriggerBGColor = Track.Timeline.GetFrameBackgroundColor(Track.Offset);
                        GL.ClearColor(Constants.TriggerBGColor);
                    }
                }
                else
                {
                    GL.ClearColor(Settings.Computed.BGColor);
                    Constants.TriggerLineColorChange = Settings.Computed.LineColor;
                }

                MSAABuffer.Use(RenderSize.Width, RenderSize.Height);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Enable(EnableCap.Blend);

                if (Settings.DrawFloatGrid)
                {
                    GameRenderer.DrawFloatGrid();
                }

                if ((InputUtils.Check(Hotkey.PreferenceDrawDebugGrid) && !TrackRecorder.Recording) || Settings.DrawCollisionGrid)
                {
                    GameRenderer.DbgDrawGrid();
                }

                if ((InputUtils.Check(Hotkey.PreferenceDrawDebugGrid) && !TrackRecorder.Recording) || Settings.DrawAGWs)
                {
                    GameRenderer.DrawAGWs();
                }

                Track.Render(blend);

                if ((InputUtils.Check(Hotkey.PreferenceDrawDebugCamera) && !TrackRecorder.Recording) || Settings.DrawCamera)
                {
                    GameRenderer.DbgDrawCamera();
                }

                Canvas.RenderCanvas();
                MSAABuffer.End();

                SwapBuffers();
                // There are machines and cases where a refresh may not hit the screen without calling glfinish...
                GL.Finish();
                double seconds = Track.FramerateWatch.Elapsed.TotalSeconds;
                Track.FramerateCounter.AddFrame(seconds);
                Track.FramerateWatch.Restart();
            }
            if (!Focused && !TrackRecorder.Recording)
            {
                Thread.Sleep(16);
            }
            else
            if (!Track.Playing &&
                    !Canvas.NeedsRedraw &&
                    !Track.NeedsDraw &&
                    !CurrentTools.CurrentTool.Active)
            {
                Thread.Sleep(10);
            }
        }
        private void GameUpdateHandleInput()
        {
            if (InputUtils.HandleMouseMove(out int x, out int y) && !Canvas.IsModalOpen)
            {
                CurrentTools.CurrentTool.OnMouseMoved(new Vector2d(x, y));
            }
        }
        /// <summary>
        /// Indefinitely run the autosave function
        /// </summary>
        private void AutosaveThreadRunner()
        {
            while (true)
            {
                Thread.Sleep(1000 * 60 * Settings.autosaveMinutes); // Settings.autosaveMinutes minutes
                try
                {
                    Track.BackupTrack(false);
                }
                catch
                {
                    // Do nothing
                }
            }
        }
        public void GameUpdate()
        {
            if (firstGameUpdate)
            {
                if (Settings.Local.Version != AssemblyInfo.Version)
                {
                    string subVer = AssemblyInfo.SubVersion;
                    if (subVer != "closed" && subVer != "test")
                        Canvas.ShowChangelog();
                }
                firstGameUpdate = false;
            }

            // Check if scarf and rider model are actual
            RiderLoader.Validate();

            // Regular code starts here
            GameUpdateHandleInput();
            int updates = Track.Scheduler.UnqueueUpdates();
            if (updates > 0)
            {
                Invalidate();
                if (Track.Playing)
                {
                    if (InputUtils.Check(Hotkey.PlaybackZoom))
                        Track.ZoomBy(0.08f);
                    else if (InputUtils.Check(Hotkey.PlaybackUnzoom))
                        Track.ZoomBy(-0.08f);
                }
            }

            if (Track.Playing)
            {
                if (ReversePlayback)
                {
                    for (int i = 0; i < updates; i++)
                    {
                        Track.PreviousFrame();
                        Track.UpdateCamera(true);
                    }
                }
                else
                {
                    Track.Update(updates);
                }
            }
            if (Program.NewVersion != null)
            {
                Canvas.ShowOutOfDate();
            }
            AudioService.EnsureSync();

            // LRL
            Coordinates.CoordsUpdate();

            if (CurrentTools._current == CurrentTools.SmoothPencilTool)
            {
                CurrentTools.SmoothPencilTool.UpdateSmooth();
            }

            
        }

        // Used to be static
        public void Invalidate() => _invalidated = true;
        public void UpdateCursor()
        {
            MouseCursor cursor;

            if (_uicursor)
                cursor = Canvas.Platform.CurrentCursor;
            else if (Track.Playing || _dragRider)
                cursor = Cursors.List[CursorsHandler.Type.Default];
            else if (CurrentTools.CurrentTool != null)
                cursor = CurrentTools.CurrentTool.Cursor;
            else
            {
                cursor = MouseCursor.Default;
                Debug.Fail("Improperly handled UpdateCursor");
            }
            if (cursor != Cursor)
            {
                Cursor = cursor;
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            Shaders.Load();
            MSAABuffer = new MsaaFbo();
            Gwen.Renderer.OpenTK renderer = new Gwen.Renderer.OpenTK();

            Gwen.Texture skinpng = renderer.CreateTexture(GameResources.defaultskin);

            Fonts f = GameResources.font_liberation_sans_15;

            Gwen.Skin.TexturedBase skin = new Gwen.Skin.TexturedBase(renderer,
            skinpng,
            GameResources.defaultcolors
            )
            { DefaultFont = f.Default };

            CurrentTools.Init();

            Canvas = new GameCanvas(skin, this, renderer, f);

            _input = new Gwen.Input.OpenTK(this);
            _input.Initialize(Canvas);
            Canvas.ShouldDrawBackground = false;

            Cursors.Reload();
            ScarfColors.RemoveAll();
            Program.UpdateCheck();
            Track.AutoLoadPrevious();
            Cursors.Refresh(Canvas);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Track.Camera.OnResize();
            try
            {
                Canvas.SetCanvasSize(RenderSize.Width, RenderSize.Height);
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            try
            {
                InputUtils.UpdateMouse(e.Mouse);
                if (TrackRecorder.Recording)
                    return;
                bool r = _input.ProcessMouseMessage(e);
                _uicursor = _input.MouseCaptured;
                if (Canvas.GetOpenWindows().Count != 0)
                {
                    UpdateCursor();
                    return;
                }
                if (!r)
                {
                    InputUtils.ProcessMouseHotkeys();
                    if (!Track.Playing)
                    {
                        bool dragstart = false;
                        MouseGamePos = ScreenPosition + new Vector2d(e.X, e.Y) / Track.Zoom;
                        if (Track.Offset == 0 &&
                         e.Button == MouseButton.Left &&
                        InputUtils.Check(Hotkey.EditorMoveStart))
                        {
                            dragstart = Game.Rider.GetBounds(
                                Track.GetStart()).Contains(
                                    MouseGamePos.X,
                                    MouseGamePos.Y);
                            if (dragstart)
                            {
                                // 5 is arbitrary, but i assume that's a decent
                                // place to assume the user has done "work"
                                if (!Track.MoveStartWarned && Track.LineCount > 5)
                                {
                                    MessageBox popup = MessageBox.Show(Canvas,
                                        "You're about to move the start position of the rider." +
                                        " This cannot be undone, and may drastically change how your track plays." +
                                        "\nAre you sure you want to do this?", "Warning", MessageBox.ButtonType.OkCancel);
                                    popup.RenameButtons("I understand");
                                    popup.Dismissed += (o, args) =>
                                    {
                                        if (popup.Result == Gwen.DialogResult.OK)
                                        {
                                            Track.MoveStartWarned = true;
                                        }
                                    };
                                }
                                else
                                {
                                    _dragRider = dragstart;
                                }
                            }
                        }
                        if (!_dragRider && !dragstart)
                        {
                            if (e.Button == MouseButton.Left)
                            {
                                CurrentTools.CurrentTool.OnMouseDown(new Vector2d(e.X, e.Y));
                            }
                            else if (e.Button == MouseButton.Right)
                            {
                                CurrentTools.CurrentTool.OnMouseRightDown(new Vector2d(e.X, e.Y));
                            }
                        }
                    }
                    else if (CurrentTools.CurrentTool == CurrentTools.PencilTool || CurrentTools.CurrentTool == CurrentTools.SmoothPencilTool)
                    {
                        if (e.Button == MouseButton.Left)
                        {
                            CurrentTools.CurrentTool.OnMouseDown(new Vector2d(e.X, e.Y));
                        }
                    }
                }
                UpdateCursor();
                Invalidate();
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            try
            {
                InputUtils.UpdateMouse(e.Mouse);
                if (TrackRecorder.Recording)
                    return;
                _dragRider = false;
                bool r = _input.ProcessMouseMessage(e);
                _uicursor = _input.MouseCaptured;
                _ = InputUtils.CheckCurrentHotkey();
                if (!r || CurrentTools.CurrentTool.IsMouseButtonDown)
                {
                    if (!CurrentTools.CurrentTool.IsMouseButtonDown &&
                        Canvas.GetOpenWindows().Count != 0)
                    {
                        UpdateCursor();
                        return;
                    }
                    if (e.Button == MouseButton.Left)
                    {
                        CurrentTools.CurrentTool.OnMouseUp(new Vector2d(e.X, e.Y));
                    }
                    else if (e.Button == MouseButton.Right)
                    {
                        CurrentTools.CurrentTool.OnMouseRightUp(new Vector2d(e.X, e.Y));
                    }
                }
                UpdateCursor();
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            try
            {
                Vector2d pos = new Vector2d(e.X, e.Y);
                MouseGamePos = ScreenPosition + pos / Track.Zoom;
                InputUtils.UpdateMouse(e.Mouse);
                if (TrackRecorder.Recording)
                    return;
                bool r = _input.ProcessMouseMessage(e);
                _uicursor = _input.MouseCaptured;
                if (Canvas.GetOpenWindows().Count != 0)
                {
                    UpdateCursor();
                    return;
                }
                if (_dragRider)
                {
                    Track.Stop();
                    using (TrackWriter trk = Track.CreateTrackWriter())
                    {
                        trk.Track.StartOffset = MouseGamePos;
                        Track.Reset();
                        Track.NotifyTrackChanged();
                    }
                    Invalidate();
                }
                if (CurrentTools.CurrentTool.RequestsMousePrecision)
                {
                    CurrentTools.CurrentTool.OnMouseMoved(new Vector2d(e.X, e.Y));
                }

                if (r)
                {
                    Invalidate();
                }
                UpdateCursor();
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            try
            {
                InputUtils.UpdateMouse(e.Mouse);
                if (TrackRecorder.Recording)
                    return;
                if (_input.ProcessMouseMessage(e))
                    return;
                if (Canvas.GetOpenWindows().Count != 0)
                {
                    UpdateCursor();
                    return;
                }
                float delta = (float.IsNaN(e.DeltaPrecise) ? e.Delta : e.DeltaPrecise) * Settings.ScrollSensitivity;
                Point cursorPos = new Point(e.X, e.Y);
                Track.ZoomBy(delta / 6, cursorPos);

                UpdateCursor();
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            try
            {
                if (!e.IsRepeat)
                {
                    InputUtils.KeyDown(e.Key);
                }
                if (e.Key == Key.B)
                {
                    Layer layer = new Layer();
                    Random random = new Random();
                    Track.GetTrack()._layers.currentLayer.SetColor(Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)));
                    Track.GetTrack()._layers.currentLayer.Rerender(Track);
                    Track.GetTrack()._layers.AddLayer(layer);
                    layer.SetColor(Color.FromArgb(255, (int)layer.ID * 4, (int)layer.ID * 2));
                }
                InputUtils.UpdateKeysDown(e.Keyboard, e.Modifiers);
                if (TrackRecorder.Recording)
                    return;
                KeyModifiers mod = e.Modifiers;
                if (_input.ProcessKeyDown(e))
                {
                    return;
                }
                if (e.Key == Key.Escape && !e.IsRepeat)
                {
                    System.Collections.Generic.List<ControlBase> openwindows = Canvas.GetOpenWindows();
                    if (openwindows != null && openwindows.Count >= 1)
                    {
                        foreach (ControlBase v in openwindows)
                        {
                            _ = ((WindowControl)v).Close();
                            Invalidate();
                        }
                        return;
                    }
                }
                if (
                    Canvas.IsModalOpen ||
                    (!Track.Playing && CurrentTools.CurrentTool.OnKeyDown(e.Key)) ||
                    _dragRider)
                {
                    UpdateCursor();
                    Invalidate();
                    return;
                }
                InputUtils.ProcessKeyboardHotkeys();
                UpdateCursor();
                Invalidate();
                KeyboardState input = e.Keyboard;
                if (!input.IsAnyKeyDown)
                    return;

                bool toggleFullscreen = ((input.IsKeyDown(Key.AltLeft) || input.IsKeyDown(Key.AltRight)) && input.IsKeyDown(Key.Enter)) || input.IsKeyDown(Key.F11);
                if (toggleFullscreen)
                {
                    if (WindowBorder == WindowBorder.Resizable)
                    {
                        _previouswindowpos = new Rectangle(X, Y, RenderSize.Width, RenderSize.Height);
                        WindowBorder = WindowBorder.Hidden;
                        RenderSize = Constants.ScreenSize;
                        X = 0;
                        Y = 0;
                    }
                    else
                    {
                        WindowBorder = WindowBorder.Resizable;
                        RenderSize = new Size(_previouswindowpos.Width, _previouswindowpos.Height);
                        X = _previouswindowpos.X;
                        Y = _previouswindowpos.Y;
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            try
            {
                InputUtils.UpdateKeysDown(e.Keyboard, e.Modifiers);
                if (TrackRecorder.Recording)
                    return;
                _ = InputUtils.CheckCurrentHotkey();
                _ = CurrentTools.CurrentTool.OnKeyUp(e.Key);
                _ = _input.ProcessKeyUp(e);
                UpdateCursor();
                Invalidate();
            }
            catch (Exception ex)
            {
                // SDL2 backend eats exceptions.
                // we have to manually crash.
                Program.Crash(ex, true);
                Close();
            }
        }

        public void StopTools() => CurrentTools.CurrentTool.Stop();
        public void StopHandTool()
        {
            if (CurrentTools.CurrentTool == CurrentTools.PanTool)
            {
                CurrentTools.PanTool.Stop();
            }
        }

        private void BeginOrtho()
        {
            if (RenderSize.Height > 0 && RenderSize.Width > 0)
            {
                GL.Viewport(new Rectangle(0, 0, RenderSize.Width, RenderSize.Height));
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, RenderSize.Width, RenderSize.Height, 0, 0, 1);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
            }
        }
        private void RegisterHotkeys()
        {
            RegisterPlaybackHotkeys();
            RegisterEditorHotkeys();
            RegisterSettingHotkeys();
            RegisterPopupHotkeys();
            RegisterCoordinateHotkeys();
        }

        private void RegisterCoordinateHotkeys()
        {
            InputUtils.RegisterHotkey(Hotkey.CopyX0, () => true, () =>
            {
                Coordinates.xClipboard = true;
                Coordinates.yClipboard = false;
                Coordinates.integerClipboard = 0;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyY0, () => true, () =>
            {
                Coordinates.xClipboard = false;
                Coordinates.yClipboard = true;
                Coordinates.integerClipboard = 0;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyX1, () => true, () =>
            {
                Coordinates.xClipboard = true;
                Coordinates.yClipboard = false;
                Coordinates.integerClipboard = 1;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyY1, () => true, () =>
            {
                Coordinates.xClipboard = false;
                Coordinates.yClipboard = true;
                Coordinates.integerClipboard = 1;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyX2, () => true, () =>
            {
                Coordinates.xClipboard = true;
                Coordinates.yClipboard = false;
                Coordinates.integerClipboard = 2;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyY2, () => true, () =>
            {
                Coordinates.xClipboard = false;
                Coordinates.yClipboard = true;
                Coordinates.integerClipboard = 2;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyX3, () => true, () =>
            {
                Coordinates.xClipboard = true;
                Coordinates.yClipboard = false;
                Coordinates.integerClipboard = 3;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyY3, () => true, () =>
            {
                Coordinates.xClipboard = false;
                Coordinates.yClipboard = true;
                Coordinates.integerClipboard = 3;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyX4, () => true, () =>
            {
                Coordinates.xClipboard = true;
                Coordinates.yClipboard = false;
                Coordinates.integerClipboard = 4;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyY4, () => true, () =>
            {
                Coordinates.xClipboard = false;
                Coordinates.yClipboard = true;
                Coordinates.integerClipboard = 4;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyX5, () => true, () =>
            {
                Coordinates.xClipboard = true;
                Coordinates.yClipboard = false;
                Coordinates.integerClipboard = 5;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyY5, () => true, () =>
            {
                Coordinates.xClipboard = false;
                Coordinates.yClipboard = true;
                Coordinates.integerClipboard = 5;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyX6, () => true, () =>
            {
                Coordinates.xClipboard = true;
                Coordinates.yClipboard = false;
                Coordinates.integerClipboard = 6;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyY6, () => true, () =>
            {
                Coordinates.xClipboard = false;
                Coordinates.yClipboard = true;
                Coordinates.integerClipboard = 6;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyX7, () => true, () =>
            {
                Coordinates.xClipboard = true;
                Coordinates.yClipboard = false;
                Coordinates.integerClipboard = 7;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyY7, () => true, () =>
            {
                Coordinates.xClipboard = false;
                Coordinates.yClipboard = true;
                Coordinates.integerClipboard = 7;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyX8, () => true, () =>
            {
                Coordinates.xClipboard = true;
                Coordinates.yClipboard = false;
                Coordinates.integerClipboard = 8;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyY8, () => true, () =>
            {
                Coordinates.xClipboard = false;
                Coordinates.yClipboard = true;
                Coordinates.integerClipboard = 8;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyX9, () => true, () =>
            {
                Coordinates.xClipboard = true;
                Coordinates.yClipboard = false;
                Coordinates.integerClipboard = 9;
                Coordinates.SaveToClipboard();
            });
            InputUtils.RegisterHotkey(Hotkey.CopyY9, () => true, () =>
            {
                Coordinates.xClipboard = false;
                Coordinates.yClipboard = true;
                Coordinates.integerClipboard = 9;
                Coordinates.SaveToClipboard();
            });
        }
        private void RegisterSettingHotkeys()
        {
            InputUtils.RegisterHotkey(Hotkey.PreferenceOnionSkinning, () => true, () =>
            {
                Settings.OnionSkinning = !Settings.OnionSkinning;
                Settings.Save();
                Track.Invalidate();
            });
            
            InputUtils.RegisterHotkey(Hotkey.TogglePreviewMode, () => true, () =>
            {
                Settings.PreviewMode = !Settings.PreviewMode;

                Settings.Save();
                Track.Invalidate();
            });

            InputUtils.RegisterHotkey(Hotkey.ToggleCameraLock, () => true, () =>
            {
                if (Track.Paused)
                    Settings.Local.LockCamera = !Settings.Local.LockCamera;
            });

            InputUtils.RegisterHotkey(Hotkey.PreferenceAllCheckboxSettings, () => true, () =>
            {
                bool newState = !(Settings.Editor.DrawContactPoints || Settings.Editor.MomentumVectors
                    || Settings.Editor.HitTest || Settings.Editor.RenderGravityWells);

                Settings.Editor.DrawContactPoints = newState;
                Settings.Editor.MomentumVectors = newState;
                Settings.Editor.HitTest = newState;
                Settings.Editor.RenderGravityWells = newState;

                Settings.Save();
                Track.Invalidate();
            });

            InputUtils.RegisterHotkey(Hotkey.PreferenceInvisibleRider, () => true, () =>
            {
                Settings.InvisibleRider = !Settings.InvisibleRider;
                Settings.Save();
                Track.Invalidate();
            });

            RegisterAddonSettingHotkeys();
        }
        private void RegisterAddonSettingHotkeys()
        {
            InputUtils.RegisterHotkey(Hotkey.MagicAnimateAdvanceFrame, () => true, () =>
            {
                MagicAnimator.AdvanceFrame();
            });
            InputUtils.RegisterHotkey(Hotkey.MagicAnimateRecedeFrame, () => true, () =>
            {
                MagicAnimator.RecedeFrame();
            });
            InputUtils.RegisterHotkey(Hotkey.MagicAnimateRecedeMultiFrame, () => true, () =>
            {
                MagicAnimator.RecedeMultiFrame();
            });
        }
        private void RegisterPlaybackHotkeys()
        {
            InputUtils.RegisterHotkey(Hotkey.PlaybackForward, () => true, () =>
            {
                StopTools();
                if (Track.Paused)
                    Track.TogglePause();
                ReversePlayback = false;
                UpdateCursor();
            },
            () =>
            {
                if (!Track.Paused)
                    Track.TogglePause();
                ReversePlayback = false;
                Track.UpdateCamera();
                UpdateCursor();
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackBackward, () => true, () =>
            {
                StopTools();
                if (Track.Paused)
                    Track.TogglePause();
                ReversePlayback = true;
                UpdateCursor();
            },
            () =>
            {
                if (!Track.Paused)
                    Track.TogglePause();
                ReversePlayback = false;
                Track.UpdateCamera();
                UpdateCursor();
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackFrameNext, () => true, () =>
            {
                if (Settings.LockTrackDuration && Track.Offset >= Canvas.TrackDuration)
                    return;

                StopHandTool();
                if (!Track.Paused)
                    Track.TogglePause();
                Track.NextFrame();
                Invalidate();
                Track.UpdateCamera();
                if (CurrentTools.CurrentTool.IsMouseButtonDown)
                {
                    CurrentTools.CurrentTool.OnMouseMoved(InputUtils.GetMouse());
                }
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.PlaybackFramePrev, () => true, () =>
            {
                StopHandTool();
                if (!Track.Paused)
                    Track.TogglePause();
                Track.PreviousFrame();
                Invalidate();
                Track.UpdateCamera(true);
                if (CurrentTools.CurrentTool.IsMouseButtonDown)
                {
                    CurrentTools.CurrentTool.OnMouseMoved(InputUtils.GetMouse());
                }
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.ToggleSlowmo, () => true, () =>
            {
                if (Track.Scheduler.UpdatesPerSecond !=
                Settings.SlowmoSpeed)
                {
                    Track.Scheduler.UpdatesPerSecond = Settings.SlowmoSpeed;
                }
                else
                {
                    Track.ResetSpeedDefault(false);
                }
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackIterationNext, () => !Track.Playing, () =>
            {
                if (Settings.LockTrackDuration && (Track.Offset >= Canvas.TrackDuration && Track.IterationsOffset == 6))
                    return;

                StopTools();

                if (!Track.Paused)
                    Track.TogglePause();

                if (Track.IterationsOffset != 6)
                {
                    Track.IterationsOffset++;
                }
                else
                {
                    Track.NextFrame();
                    Track.IterationsOffset = 0;
                    Track.UpdateCamera();
                }
                Track.InvalidateRenderRider();
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.PlaybackIterationPrev, () => !Track.Playing, () =>
            {
                if (Track.Offset == 0)
                    return;

                StopTools();
                if (Track.IterationsOffset > 0)
                {
                    Track.IterationsOffset--;
                }
                else
                {
                    Track.PreviousFrame();
                    Track.IterationsOffset = 6;
                    Invalidate();
                    Track.UpdateCamera();
                }
                Track.InvalidateRenderRider();
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.PlaybackResetCamera, () => true, () => Track.ResetCamera());
            InputUtils.RegisterHotkey(Hotkey.PlaybackStart, () => true, () =>
            {
                StopTools();
                Track.StartFromFlag();
                Track.ResetSpeedDefault();
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackStartSlowmo, () => true, () =>
            {
                StopTools();
                Track.StartFromFlag();
                Track.Scheduler.UpdatesPerSecond = Settings.SlowmoSpeed;
            });
            InputUtils.RegisterHotkey(Hotkey.PlaybackStartIgnoreFlag, () => true, () =>
            {
                StopTools();
                Track.StartIgnoreFlag();
                Track.ResetSpeedDefault();
            });
        }
        private void RegisterPopupHotkeys()
        {
            InputUtils.RegisterHotkey(Hotkey.LoadWindow, () => true, () =>
            {
                Canvas.ShowLoadDialog();
            });

            InputUtils.RegisterHotkey(Hotkey.PreferencesWindow,
            () => !CurrentTools.CurrentTool.Active,
            () =>
            {
                Canvas.ShowPreferencesDialog();
            });
            InputUtils.RegisterHotkey(Hotkey.GameMenuWindow,
            () => !CurrentTools.CurrentTool.Active,
            () =>
            {
                Canvas.ShowGameMenuWindow();
            });
            InputUtils.RegisterHotkey(Hotkey.TriggerMenuWindow,
            () => !CurrentTools.CurrentTool.Active,
            () =>
            {
                Canvas.ShowTriggerWindow();
            });
            InputUtils.RegisterHotkey(Hotkey.SaveAsWindow,
            () => !CurrentTools.CurrentTool.Active,
            () =>
            {
                Canvas.ShowSaveDialog();
            });
            InputUtils.RegisterHotkey(Hotkey.TrackPropertiesWindow,
            () => !CurrentTools.CurrentTool.Active,
            () =>
            {
                Canvas.ShowTrackPropertiesDialog();
            });
            InputUtils.RegisterHotkey(Hotkey.Quicksave, () => true, () =>
            {
                Track.QuickSave();
            });
        }
        private void RegisterEditorHotkeys()
        {
            InputUtils.RegisterHotkey(Hotkey.EditorQuickPan, () => !Track.Playing && !Canvas.IsModalOpen, () =>
            {
                CurrentTools.QuickPan = true;
                Invalidate();
                UpdateCursor();
            },
            () =>
            {
                CurrentTools.QuickPan = false;
                Invalidate();
                UpdateCursor();
            });
            InputUtils.RegisterHotkey(Hotkey.EditorDragCanvas, () => !Track.Playing && !Canvas.IsModalOpen, () =>
            {
                Vector2d mouse = InputUtils.GetMouse();
                CurrentTools.QuickPan = true;
                CurrentTools.PanTool.OnMouseDown(new Vector2d(mouse.X, mouse.Y));
            },
            () =>
            {
                if (CurrentTools.QuickPan)
                {
                    Vector2d mouse = InputUtils.GetMouse();
                    CurrentTools.PanTool.OnMouseUp(new Vector2d(mouse.X, mouse.Y));
                    CurrentTools.QuickPan = false;
                }
            });

            InputUtils.RegisterHotkey(Hotkey.EditorUndo, () => !Track.Playing, () =>
            {
                CurrentTools.CurrentTool.Cancel();
                object hint = Track.UndoManager.Undo();
                CurrentTools.CurrentTool.OnUndoRedo(true, hint);
                Invalidate();
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.EditorRedo, () => !Track.Playing, () =>
            {
                CurrentTools.CurrentTool.Cancel();
                object hint = Track.UndoManager.Redo();
                CurrentTools.CurrentTool.OnUndoRedo(false, hint);
                Invalidate();
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.EditorRemoveLatestLine, () => !Track.Playing, () =>
            {
                if (!Track.Playing)
                {
                    StopTools();
                    using (TrackWriter trk = Track.CreateTrackWriter())
                    {
                        CurrentTools.CurrentTool.Stop();
                        Game.GameLine l = trk.GetNewestLine();
                        if (l != null)
                        {
                            Track.UndoManager.BeginAction();
                            trk.RemoveLine(l);
                            Track.UndoManager.EndAction();
                        }

                        Track.NotifyTrackChanged();
                        Invalidate();
                    }
                }
            },
            null,
            repeat: true);
            InputUtils.RegisterHotkey(Hotkey.EditorFocusStart, () => !Track.Playing, () =>
            {
                using (TrackReader trk = Track.CreateTrackReader())
                {
                    Game.GameLine l = trk.GetOldestLine();
                    if (l != null)
                    {
                        Track.Camera.SetFrameCenter(l.Position1);
                        Invalidate();
                    }
                }
            });
            InputUtils.RegisterHotkey(Hotkey.EditorFocusLastLine, () => !Track.Playing, () =>
            {
                using (TrackReader trk = Track.CreateTrackReader())
                {
                    Game.GameLine l = trk.GetNewestLine();
                    if (l != null)
                    {
                        Track.Camera.SetFrameCenter(l.Position1);
                        Invalidate();
                    }
                }
            });
            InputUtils.RegisterHotkey(Hotkey.EditorCycleToolSetting, () => !Track.Playing, () =>
            {
                if (CurrentTools.CurrentTool.ShowSwatch)
                {
                    CurrentTools.CurrentTool.Swatch.IncrementSelectedMultiplier();
                    Invalidate();
                }
            });
            InputUtils.RegisterHotkey(Hotkey.ToolToggleOverlay, () => !Track.Playing, () =>
            {
                Settings.Local.TrackOverlay = !Settings.Local.TrackOverlay;
            });
            InputUtils.RegisterHotkey(Hotkey.EditorFocusFlag, () => !Track.Playing, () =>
            {
                Game.RiderFrame flag = Track.Flag;
                if (flag != null)
                {
                    Track.Camera.SetFrameCenter(flag.State.CalculateCenter());
                    Invalidate();
                }
            });
            InputUtils.RegisterHotkey(Hotkey.EditorFocusRider, () => !Track.Playing, () =>
            {
                Track.Camera.SetFrameCenter(Track.RenderRider.CalculateCenter());
                Invalidate();
            });
            InputUtils.RegisterHotkey(Hotkey.EditorCancelTool,
            () => CurrentTools.CurrentTool.Active,
            () =>
            {
                Tool tool = CurrentTools.CurrentTool;
                SelectSubtool selecttool = CurrentTools.SelectSubtool;
                if (tool == selecttool)
                {
                    selecttool.CancelSelection();
                }
                else
                {
                    tool.Cancel();
                }
                Invalidate();
            });
            InputUtils.RegisterHotkey(Hotkey.ToolCopy, () => !Track.Playing &&
            CurrentTools.CurrentTool == CurrentTools.SelectSubtool, () =>
            {
                CurrentTools.SelectSubtool.Copy();
                Invalidate();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.ToolCut, () => !Track.Playing &&
            CurrentTools.CurrentTool == CurrentTools.SelectSubtool, () =>
            {
                CurrentTools.SelectSubtool.Cut();
                Invalidate();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.ToolPaste, () => !Track.Playing &&
            (CurrentTools.CurrentTool == CurrentTools.SelectSubtool ||
            CurrentTools.CurrentTool == CurrentTools.SelectTool), () =>
            {
                CurrentTools.SelectSubtool.Paste();
                Invalidate();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.ToolDelete, () => !Track.Playing &&
            CurrentTools.CurrentTool == CurrentTools.SelectSubtool, () =>
            {
                CurrentTools.SelectSubtool.Delete();
                Invalidate();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.ToolCopyValues, () => !Track.Playing &&
            (CurrentTools.CurrentTool == CurrentTools.SelectSubtool ||
            CurrentTools.CurrentTool == CurrentTools.SelectTool), () =>
            {
                CurrentTools.SelectSubtool.CopyValues();
                Invalidate();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.ToolPasteValues, () => !Track.Playing &&
            (CurrentTools.CurrentTool == CurrentTools.SelectSubtool ||
            CurrentTools.CurrentTool == CurrentTools.SelectTool), () =>
            {
                CurrentTools.SelectSubtool.PasteValues(Track.GetTrack()._layers.currentLayer);
                Invalidate();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.ToolSwitchBlue, () => !Track.Playing &&
            CurrentTools.CurrentTool == CurrentTools.SelectSubtool, () =>
            {
                CurrentTools.SelectSubtool.SwitchLineType(LineType.Standard);
                Invalidate();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.ToolSwitchRed, () => !Track.Playing &&
            CurrentTools.CurrentTool == CurrentTools.SelectSubtool, () =>
            {
                CurrentTools.SelectSubtool.SwitchLineType(LineType.Acceleration);
                Invalidate();
            },
            null,
            repeat: false);
            InputUtils.RegisterHotkey(Hotkey.ToolSwitchGreen, () => !Track.Playing &&
            CurrentTools.CurrentTool == CurrentTools.SelectSubtool, () =>
            {
                CurrentTools.SelectSubtool.SwitchLineType(LineType.Scenery);
                Invalidate();
            },
            null,
            repeat: false);
        }
    }
}