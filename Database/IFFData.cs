using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.Database
{
    public struct IFFData
    {
        public IFFType Type;
        public short Code;

        [Flags]
        public enum IFFType
        {
            None = 0,
            Mode1 = 1,
            Mode2 = 2,
            Mode3 = 4,
            Mode4 = 8,
            TADIL = 16,
        }
    }
}
