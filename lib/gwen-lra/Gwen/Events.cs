using Gwen.Controls;
using System.Drawing;

namespace Gwen
{
    /// <summary>
    /// Delegate used to request a text value for a label
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="text">Current label text.</param>
    public delegate string TextRequestHandler(ControlBase sender, string currenttext);
    /// <summary>
    /// Delegate used to request a new position upon parent or self resizing
    /// </summary>
    public delegate Point PositionerDelegate(ControlBase sender);
}