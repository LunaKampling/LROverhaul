using System;
using System.Drawing;
using Gwen.ControlInternal;
namespace Gwen.Controls
{
    public class DropDownButton : Button
    {
        /// <summary>
        /// Invoked when the dropdown arrow is pressed
        /// </summary>
        public event GwenEventHandler<EventArgs> DropDownClicked;
        private Button _arrow;
        public DropDownButton(ControlBase parent) : base(parent)
        {
            _arrow = new DropDownArrow(this);
            Padding = new Padding(0, 0, _arrow.Width, 0);
            Invalidate();
            _arrow.Clicked += (o, e) =>
            {
                if (DropDownClicked != null)
                    DropDownClicked.Invoke(this, EventArgs.Empty);
            };
        }
        protected override void ProcessLayout(Size size)
        {
            base.ProcessLayout(size);
            if (_arrow != null)
            {
                _arrow.SetBounds(Width - _arrow.Width, 0, _arrow.Width, Height);
            }
        }
    }
}