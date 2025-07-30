using System;

namespace Gwen.Controls
{
    public class ClickedEventArgs(int x, int y, bool down) : EventArgs
    {
        public int X { get; private set; } = x;
        public int Y { get; private set; } = y;
        public bool MouseDown { get; private set; } = down;
    }
}
