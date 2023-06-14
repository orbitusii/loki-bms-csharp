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
        public LatLonCoord LatLon => MathL.Conversions.XYZToLL(Position);
        public Vector64 Velocity { get; set; }

        public IFFData[] IFFCodes { get; set; }

        public DateTime Timestamp { get; set; }

        public TrackCategory Category = TrackCategory.None;

        public double Heading
        {
            get => Heading_Rads * MathL.Conversions.ToDegrees;
            set => Heading_Rads = value * MathL.Conversions.ToRadians;
        }
        public double Heading_Rads { get; set; }

        public double Altitude { get; set; }

        public string[] ExtraData { get; set; } = new string[0];
    }
}
