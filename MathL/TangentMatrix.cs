using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.MathL
{
    public class TangentMatrix
    {
        double a, b, c, px;
        double d, e, f, py;
        double g, h, k, pz;
        double scale;

        public void SetOrigin (Vector64 newOrigin)
        {
            px = newOrigin.x;
            py = newOrigin.y;
            pz = newOrigin.z;
        }

        public void SetScale (double newScale)
        {
            scale = newScale;
        }

        public static TangentMatrix FromLatLon(double lat, double lon, bool radians = true)
        {
            LatLonCoord latLon;
            if(radians)
            {
                latLon = new LatLonCoord { Lat_Rad = lat, Lon_Rad = lon, Alt = 0 };
            }
            else
            {
                latLon = new LatLonCoord { Lat_Degrees = lat, Lon_Degrees = lon, Alt = 0 };
            }

            return FromLatLon(latLon);
        }

        public static TangentMatrix FromLatLon(LatLonCoord latLon)
        {
            Vector64 vectorForm = Conversions.LLToXYZ(latLon);

            return FromXYZ(vectorForm);
        }

        public static TangentMatrix FromXYZ(Vector64 position)
        {
            if (position.magnitude == 0)
            {
                throw new System.Exception("Unable to compute a Matrix with a zero-length vector!");
            }

            Vector64 UnitSurface = position.normalized;
            Vector64 normal = UnitSurface;
            Vector64 north;

            if (UnitSurface.x + UnitSurface.z == 0)
            {
                north = new Vector64(-1,0,0) * Math.Sign(UnitSurface.y);
            }
            else
            {
                north = -GetNorthVector(UnitSurface);
            }

            Vector64 east = Vector64.Cross(normal, north);

            return new TangentMatrix
            {
                a = normal.x,
                b = normal.y,
                c = normal.z,
                px = UnitSurface.x,
                d = east.x,
                e = east.y,
                f = east.z,
                py = UnitSurface.y,
                g = north.x,
                h = north.y,
                k = north.z,
                pz = UnitSurface.z,
                scale = 1,
            };
        }

        public static Vector64 GetNorthVector (Vector64 surfacePoint)
        {
            if(surfacePoint.z == 0)
            {
                return (0, 0, 1);
            }
            else
            {
                Vector64 cosec = GetNorthCosecant(surfacePoint);

                Vector64 localDir = (cosec - surfacePoint) * Math.Sign(surfacePoint.z);

                return localDir.normalized;
            }
        }

        private static Vector64 GetNorthCosecant (Vector64 surfacePoint)
        {
            Vector64 cosecant = new Vector64(0, 0, 1) / surfacePoint.normalized.z;

            return cosecant;
        }

        public void PointsToTangentSpace (Vector64[] vectors)
        {
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i] = PointToTangentSpace(vectors[i]);
            }
        }

        /// <summary>
        /// Gets a Point in this TangentMatrix's space from World space.
        /// Used for Points where relative position matters.
        /// </summary>
        /// <param name="world">Vector in World space</param>
        /// <returns></returns>
        public Vector64 PointToTangentSpace(Vector64 world)
        {
            
            return VectorToTangentSpace(world - Origin);
        }

        public Vector64 VectorToTangentSpace(Vector64 world)
        {
            double lx = Vector64.Dot(world, Out);
            double ly = Vector64.Dot(world, Up);
            double lz = Vector64.Dot(world, Right);

            return new Vector64(lx, ly, lz) / scale;
        }

        /// <summary>
        /// Returns a Point from this TangentMatrix's space to World space.
        /// Used for Points where relative position matters.
        /// </summary>
        /// <param name="local">Vector in Tangent space</param>
        /// <returns></returns>
        public Vector64 PointToWorldSpace(Vector64 local)
        {
            return VectorToWorldSpace(local) + Origin;
        }

        public Vector64 VectorToWorldSpace(Vector64 local)
        {
            Vector64 worldVec = local.x * Out + local.y * Up + local.z * Right;

            return worldVec * scale;
        }

        public Vector64 Origin
        {
            get
            {
                return new Vector64(px, py, pz);
            }
        }

        public Vector64 Out
        {
            get
            {
                return new Vector64(a, b, c);
            }
        }

        public Vector64 Up
        {
            get
            {
                return new Vector64(d, e, f);
            }
        }

        public Vector64 Right
        {
            get
            {
                return new Vector64(g, h, k);
            }
        }

        public override string ToString()
        {
            return
            $"[[{a} {b} {c} {px}] => {Out.magnitude}\n" +
            $" [{d} {e} {f} {py}] => {Up.magnitude}\n" +
            $" [{g} {h} {k} {pz}] => {Right.magnitude}\n" +
            $" [0 0 0 {scale}]]";
        }
    }
}
