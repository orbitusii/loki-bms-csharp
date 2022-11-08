using loki_bms_csharp.Geometry.SVG;
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

namespace loki_bms_csharp.Geometry
{
    public class MapGeometry
    {
        public Size imageSize;
        public Path3D[] Paths3D;

        public SKPath[] CachedPaths;

        public static MapGeometry LoadGeometryFromStream (Stream source)
        {
            (Size size, SVGPath[] paths) = UnpackSVG(source);

            MapGeometry newData = new MapGeometry
            {
                imageSize = size,
                Paths3D = ConvertGeometryTo3D(paths, size),
            };

            return newData;
        }

        public void CachePaths(MathL.TangentMatrix cameraMatrix)
        {
            CachedPaths = ConvertToSKPaths(Paths3D, cameraMatrix);
        }

        public static (Size size, SVGPath[] paths) UnpackSVG(Stream source)
        {
            System.Diagnostics.Debug.WriteLine("Attempting to deserialize geometry from an SVG");

            XmlSerializer ser = new XmlSerializer(typeof(SVGDoc));

            SVGDoc svg = (SVGDoc)ser.Deserialize(source);
            Size imageSize = ParseImageSize(svg.viewBox);

            List<SVGPath> paths = new List<SVGPath>(0);

            if (svg.paths != null)
            {
                foreach (SVGPath path in svg.paths)
                {
                    System.Diagnostics.Debug.WriteLine($"Found path {path.name}");
                    paths.Add(path);
                }
            }
            if(svg.groups != null)
            {

                foreach(SVGGroup group in svg.groups)
                {
                    foreach(SVGPath g_p in group.paths)
                    {
                        paths.Add(g_p);
                    }
                }
            }

            return (imageSize, paths.ToArray());

        }

        public static Size ParseImageSize (string viewBox)
        {
            string[] elements = viewBox.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            double width = double.Parse(elements[2]);
            double height = double.Parse(elements[3]);

            return new Size(width, height);
        }

        public static SVGPath ExtractRawPath (XElement element)
        {
            var parent = element.Parent;
            string pathName = element.Attribute("id") != null ? element.Attribute("id").Value : parent.Attribute("id")?.Value;

            string data = element.Attribute("d").Value;

            return new SVGPath { name = pathName, data = data };
        }

        public static Path3D[] ConvertGeometryTo3D (SVGPath[] originals, Size imageSize)
        {
            List<Path3D> paths = new List<Path3D>(0);

            foreach(var pathsvg in originals)
            {
                SVGPath[] subpaths = pathsvg.Subdivide();

                foreach(var subpath in subpaths)
                {
                    Path3D convertedPath = ConvertPath(subpath, imageSize);
                    paths.Add(convertedPath);
                }
            }

            Path3D[] pathsArray = paths.ToArray();

            return pathsArray;
        }

        private static Path3D ConvertPath (SVGPath path, Size imageSize)
        {
            Point[] points = path.GetPoints();
            Vector64[] points3D = new Vector64[points.Length];

            System.Diagnostics.Debug.WriteLine(path.name + ": " + points.Length + " points");

            for (int i = 0; i < points.Length; i++)
            {
                LatLonCoord latLon = PointToLatLon(points[i], imageSize);
                points3D[i] = MathL.Conversions.LLToXYZ(latLon);
            }

            return new Path3D { Name = path.name, Points = points3D };
        }

        public static LatLonCoord PointToLatLon (Point point, Size imageSize)
        {
            double percentDown = point.Y / imageSize.Height;
            double percentAcross = point.X / imageSize.Width;

            double Lat = (0.5 - percentDown) * 180;
            double Lon = (percentAcross - 0.5) * 360;

            return new LatLonCoord { Lat_Degrees = Lat, Lon_Degrees = Lon };
        }

        public static SKPath[] ConvertToSKPaths (Path3D[] paths, MathL.TangentMatrix cameraMatrix)
        {
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [MapData]: Caching map data paths for drawing...");

            SKPath[] cached = new SKPath[paths.Length];

            for (int i = 0; i < paths.Length; i++)
            {
                cached[i] = paths[i].GetScreenSpacePath(cameraMatrix);
            }

            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [MapData]: Done!");
            return cached;
        }
    }
}
