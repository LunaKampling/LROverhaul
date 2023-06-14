using OpenTK.Input;
using System.Globalization;
namespace linerider.UI
{
    public class Keybinding
    {
        private Key _key = (Key)(-1);
        public MouseButton MouseButton = (MouseButton)(-1);
        public Key Key
        {
            get => _key;
            set
            {
                switch (value)
                {
                    case Key.AltLeft:
                    case Key.AltRight:
                        _key = (Key)(-1);
                        Modifiers |= KeyModifiers.Alt;
                        return;
                    case Key.ShiftLeft:
                    case Key.ShiftRight:
                        _key = (Key)(-1);
                        Modifiers |= KeyModifiers.Shift;
                        return;
                    case Key.ControlLeft:
                    case Key.ControlRight:
                        _key = (Key)(-1);
                        Modifiers |= KeyModifiers.Control;
                        return;
                    default:
                        _key = value;
                        break;
                }
            }
        }
        public KeyModifiers Modifiers = 0;
        public bool IsEmpty => Modifiers == 0 && Key == (Key)(-1) && MouseButton == (MouseButton)(-1);
        public int KeysDown
        {
            get
            {
                int ret = 0;
                if (Modifiers.HasFlag(KeyModifiers.Alt))
                    ret++;
                if (Modifiers.HasFlag(KeyModifiers.Shift))
                    ret++;
                if (Modifiers.HasFlag(KeyModifiers.Control))
                    ret++;
                if (UsesKeys)
                    ret++;
                return ret;
            }
        }
        public bool UsesModifiers => Modifiers != 0;
        public bool UsesKeys => Key != (Key)(-1);
        public bool UsesMouse => MouseButton != (MouseButton)(-1);
        public Keybinding()
        {
        }
        public Keybinding(Key key, KeyModifiers modifiers)
        {
            Modifiers = modifiers;
            Key = key;
        }
        public Keybinding(Key key)
        {
            Key = key;
        }
        public Keybinding(MouseButton mouse)
        {
            MouseButton = mouse;
        }
        public Keybinding(KeyModifiers modifiers)
        {
            Modifiers = modifiers;
        }
        public Keybinding(MouseButton mouse, KeyModifiers modifiers)
        {
            MouseButton = mouse;
            Modifiers = modifiers;
        }
        public bool IsBindingEqual(Keybinding other) => other != null && other.Key == Key && other.Modifiers == Modifiers && other.MouseButton == MouseButton;
        public override string ToString()
        {
            if (IsEmpty)
                return "<unset>";
            string kb = "";
            int modifiers = 0;
            if (UsesModifiers)
            {
                if (Modifiers.HasFlag(KeyModifiers.Control))
                {
                    kb += "Ctrl";
                    modifiers++;
                }
                if (Modifiers.HasFlag(KeyModifiers.Shift))
                {
                    if (modifiers > 0)
                    {
                        kb += "+";
                    }
                    kb += "Shift";
                    modifiers++;
                }
                if (Modifiers.HasFlag(KeyModifiers.Alt))
                {
                    if (modifiers > 0)
                    {
                        kb += "+";
                    }
                    kb += "Alt";
                    modifiers++;
                }
            }
            if (UsesKeys)
            {
                if (modifiers > 0)
                    kb += "+";
                kb += CultureInfo.CurrentCulture.TextInfo.ToTitleCase(KeyToString(Key));
            }
            if (UsesMouse)
            {

                if (modifiers > 0)
                    kb += "+";
                kb += MouseToString(MouseButton);
            }
            return kb;
        }
        private string MouseToString(MouseButton button)
        {
            switch (MouseButton)
            {
                case MouseButton.Left:
                    return "<Left MButton>";
                case MouseButton.Right:
                    return "<Right MButton>";
                case MouseButton.Middle:
                    return "<Middle MButton>";
            }
            if (button >= MouseButton.Button1 && button <= MouseButton.Button9)
            {
                int num = 4 + ((int)button - (int)MouseButton.Button1);
                return $"<Mouse Button #{num}>";
            }
            return button.ToString();
        }
        private string KeyToString(Key key)
        {

            switch (key)
            {
                case Key.Enter:
                case Key.Escape:
                case Key.Tab:
                case Key.Space:
                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                case Key.Home:
                case Key.End:
                case Key.Delete:
                case Key.PageDown:
                case Key.PageUp:
                case Key.Insert:
                    return key.ToString();
                case Key.BackSpace:
                    return "Backspace";
                case Key.RControl:
                case Key.LControl:
                    return "Control";
                case Key.RAlt:
                case Key.LAlt:
                    return "Alt";
                case Key.RShift:
                case Key.LShift:
                    return "Shift";
                case Key.BracketLeft:
                    return "[";
                case Key.BracketRight:
                    return "]";
                case Key.Semicolon:
                    return ";";
                case Key.Quote:
                    return "\"";
                case Key.Period:
                    return ".";
                case Key.Comma:
                    return ",";
                case Key.Grave:
                    return "`";
                case Key.Minus:
                    return "-";
                case Key.Plus:
                    return "+";
                case Key.Slash:
                    return "/";
                case Key.BackSlash:
                    return "\\";
                default:
                    char trans = TranslateChar(key);
                    if (trans == ' ')
                        return key.ToString();
                    return trans.ToString();
            }
        }
        private static char TranslateChar(Key key) => key >= Key.A && key <= Key.Z
                ? (char)('A' + ((int)key - (int)Key.A))
                : key >= Key.Number0 && key <= Key.Number9
                ? (char)('0' + ((int)key - (int)Key.Number0))
                : key >= Key.Keypad0 && key <= Key.Keypad9 ? (char)('0' + ((int)key - (int)Key.Keypad0)) : ' ';
    }
}