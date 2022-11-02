using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.MathL
{
    public struct TangentMatrix
    {
        double a, b, c, px;
        double d, e, f, py;
        double g, h, k, pz;
        double scale;

        public static TangentMatrix FromLatLon (double lat, double lon, bool radians = true)
        {
            if(!radians)
            {
                lat *= Math.PI / 180;
                lon *= Math.PI / 180;
            }

            double z = Math.Sin(lat);
            double cosLat = Math.Cos(lat);

            double x = Math.Cos(lon) * cosLat;
            double y = Math.Sin(lon) * cosLat;

            return OnUnitSphere((x, y, z));
        }

        public static TangentMatrix OnUnitSphere(Vector64 position)
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
                north = GetNorthVector(UnitSurface);
            }

            Vector64 east = Vector64.Cross(normal, north);

            return new TangentMatrix
            {
                a = normal.x,
                b = normal.y,
                c = normal.z,
                px = UnitSurface.x,
                d = north.x,
                e = north.y,
                f = north.z,
                py = UnitSurface.y,
                g = east.x,
                h = east.y,
                k = east.z,
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

        public Vector64 ToLocal(Vector64 world)
        {
            world -= Origin;

            double lx = Vector64.Dot(world, Out);
            double ly = Vector64.Dot(world, Up);
            double lz = Vector64.Dot(world, Right);

            return new Vector64(lx, ly, lz);
        }

        public Vector64 ToWorld(Vector64 local)
        {
            Vector64 worldVec = local.x * Out + local.y * Up + local.z * Right + Origin;

            return worldVec;
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
