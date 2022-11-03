using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.Database
{
    public class TrackDatum : IReturnData
    {
        public Vector64 Position { get; set; }
        public Vector64 Velocity { get; set; }

        public IFFData[] IFFCodes { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
