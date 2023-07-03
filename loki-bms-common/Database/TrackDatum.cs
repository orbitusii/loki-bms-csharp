using System;
using System.Collections.Generic;
using System.Text;
using loki_bms_common;
using loki_bms_common.MathL;

namespace loki_bms_common.Database
{
    public class TrackDatum : IPositionedObject
    {
        public TrackNumber ID;
        public LokiDataSource Origin;
        public Vector64 Position { get; set; }
        public LatLonCoord LatLon => Conversions.XYZToLL(Position);
        public Vector64 Velocity { get; set; }

        public IFFData[]? IFFCodes { get; set; }

        public DateTime Timestamp { get; set; }

        public TrackCategory Category = TrackCategory.None;

        public double Heading_Rads { get; set; }
        public double Heading_Deg
        {
            get => Heading_Rads * Conversions.ToDegrees;
            set
            {
                Heading_Rads = value * Conversions.ToRadians;
            }
        }

        public double Altitude { get; set; }

        public string[] ExtraData { get; set; } = new string[0];

        public TrackDatum(TrackNumber ID, LokiDataSource Origin, Vector64 Position, Vector64 Velocity)
        {
            this.ID = ID;
            this.Origin = Origin;
            this.Position = Position;
            this.Velocity = Velocity;
            Timestamp = DateTime.Now;
        }
    }
}
