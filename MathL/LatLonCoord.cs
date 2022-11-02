using System;
using System.Collections.Generic;
using System.Text;
using loki_bms_csharp.MathL;

namespace loki_bms_csharp
{

    public struct LatLonCoord
    {
        /// <summary>
        /// Latitude, in Degrees Decimal
        /// </summary>
        public double Lat_Degrees
        {
            get { return Lat_Rad * Conversions.ToDegrees; }
            set { Lat_Rad = value * Conversions.ToRadians; }
        }
        public double Lat_Rad;
        /// <summary>
        /// Longitude, in Degrees Decimal
        /// </summary>
        public double Lon_Degrees
        {
            get { return Lon_Rad * Conversions.ToDegrees; }
            set { Lon_Rad = value * Conversions.ToRadians; }
        }
        public double Lon_Rad;
        /// <summary>
        /// Altitude relative to the surface of the reference sphere
        /// </summary>
        public double Alt;
    }
}
