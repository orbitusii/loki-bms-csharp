using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.MathL
{
    public struct LatLonCoord
    {
        /// <summary>
        /// Latitude, in Degrees Decimal
        /// </summary>
        public double Lat_Degrees
        {
            get { return Lat_Rad * MathL.ToDegrees; }
            set { Lat_Rad = value * MathL.ToRadians; }
        }
        public double Lat_Rad;
        /// <summary>
        /// Longitude, in Degrees Decimal
        /// </summary>
        public double Lon_Degrees
        {
            get { return Lon_Rad * MathL.ToDegrees; }
            set { Lon_Rad = value * MathL.ToRadians; }
        }
        public double Lon_Rad;
        /// <summary>
        /// Altitude relative to the surface of the reference sphere
        /// </summary>
        public double Alt;
    }
}
