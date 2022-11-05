using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.UserInterface
{
    [Flags]
    public enum MouseClickState
    {
        None = 0,
        Left = 1,
        Middle = 2,
        Right = 4,
        Fourth = 8,
        Fifth = 16,
    }
}
