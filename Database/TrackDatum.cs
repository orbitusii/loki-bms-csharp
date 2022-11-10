using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.Database
{
    public class TrackDatum : IKinematicData
    {
        public TrackNumber ID;
        public DataSource Origin;

        public Vector64 Position { get; set; }
        public Vector64 Velocity { get; set; }

        public IFFData[] IFFCodes { get; set; }

        public DateTime Timestamp { get; set; }

        public TrackCategory Category = TrackCategory.None;

        public string[] ExtraData { get; set; } = new string[0];
    }
}
