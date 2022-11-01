using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.Database
{
    public struct IFFData
    {
        public IFFType Type;
        public short Code;

        public enum IFFType
        {
            Mode1 = 0,
            Mode2 = 1,
            Mode3 = 2,
            Mode4 = 3,
            TADIL = 4,
        }
    }
}
