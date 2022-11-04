using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace loki_bms_csharp.UserInterface.Maps
{
    public class Path3D
    {
        public string Name = "New Path";
        public Vector64[] Points;

        public SKPath GetScreenSpacePath (MathL.TangentMatrix cameraMatrix)
        {
            SKPoint[] sKPoints = new SKPoint[Points.Length];

            for (int i = 0; i < Points.Length; i++)
            {
                Vector64 screenSpace = cameraMatrix.PointToTangentSpace(Points[i]);

                if(Math.Abs(screenSpace.x) <= MathL.Conversions.EarthRadius)
                {
                    sKPoints[i] = new SKPoint((float)screenSpace.y, (float)screenSpace.z);
                }
                else
                {
                    float actualY = (float)(screenSpace.normalized.y * MathL.Conversions.EarthRadius);
                    float actualZ = (float)(screenSpace.normalized.z * MathL.Conversions.EarthRadius);

                    sKPoints[i] = new SKPoint(actualY, actualZ);
                }
            }

            SKPath path = new SKPath();
            path.AddPoly(sKPoints);
            path.FillType = SKPathFillType.Winding;

            return path;
        }
    }
}
