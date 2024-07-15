using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_common.MathL
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
        public const double MetersToFeet = 3.28084;
        public const double MetersPerSecToKnots = 1.944012;

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
        /// Converts an <see cref="O2Kml.LatLon"/> coordinate into its cartesian (XYZ) counterpart
        /// </summary>
        /// <param name="latLon"><see cref="O2Kml.LatLon"/> to be converted</param>
        /// <param name="radius">The radius of the reference sphere (defaults to 1)</param>
        /// <returns></returns>
        public static Vector64 LLToXYZ (O2Kml.LatLon latLon, double radius = 1)
        {
            return LLToXYZ(new LatLonCoord()
            {
                Lat_Degrees = latLon.Lat,
                Lon_Degrees = latLon.Lon,
                Alt = latLon.Alt,
            }, radius);
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
        /// Gets the velocity of an object, in cartesian space, based on its speed tangent to the surface of Earth
        /// </summary>
        /// <param name="latLon"></param>
        /// <param name="heading">Heading, in clockwise radians from true North</param>
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

            if(double.IsNaN(vx) || double.IsNaN(vy) || double.IsNaN(vz))
            {
                return Vector64.zero;
            }

            return toWorldSpace;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        /// <returns>Speed in m/s, vertical speed in m/s, heading in clockwise radians</returns>
        public static (double speed, double vertSpeed, double heading) GetSurfaceMotion(Vector64 position, Vector64 velocity)
        {
            TangentMatrix mat = TangentMatrix.FromXYZ(position);

            Vector64 vel_surface = mat.VectorToTangentSpace(velocity);
            vel_surface -= (vel_surface.x, 0, 0);

            double heading = Math.Acos(-vel_surface.normalized.z);
            if(vel_surface.y < 0)
            {
                heading *= -1;
                heading += Math.PI * 2;
            }
            double vertSpeed = vel_surface.x;

            Vector64 pureTangent = (0, vel_surface.y, vel_surface.z);
            double speed = pureTangent.magnitude;

            return (speed, vertSpeed, heading);
        }

        public static double GetGreatCircleDistance(Vector64 from, Vector64 to)
        {
            double surfaceAngle = Vector64.AngleBetween(from, to);
            double dist = Conversions.EarthRadius * surfaceAngle;

            return dist;
        }

        public static double GetBearing (Vector64 from, Vector64 to)
        {

            TangentMatrix mat = TangentMatrix.FromXYZ(from);

            Vector64 vel_surface = mat.VectorToTangentSpace(to);
            vel_surface -= (vel_surface.x, 0, 0);

            double heading = Math.Acos(-vel_surface.normalized.z);
            if (vel_surface.y < 0)
            {
                heading *= -1;
                heading += Math.PI * 2;
            }

            return heading;
        }
    }
}
