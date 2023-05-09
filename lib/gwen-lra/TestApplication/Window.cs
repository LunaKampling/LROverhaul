using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Gwen;
using Gwen.Controls;
using System.Drawing;
using TestApplication.Tests;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
namespace TestApplication
{
    public class Window : GameWindow
    {
        public class PlatformImpl : Gwen.Platform.Neutral.PlatformImplementation
        {

            private GameWindow game;
            public PlatformImpl(GameWindow game)
            {
                this.game = game;
            }
            private void SetGameCursor(OpenTK.MouseCursor cursor)
            {
                if (game.Cursor != cursor)
                {
                    game.Cursor = cursor;
                }
            }
            public override bool SetClipboardText(string text)
            {
                bool ret = false;
                Thread staThread = new Thread(
                    () =>
                    {
                        try
                        {
                            Clipboard.SetText(text);
                            ret = true;
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    });
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();
                // at this point either you have clipboard data or an exception
                return ret;
            }
            public override string GetClipboardText()
            {
                // code from http://forums.getpaint.net/index.php?/topic/13712-trouble-accessing-the-clipboard/page__view__findpost__p__226140
                string ret = String.Empty;
                Thread staThread = new Thread(
                    () =>
                    {
                        try
                        {
                            if (!Clipboard.ContainsText())
                                return;
                            ret = Clipboard.GetText();
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    });
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();
                staThread.Join();
                // at this point either you have clipboard data or an exception
                return ret;
            }
            //dont use this, lol
            public override void SetCursor(Gwen.Cursor c)
            {
                // Bitmap.FromHicon(cursor.Handle);
                System.Windows.Forms.Cursor cursor;
                switch (c.Name)
                {
                    default:
                    case "Default":
                        game.Cursor = MouseCursor.Default;
                        return;
                    case "SizeWE":
                        cursor = System.Windows.Forms.Cursors.SizeWE;
                        break;
                    case "SizeNWSE":
                        cursor = System.Windows.Forms.Cursors.SizeNWSE;
                        break;
                    case "SizeNS":
                        cursor = System.Windows.Forms.Cursors.SizeNS;
                        break;
                    case "SizeNESW":
                        cursor = System.Windows.Forms.Cursors.SizeNESW;
                        break;
                    case "SizeAll":
                        cursor = System.Windows.Forms.Cursors.SizeAll;
                        break;
                    case "IBeam":
                        cursor = System.Windows.Forms.Cursors.IBeam;
                        break;
                    case "Help":
                        cursor = System.Windows.Forms.Cursors.Help;
                        break;
                    case "Hand":
                        cursor = System.Windows.Forms.Cursors.Hand;
                        break;
                    case "No":
                        cursor = System.Windows.Forms.Cursors.No;
                        break;
                }
                var t = cursor.GetType();
                var whatever = cursor.GetType().GetMethod("ToBitmap", BindingFlags.Instance | BindingFlags.NonPublic);
                var bmp = (Bitmap)whatever.Invoke(cursor, new object[] { false, true });
                // var bmp = (Bitmap)whatever.GetValue(cursor);
                // using (var bmp = new Bitmap(cursor.Size.Width, cursor.Size.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        cursor.Draw(g, new Rectangle(Point.Empty, cursor.Size));

                        BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        SetGameCursor(new MouseCursor(cursor.HotSpot.X, cursor.HotSpot.Y, bmp.Width, bmp.Height, data.Scan0));
                        bmp.UnlockBits(data);
                    }
                }
            }
        }
        Gwen.Input.OpenTK input;
        Canvas Canvas;
        bool _slow = false;
        bool _steps = false;
        public Window() : base(600, 600, GraphicsMode.Default, "UI Test")
        {
            Gwen.Platform.Neutral.Implementation = new PlatformImpl(this);
        }

