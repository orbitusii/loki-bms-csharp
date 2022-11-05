using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.MathL
{
    public static class Conversions
    {
        public const double ToRadians = Math.PI / 180;
        public const double ToDegrees = 1 / ToRadians;
        public const double EarthRadius = 6378137.0; // Meters
        public static double EarthCircumference
        {
            get
            {
                return 2 * Math.PI * EarthRadius;
            }
        }

        public const double MetersToNM = 5.399568e-4;

        /// <summary>
        /// Converts a Vector64 coordinate in world space to a LatLonCoord representing its position on the surface of a sphere
        /// </summary>
        /// <param name="cartesian">Vector64 to be converted</param>
        /// <param name="radius">The radius of the reference sphere (defaults to 1)</param>
        /// <returns></returns>
        public static LatLonCoord XYZToLL (Vector64 cartesian, double radius = 1)
        {
            double altitude = cartesian.magnitude - radius;
            Vector64 normalized = cartesian.normalized;

            double lat_rad = Math.Asin(normalized.z);
            double lon_rad = Math.Acos(normalized.x / Math.Cos(lat_rad)) * Math.Sign(normalized.y);

            return new LatLonCoord { Lat_Rad = lat_rad, Lon_Rad = lon_rad, Alt = altitude };
        }

        /// <summary>
        /// Converts a LatLonCoord representing an object on the surface of a sphere to a Vector64
        /// </summary>
        /// <param name="latLon">LatLonCoord to be converted</param>
        /// <param name="radius">The radius of the reference sphere (defaults to 1)</param>
        /// <returns></returns>
        public static Vector64 LLToXYZ (LatLonCoord latLon, double radius = 1)
        {
            double magnitude = latLon.Alt + radius;
            double cosLat = Math.Cos(latLon.Lat_Rad);

            double x = cosLat * Math.Cos(latLon.Lon_Rad);
            double y = cosLat * Math.Sin(latLon.Lon_Rad);
            double z = Math.Sin(latLon.Lat_Rad);

            return new Vector64(x, y, z) * magnitude;
        }

        /// <summary>
        /// Gets the velocity of an object tangent 
        /// </summary>
        /// <param name="latLon"></param>
        /// <param name="heading"></param>
        /// <param name="speed"></param>
        /// <param name="verticalSpeed"></param>
        /// <returns></returns>
        public static Vector64 GetTangentVelocity (LatLonCoord latLon, double heading = 0, double speed = 0, double verticalSpeed = 0)
        {
            TangentMatrix mat = TangentMatrix.FromLatLon(latLon);

            double vx = verticalSpeed;
            double vy = speed * Math.Sin(heading);
            double vz = speed * -Math.Cos(heading);

            Vector64 toWorldSpace = mat.VectorToWorldSpace((vx, vy, vz));

            return toWorldSpace;
        }

        public static (double speed, double vertSpeed, double heading) GetSurfaceMotion(Vector64 cartesian, Vector64 velocity)
        {
            TangentMatrix mat = TangentMatrix.FromXYZ(cartesian);

            Vector64 toLocalSpace = mat.VectorToTangentSpace(velocity);

            double heading = Math.Asin(toLocalSpace.normalized.y);
            double vertSpeed = toLocalSpace.x;

            Vector64 pureTangent = (0, toLocalSpace.y, toLocalSpace.z);
            double speed = pureTangent.magnitude;

            return (speed, vertSpeed, heading);
        }
    }
}
