using System;

namespace Gwen.Controls
{
    public class ClickedEventArgs : EventArgs
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool MouseDown { get; private set; }

        public ClickedEventArgs(int x, int y, bool down)
        {
            X = x;
            Y = y;
            MouseDown = down;
        }
    }
}
