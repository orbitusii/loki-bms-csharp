using loki_bms_common.MathL;
using loki_bms_common.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_common
{
    public class RawTrackDatum: IPositionedObject
    {
        public int RawID;
        public LokiDataSource Origin;

        public RawTrackDatum (int ID, LokiDataSource Origin)
        {
            this.RawID = ID;
            this.Origin = Origin;
        }

        public Vector64 Position { get; set; }
        public LatLonCoord LatLon => Conversions.XYZToLL(Position);

        public double Heading
        {
            get => Heading_Rads * Conversions.ToDegrees;
            set => Heading_Rads = value * Conversions.ToRadians;
        }
        public double Heading_Rads { get; set; }

        public Vector64 Velocity { get; set; }

        public DateTime Timestamp { get; }
    }
}
