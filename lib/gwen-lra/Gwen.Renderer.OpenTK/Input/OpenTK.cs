using Gwen.Controls;
using OpenTK;
using OpenTK.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using System.Linq;

namespace Gwen.Input
{
    public class OpenTK
    {
        #region Properties
        public bool MouseCaptured => InputHandler.MouseCaptured;
        private bool m_AltGr = false;
        private Canvas m_Canvas = null;

        private int m_MouseX = 0;
        private int m_MouseY = 0;

        #endregion Properties

        #region Constructors

        public OpenTK(NativeWindow window)
        {
            window.TextInput += TextInput;
        }

        #endregion Constructors

        #region Methods

        public void Initialize(Canvas c) => m_Canvas = c;

        public void TextInput(TextInputEventArgs args) => m_Canvas.Input_Character(args.AsString.First());

        public bool ProcessKeyDown(KeyboardKeyEventArgs args)
        {
            KeyboardKeyEventArgs ev = args; //as KeyboardKeyEventArgs;
            char ch = TranslateChar(ev.Key);

            if (InputHandler.DoSpecialKeys(m_Canvas, ch))
                return false;

            Key iKey = TranslateKeyCode(ev.Key);

            return m_Canvas.Input_Key(iKey, true);
        }

        public bool ProcessKeyUp(KeyboardKeyEventArgs args)
        {
            KeyboardKeyEventArgs ev = args; //as KeyboardKeyEventArgs;
            _ = TranslateChar(ev.Key);

            Key iKey = TranslateKeyCode(ev.Key);

            return m_Canvas.Input_Key(iKey, false);
        }

        public bool ProcessMouseMessage(object args)
        {
            if (null == m_Canvas)
                return false;

            if (args is MouseMoveEventArgs)
            {
                MouseMoveEventArgs ev = (MouseMoveEventArgs) args;// as MouseMoveEventArgs;
                int dx = (int)ev.X - m_MouseX;
                int dy = (int)ev.Y - m_MouseY;

                m_MouseX = (int)ev.X;
                m_MouseY = (int)ev.Y;

                return m_Canvas.Input_MouseMoved(m_MouseX, m_MouseY, dx, dy);
            }

            if (args is MouseButtonEventArgs)
            {
                MouseButtonEventArgs ev = (MouseButtonEventArgs)args;// as MouseButtonEventArgs;
                //m_MouseX = ev.X;
                //m_MouseY = ev.Y;

                /* We can not simply cast ev.Button to an int, as 1 is middle click, not right click. */
                int ButtonID = -1; // Do not trigger event.

                if (ev.Button == MouseButton.Left)
                    ButtonID = 0;
                else if (ev.Button == MouseButton.Right)
                    ButtonID = 1;

                if (ButtonID != -1) // We only care about left and right click for now
                    return m_Canvas.Input_MouseButton(ButtonID, ev.IsPressed, m_MouseX, m_MouseY);
            }

            if (args is MouseWheelEventArgs)
            {
                MouseWheelEventArgs ev = (MouseWheelEventArgs)args;// as MouseWheelEventArgs;
                return m_Canvas.Input_MouseWheel((int)ev.OffsetY * 60);
            }

            return false;
        }

        /// <summary>
        /// Translates alphanumeric OpenTK key code to character value.
        /// </summary>
        /// <param name="key">OpenTK key code.</param>
        /// <returns>Translated character.</returns>
        private static char TranslateChar(global::OpenTK.Windowing.GraphicsLibraryFramework.Keys key) => key >= global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.A && key <= global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Z
                ? (char)('a' + ((int)key - (int)global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
                : ' ';

        /// <summary>
        /// Translates control key's OpenTK key code to GWEN's code.
        /// </summary>
        /// <param name="key">OpenTK key code.</param>
        /// <returns>GWEN key code.</returns>
        private Key TranslateKeyCode(global::OpenTK.Windowing.GraphicsLibraryFramework.Keys key)
        {
            switch (key)
            {
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backspace: return Key.Backspace;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter: return Key.Return;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape: return Key.Escape;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Tab: return Key.Tab;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space: return Key.Space;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up: return Key.Up;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down: return Key.Down;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left: return Key.Left;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right: return Key.Right;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Home: return Key.Home;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.End: return Key.End;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.Delete: return Key.Delete;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftControl:
                    m_AltGr = true;
                    return Key.Control;

                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt: return Key.Alt;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift: return Key.Shift;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightControl: return Key.Control;
                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightAlt:
                    if (m_AltGr)
                    {
                        _ = m_Canvas.Input_Key(Key.Control, false);
                    }
                    return Key.Alt;

                case global::OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightShift: return Key.Shift;
            }
            return Key.Invalid;
        }

        #endregion Methods
    }
}