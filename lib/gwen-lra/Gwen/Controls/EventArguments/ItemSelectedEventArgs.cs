using System;

namespace Gwen.Controls
{
    public class ItemSelectedEventArgs : EventArgs
    {
        public ControlBase SelectedItem { get; private set; }

        public ItemSelectedEventArgs(ControlBase selecteditem)
        {
            SelectedItem = selecteditem;
        }
    }
}
