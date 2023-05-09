using System;

namespace Gwen
{
    /// <summary>
    /// Represents dock position.
    /// </summary>
    [Flags]
    public enum Dock
    {
        None = 0,
        Left = 1 << 1,
        Right = 1 << 2,
        Top = 1 << 3,
        Bottom = 1 << 4,
        // CenterV = 1 << 5,
        // CenterH = 1 << 6,
        ///<summary>
        ///Dock in the remaining space after all fill operations.
        ///</summary>
        ///<remarks>Doesn't play well with AutoSizeToContents</remarks>
        Fill = 1 << 7,
        // Center = CenterV | CenterH,
    }
}
