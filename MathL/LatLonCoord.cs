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
        [System.Xml.Serialization.XmlAttribute]
        public double Lat_Degrees
        {
            get { return Lat_Rad * Conversions.ToDegrees; }
            set { Lat_Rad = value * Conversions.ToRadians; }
        }
        [System.Xml.Serialization.XmlIgnore]
        public double Lat_Rad;
        /// <summary>
        /// Longitude, in Degrees Decimal
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public double Lon_Degrees
        {
            get { return Lon_Rad * Conversions.ToDegrees; }
            set { Lon_Rad = value * Conversions.ToRadians; }
        }
        [System.Xml.Serialization.XmlIgnore]
        public double Lon_Rad;
        /// <summary>
        /// Altitude relative to the surface of the reference sphere
        /// </summary>
        [System.Xml.Serialization.XmlAttribute]
        public double Alt;

        public override string ToString()
        {
            return $"({Lat_Degrees}{(Lat_Rad >= 0 ? 'N' : 'S')},{Lon_Degrees}{(Lon_Rad >= 0 ? 'E' : 'W')})";
        }
    }
}
