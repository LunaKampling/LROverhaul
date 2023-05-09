using System;

namespace Gwen
{
    /// <summary>
    /// Represents relative position.
    /// </summary>
    [Flags]
    public enum Pos
    {
        Left = 1 << 1,
        Right = 1 << 2,
        Top = 1 << 3,
        Bottom = 1 << 4,
        CenterV = 1 << 5,
        CenterH = 1 << 6,
        Center = CenterV | CenterH,
    }
}
