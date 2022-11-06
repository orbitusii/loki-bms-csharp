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

        public SKPath GetScreenSpacePath (MathL.TangentMatrix cameraMatrix)
        {
            SKPoint[] sKPoints = new SKPoint[Points.Length];
            int numBehind = 0;

            for (int i = 0; i < Points.Length; i++)
            {
                Vector64 screenSpace = cameraMatrix.PointToTangentSpace(Points[i]);

                if(Math.Abs(screenSpace.x) <= MathL.Conversions.EarthRadius)
                {
                    sKPoints[i] = new SKPoint((float)screenSpace.y, (float)screenSpace.z);
                }
                else
                {
                    numBehind++;

                    Vector64 atEdge = (0, screenSpace.y, screenSpace.z);
                    atEdge = atEdge.normalized;
                    float actualY = (float)atEdge.y;
                    float actualZ = (float)atEdge.z;

                    sKPoints[i] = new SKPoint(actualY, actualZ);
                }
            }

            SKPath path = new SKPath();
            path.FillType = SKPathFillType.Winding;

            if (numBehind < Points.Length)
            {
                path.AddPoly(sKPoints);
            }

            return path;
        }
    }
}
