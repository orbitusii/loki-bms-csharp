using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.Database
{
    public interface IReturnData
    {
        public Vector64 Position { get; }
        public Vector64 Velocity { get; }

        public IFFData[] IFFCodes { get; }

        public DateTime Timestamp { get; }
    }
}