        Texture skinpng;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            input = new Gwen.Input.OpenTK(this);
            var renderer = new Gwen.Renderer.OpenTK();
            var skinimg = new Bitmap(Image.FromFile("DefaultSkin.png"));
            // var font = new Bitmap(Image.FromFile("liberation_20_0.png"));
            // var fontdata = System.IO.File.ReadAllText("liberation_20.fnt");
            var font = new Bitmap(Image.FromFile("gamefont_15_0.png"));
            var fontdata = System.IO.File.ReadAllText("gamefont_15.fnt");
            var colorxml = System.IO.File.ReadAllText("DefaultColors.xml");
            skinpng = renderer.CreateTexture(skinimg);

            var fontpng = renderer.CreateTexture(font);

            var gamefont_15 = new Gwen.Renderer.BitmapFont(
                renderer,
                fontdata,
                fontpng);

            var skin = new Gwen.Skin.TexturedBase(renderer, skinpng, colorxml) { DefaultFont = gamefont_15 };
            // var skin = new Gwen.Skin.Simple(renderer) { DefaultFont = gamefont_15 };
            Canvas = new Canvas(skin);
            // Canvas.Scale = 2;
            Canvas.SetCanvasSize(ClientSize.Width, ClientSize.Height);
            input.Initialize(Canvas);
            TestContainer container = new TestContainer(Canvas);
            font.Dispose();
            skinimg.Dispose();
            ControlBase.LogLayout = true;
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Canvas.Skin.DefaultFont.Dispose();
            Canvas.Skin.Dispose();
            Canvas.Dispose();
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Canvas.SetCanvasSize(ClientSize.Width, ClientSize.Height);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (_slow && !_steps)
                return;
            _steps = false;
            GL.ClearColor(255, 255, 255, 255);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Viewport(new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, ClientSize.Width, ClientSize.Height, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            Canvas.RenderCanvas();
            Canvas.ShouldDrawBackground = true;
            SwapBuffers();
            base.OnRenderFrame(e);
        }
        protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            try
            {
                input.ProcessKeyDown(e);
            }
            catch (Exception ex)
            {
                //sdl eats exceptions
                Console.WriteLine(ex.ToString());
                throw ex;
            }
            if (e.Key == OpenTK.Input.Key.F5)
            {
                skinpng.Dispose();
                skinpng = new Texture(Canvas.Skin.Renderer);
                var skinimg = new Bitmap(Image.FromFile("DefaultSkin.png"));
                var colorxml = System.IO.File.ReadAllText("DefaultColors.xml");
                Gwen.Renderer.OpenTK.LoadTextureInternal(
                    skinpng,
                    skinimg);
                var skin = new Gwen.Skin.TexturedBase(Canvas.Skin.Renderer, skinpng, colorxml) { DefaultFont = Canvas.Skin.DefaultFont };
                // Canvas.Skin = skin;
                Canvas.SetSkin(skin, true);
                skinimg.Dispose();
            }
            if (e.Key == OpenTK.Input.Key.F12)
            {
                _slow = !_slow;
            }
            if (e.Key == OpenTK.Input.Key.F11)
            {
                _steps = true;
            }
        }
        protected override void OnKeyUp(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            try
            {
                input.ProcessKeyUp(e);
            }
            catch (Exception ex)
            {
                //sdl eats exceptions
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
        protected override void OnMouseMove(OpenTK.Input.MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            try
            {
                input.ProcessMouseMessage(e);
            }
            catch (Exception ex)
            {
                //sdl eats exceptions
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
        protected override void OnMouseUp(OpenTK.Input.MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            try
            {
                input.ProcessMouseMessage(e);
            }
            catch (Exception ex)
            {
                //sdl eats exceptions
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
        protected override void OnMouseDown(OpenTK.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            try
            {
                input.ProcessMouseMessage(e);
            }
            catch (Exception ex)
            {
                //sdl eats exceptions
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
        protected override void OnMouseWheel(OpenTK.Input.MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            try
            {
                input.ProcessMouseMessage(e);
            }
            catch (Exception ex)
            {
                //sdl eats exceptions
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }
    }
}
