using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Windows;
using SkiaSharp;

namespace loki_bms_csharp.UserInterface.Maps
{
    public static class MapData
    {
        public static System.Windows.Size imageSize;
        public static PathSVG[] RawPaths;
        public static Path3D[] Paths3D;

        public static SKPath[] CachedPaths;

        public static void LoadAllGeometry ()
        {
            GetPathsFromSVG();

            ConvertGeometryTo3D();
        }

        public static void GetPathsFromSVG()
        {
            System.Diagnostics.Debug.WriteLine("Attempting to deserialize WorldLandGeometry");

            XDocument doc = XDocument.Parse(Properties.Resources.WorldGeometry);

            string viewBox = doc.Root.Attribute("viewBox").Value;
            imageSize = ParseImageSize(viewBox);

            var matches = doc.Root.Descendants().Where(x => x.Name == "{http://www.w3.org/2000/svg}path").ToArray();
            RawPaths = new PathSVG[matches.Length];

            for (int i = 0; i < matches.Length; i++)
            {
                RawPaths[i] = ExtractRawPath(matches[i]);

                System.Diagnostics.Debug.WriteLine($"New Landmass PathSVG: {RawPaths[i].name}");
            }
            
            return;
        }

        public static Size ParseImageSize (string viewBox)
        {
            string[] elements = viewBox.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            double width = double.Parse(elements[2]);
            double height = double.Parse(elements[3]);

            return new Size(width, height);
        }

        public static PathSVG ExtractRawPath (XElement element)
        {
            var parent = element.Parent;
            string pathName = parent.Attribute("id").Value;

            string data = element.Attribute("d").Value;

            return new PathSVG { name = pathName, data = data };
        }

        public static void ConvertGeometryTo3D ()
        {
            List<Path3D> paths = new List<Path3D>(0);

            foreach(var pathsvg in RawPaths)
            {
                PathSVG[] subpaths = pathsvg.Subdivide();

                foreach(var subpath in subpaths)
                {
                    Path3D convertedPath = ConvertPath(subpath);
                    paths.Add(convertedPath);
                }
            }

            Paths3D = paths.ToArray();

            return;
        }

        private static Path3D ConvertPath (PathSVG path)
        {
            Point[] points = path.GetPoints();
            Vector64[] points3D = new Vector64[points.Length];

            System.Diagnostics.Debug.WriteLine(path.name + " " + points.Length);

            for (int i = 0; i < points.Length; i++)
            {
                LatLonCoord latLon = PointToLatLon(points[i]);
                points3D[i] = MathL.Conversions.LLToXYZ(latLon);
            }

            return new Path3D { Name = path.name, Points = points3D };
        }

        public static LatLonCoord PointToLatLon (Point point)
        {
            double percentDown = point.Y / imageSize.Height;
            double percentAcross = point.X / imageSize.Width;

            double Lat = (0.5 - percentDown) * 180;
            double Lon = (percentAcross - 0.5) * 360;

            return new LatLonCoord { Lat_Degrees = Lat, Lon_Degrees = Lon };
        }

        public static void CacheSKPaths (MathL.TangentMatrix cameraMatrix)
        {
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [MapData]: Caching map data paths for drawing...");

            SKPath[] cached = new SKPath[Paths3D.Length];

            for (int i = 0; i < Paths3D.Length; i++)
            {
                cached[i] = Paths3D[i].GetScreenSpacePath(cameraMatrix);
            }

            CachedPaths = cached;
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [MapData]: Done!");
        }
    }
}
