using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace loki_bms_csharp.Geometry
{
    public class Path3D
    {
        public string Name = "New Path";
        public Vector64[] Points;
        public bool ConformToSurface = false;
        public bool Closed = true;

        public SKPath GetScreenSpacePath (TangentMatrix cameraMatrix)
        {
            List<SKPoint> sKPoints = new List<SKPoint>(0);
            int numBehind = 0;

            for (int i = 0; i < Points.Length; i++)
            {
                Vector64 screenSpace = cameraMatrix.PointToTangentSpace(Points[i]);

                if(Math.Abs(screenSpace.x) <= Conversions.EarthRadius)
                {
                    if(ConformToSurface && i > 0)
                    {
                        var slerped = BetweenPoints(Points[i - 1], Points[i], 5);
                        cameraMatrix.PointsToTangentSpace(slerped);

                        sKPoints.AddRange(ToSKPoints(slerped));
                    }
                    else
                    {
                        sKPoints.Add(ToSKPoint(screenSpace));
                    }
                }
                else
                {
                    numBehind++;

                    Vector64 atEdge = (0, screenSpace.y, screenSpace.z);
                    atEdge = atEdge.normalized;
                    float actualY = (float)atEdge.y;
                    float actualZ = (float)atEdge.z;

                    sKPoints.Add(new SKPoint(actualY, actualZ));
                }
            }

            SKPath path = new SKPath();
            path.FillType = SKPathFillType.Winding;

            if (numBehind < Points.Length)
            {
                path.AddPoly(sKPoints.ToArray(), Closed);
            }

            return path;
        }

        public static SKPoint[] ToSKPoints(Vector64[] points)
        {
            List<SKPoint> converted = new List<SKPoint>();

            foreach(var vec in points)
            {
                converted.Add(ToSKPoint(vec));
            }

            return converted.ToArray();
        }

        public static SKPoint ToSKPoint (Vector64 point)
        {
            return new SKPoint((float)point.y, (float)point.z);
        }

        public Vector64[] BetweenPoints (Vector64 from, Vector64 to, int steps)
        {
            Vector64[] points = new Vector64[steps + 1];
            points[0] = from;
            points[steps] = to;

            for (int i = 1; i < steps; i++)
            {
                double t = (double)i / steps;

                points[i] = Vector64.Slerp(from, to, t);
            }

            return points;
        }
    }
}
