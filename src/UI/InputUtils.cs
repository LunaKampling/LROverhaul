using linerider.Utils;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using Key = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace linerider.UI
{
    internal static class InputUtils
    {
        /// how hotkeys ive observed in applications work
        /// 
        /// if a modifier is pressed and a key is pressed and the modifier does 
        /// not hit a special case, the normal case not play
        /// 
        /// if a key is pressed then a modifier is pressed, regardless of if 
        /// the previous key hit, it switches over to the modifier version
        /// 
        /// if that modifier is released, the key will resume without modifiers
        /// 
        /// if another non mod key is pressed, it's like the previous 
        /// non-modifier key was never pressed.

        private class HotkeyHandler
        {
            public Hotkey hotkey = Hotkey.None;
            public bool repeat = false;
            public Func<bool> condition = null;
            public Action keydownhandler = null;
            public Action keyuphandler = null;
        }
        private static GameWindow _window;
        private static KeyboardState _kbstate;
        private static KeyboardState _prev_kbstate;
        private static readonly List<MouseButton> _mousebuttonsdown = new List<MouseButton>();
        private static MouseState _mousestate;
        private static MouseState _prev_mousestate;
        private static bool _hasmoved = false;
        private static readonly ResourceSync _lock = new ResourceSync();
        private static KeyModifiers _modifiersdown;
        private static readonly Dictionary<Hotkey, HotkeyHandler> Handlers = new Dictionary<Hotkey, HotkeyHandler>();
        private static HotkeyHandler _current_hotkey = null;
        // Macos has to handle ctrl+ combos differently.
        private static bool _macOS = false;
        /// "If a non modifier key was pressed, it's like the previous key 
        /// stopped being pressed"
        /// We implement that using RepeatKey which we update on non-repeat
        /// keystokes
        private static Key RepeatKey = Key.Unknown;
        public static List<KeyModifiers> SplitModifiers(KeyModifiers modifiers)
        {
            List<KeyModifiers> ret = new List<KeyModifiers>();
            if (modifiers.HasFlag(KeyModifiers.Control))
            {
                ret.Add(KeyModifiers.Control);
            }
            if (modifiers.HasFlag(KeyModifiers.Shift))
            {
                ret.Add(KeyModifiers.Shift);
            }
            if (modifiers.HasFlag(KeyModifiers.Alt))
            {
                ret.Add(KeyModifiers.Alt);
            }
            return ret;
        }
        public static Keybinding ReadHotkey()
        {
            Key key = RepeatKey;
            if (!_kbstate[key])
                key = (Key)(-1);
            if (_mousebuttonsdown.Count == 1 && key == (Key)(-1))
            {
                MouseButton button = _mousebuttonsdown[0];
                if (button == MouseButton.Left || button == MouseButton.Right)
                    return new Keybinding(); // Ignore
                return new Keybinding(button, _modifiersdown);
            }
            return new Keybinding(key, _modifiersdown);
        }
        public static void KeyDown(Key key)
        {
            if (!IsModifier(key))
                RepeatKey = key;
        }
        public static void UpdateKeysDown(KeyboardState ks, KeyModifiers modifiers)
        {
            using (_lock.AcquireWrite())
            {
                _prev_kbstate = _kbstate;
                _kbstate = ks;
                _modifiersdown = modifiers;
            }
        }
        public static void RegisterHotkey(
            Hotkey hotkey,
            Func<bool> condition,
            Action onkeydown,
            Action onkeyup = null,
            bool repeat = false)
        {
            if (Handlers.ContainsKey(hotkey))
                throw new Exception("Hotkey registered twice");

            Handlers.Add(hotkey, new HotkeyHandler()
            {
                hotkey = hotkey,
                condition = condition,
                keydownhandler = onkeydown,
                keyuphandler = onkeyup,
                repeat = repeat
            });
        }
        public static void FreeHotkey(Hotkey hotkey) => Handlers.Remove(hotkey);
        /// <summary>
        /// Checks if the currently pressed hotkey is still 'pressed' after a
        /// state change.
        /// </summary>
        public static bool CheckCurrentHotkey()
        {
            if (_current_hotkey != null)
            {
                if (Check(_current_hotkey.hotkey) &&
                    _current_hotkey.condition())
                {
                    return true;
                }
                _current_hotkey.keyuphandler?.Invoke();
                _current_hotkey = null;
            }
            return false;
        }
        public static void ProcessMouseHotkeys()
        {
            _ = CheckCurrentHotkey();
            foreach (KeyValuePair<Hotkey, HotkeyHandler> pair in Handlers)
            {
                Keybinding bind = CheckInternal(pair.Key, true);
                if (bind != null && bind.UsesMouse)
                {
                    HotkeyHandler handler = pair.Value;
                    if (handler.condition())
                    {
                        bool waspressed = CheckPressed(
                            bind,
                            ref _prev_kbstate,
                            ref _prev_mousestate);
                        if (waspressed)
                        {
                            continue;
                        }
                        _current_hotkey?.keyuphandler?.Invoke();
                        _current_hotkey = handler;
                        handler.keydownhandler();
                        break;
                    }
                }
            }
        }
        public static void ProcessKeyboardHotkeys()
        {
            if (CheckCurrentHotkey())
            {
                Keybinding kb = CheckInternal(_current_hotkey.hotkey, true);
                if (!kb.UsesMouse && _current_hotkey.repeat)
                {
                    _current_hotkey.keydownhandler();
                }
                return;
            }
            _current_hotkey = null;
            foreach (KeyValuePair<Hotkey, HotkeyHandler> pair in Handlers)
            {
                Keybinding bind = CheckInternal(pair.Key, false);
                if (bind != null)
                {
                    HotkeyHandler handler = pair.Value;
                    if (handler.condition())
                    {
                        bool waspressed = CheckPressed(
                            bind,
                            ref _prev_kbstate,
                            ref _prev_mousestate);
                        if (waspressed && !handler.repeat)
                        {
                            continue;
                        }
                        handler.keydownhandler();
                        _current_hotkey = handler;
                        break;
                    }
                }
            }
        }
        public static bool HandleMouseMove(out int x, out int y)
        {
            using (_lock.AcquireWrite())
            {
                x = (int)_mousestate.X;
                y = (int)_mousestate.Y;
                if (_hasmoved)
                {
                    _hasmoved = false;
                    return true;
                }
                return false;
            }
        }
        public static void UpdateMouse(MouseState ms)
        {
            using (_lock.AcquireWrite())
            {
                if (_mousestate.X != ms.X || _mousestate.Y != ms.Y)
                {
                    _hasmoved = true;
                }
                _prev_mousestate = _mousestate;
                _mousestate = ms;
                _mousebuttonsdown.Clear();
                for (MouseButton btn = 0; btn < MouseButton.Last; btn++)
                {
                    if (_mousestate[btn])
                        _mousebuttonsdown.Add(btn);
                }
            }
        }
        public static Vector2d GetMouse() => new Vector2d(_mousestate.X, _mousestate.Y);
        public static bool Check(Hotkey hotkey) => CheckInternal(hotkey, true) != null;
        public static bool CheckPressed(Hotkey hotkey)
        {
            if (Settings.Keybinds.TryGetValue(hotkey, out List<Keybinding> keybindings))
            {
                using (_lock.AcquireRead())
                {
                    foreach (Keybinding bind in keybindings)
                    {
                        if (bind.IsEmpty)
                        {
                            continue;
                        }
                        if (CheckPressed(bind, ref _kbstate, ref _mousestate))
                            return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Returns true if no key has been pressed that changes the definition
        /// of the "currently pressed" keybind.
        /// </summary>
        private static bool IsKeybindExclusive(Keybinding bind)
        {
            if (bind.UsesModifiers && !bind.UsesKeys)
                return true;
            if (bind.UsesKeys)
            {
                if (_modifiersdown != bind.Modifiers ||
                bind.Key != RepeatKey) // Someone overrode us
                    return false;
            }
            if (bind.UsesMouse)
            {
                // We can conflict with left/right, not others
                int buttonsdown = _mousebuttonsdown.Count;
                if (_mousestate[MouseButton.Left])
                    buttonsdown--;
                if (_mousestate[MouseButton.Right])
                    buttonsdown--;
                if (buttonsdown > 1)
                    return false;
            }
            return true;
        }
        private static Keybinding CheckInternal(Hotkey hotkey, bool checkmouse)
        {
            if (Settings.Keybinds.TryGetValue(hotkey, out List<Keybinding> keybindings))
            {
                using (_lock.AcquireRead())
                {
                    foreach (Keybinding bind in keybindings)
                    {
                        if (bind.IsEmpty ||
                            (bind.UsesMouse && !checkmouse) ||
                            !IsKeybindExclusive(bind))
                        {
                            continue;
                        }
                        if (CheckPressed(bind, ref _kbstate, ref _mousestate))
                            return bind;
                    }
                }
            }
            return null;
        }
        private static bool CheckPressed(Keybinding bind, ref KeyboardState state, ref MouseState mousestate)
        {
            if (_window != null && !_window.IsFocused)
                return false;
            if (bind.Key != (Key)(-1))
            {
                if (!state.IsKeyDown(bind.Key))
                {
                    if (_macOS)
                    {
                        // We remap command to control here.
                        // Ctrl+ keys aren't working properly on osx
                        // I don't know of a better way to handle this platform
                        // issue.
                        switch (bind.Key)
                        {
                            case Key.LeftControl:
                                if (!state.IsKeyDown(Key.LeftSuper))
                                    return false;
                                break;
                            case Key.RightControl:
                                if (!state.IsKeyDown(Key.RightSuper))
                                    return false;
                                break;
                            default:
                                return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            if (bind.UsesMouse)
            {
                if (!mousestate.IsButtonDown(bind.MouseButton))
                    return false;
            }
            if (bind.Modifiers != 0)
            {
                bool alt =
                state.IsKeyDown(Key.LeftAlt) ||
                state.IsKeyDown(Key.RightAlt);
                bool ctrl =
                state.IsKeyDown(Key.LeftControl) ||
                state.IsKeyDown(Key.RightControl);
                if (_macOS)
                {
                    // Remap the command key to ctrl.
                    ctrl |=
                    state.IsKeyDown(Key.LeftSuper) ||
                    state.IsKeyDown(Key.RightSuper);
                }
                bool shift =
                state.IsKeyDown(Key.LeftShift) ||
                state.IsKeyDown(Key.RightShift);

                if ((bind.Modifiers.HasFlag(KeyModifiers.Alt) && !alt) ||
                (bind.Modifiers.HasFlag(KeyModifiers.Shift) && !shift) ||
                (bind.Modifiers.HasFlag(KeyModifiers.Control) && !ctrl))
                    return false;
            }
            return !bind.IsEmpty;
        }
        private static bool IsModifier(Key key)
        {
            switch (key)
            {
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftControl:
                case Key.RightControl:
                case Key.LeftSuper:
                case Key.RightSuper:
                    return true;
            }
            return false;
        }
        public static void SetWindow(GameWindow window)
        {
            _window = window ?? throw new NullReferenceException("InputUtils SetWindow cannot be null");
            _macOS = OperatingSystem.IsMacOS();
        }
    }
}