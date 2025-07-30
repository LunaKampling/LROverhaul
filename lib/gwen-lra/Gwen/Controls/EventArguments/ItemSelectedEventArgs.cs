using System;

namespace Gwen.Controls
{
    public class ItemSelectedEventArgs(ControlBase selecteditem) : EventArgs
    {
        public ControlBase SelectedItem { get; private set; } = selecteditem;
    }
}
