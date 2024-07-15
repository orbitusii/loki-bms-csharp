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
using O2Kml;
using O2Kml.Shapes;
using loki_bms_common.Database;

namespace loki_bms_csharp.Geometry
{
    public class MapGeometry
    {
        [XmlAttribute]
        public string Name { get; set; } = "";

        [XmlAttribute]
        public bool Visible { get; set; } = true;

        [XmlAttribute]
        public bool ConformToSurface {
            get => _conform;
            set
            {
                _conform = value;
                foreach(Path3D path in Paths3D)
                {
                    path.ConformToSurface = value;
                }
            }
        }
        private bool _conform;

        [XmlElement]
        public string FilePath { get; set; } = "";

        [XmlElement]
        public string StrokeColor
        {
            get => _strokecolor;
            set
            {
                _strokecolor = value;
                if(_strokeBrush is not null)
                {
                    _strokeBrush.Color = SKColor.Parse(_strokecolor);
                }
            }
        }
        private string _strokecolor = "#ffffffff";

        [XmlElement]
        public float StrokeWidth
        {
            get => _strokewidth;
            set
            {
                _strokewidth = value;
                if(_strokeBrush is not null)
                {
                    _strokeBrush.StrokeWidth = _strokewidth;
                }
            }
        }
        private float _strokewidth = 1;

        [XmlElement]
        public string FillColor
        {
            get => _fillcolor;
            set
            {
                _fillcolor = value;
                if (_fillBrush is not null)
                {
                    _fillBrush.Color = SKColor.Parse(_fillcolor);
                }
            }
        }
        private string _fillcolor = "#88ffffff";

        [XmlIgnore]
        public SKPaint GetStrokeBrush
        {
            get
            {
                if(_strokeBrush is null)
                {
                    _strokeBrush = new SKPaint()
                    {
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = StrokeWidth,
                        Color = SKColor.Parse(StrokeColor),
                    };
                }
                return _strokeBrush;
            }
        }
        private SKPaint? _strokeBrush;

        [XmlIgnore]
        public SKPaint GetFillBrush
        {
            get
            {
                if(_fillBrush is null)
                {
                    _fillBrush = new SKPaint()
                    {
                        Style = SKPaintStyle.Fill,
                        Color = SKColor.Parse(FillColor),
                    };
                }

                return _fillBrush;
            }
        }
        private SKPaint? _fillBrush;

        [XmlIgnore]
        public Size? imageSize;

        [XmlIgnore]
        public Path3D[] Paths3D = new Path3D[0];
        [XmlIgnore]
        public SKPath[]? CachedPaths;

        public static MapGeometry LoadFromFile (string filepath, bool conformal = true)
        {
            if (filepath.EndsWith(".svg")) return LoadFromSVG(filepath, conformal);
            else if (filepath.EndsWith(".kml")) return LoadFromKML(filepath, conformal);
            else return default;
        }

        public static MapGeometry LoadFromKML (string filepath, bool conformToSurface = true)
        {
            if (File.Exists(filepath))
            {
                Kml kml;
                using (FileStream stream = new FileStream(filepath, FileMode.Open))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(Kml));

                    kml = (Kml)ser.Deserialize(stream);
                }

                var placemarks = kml.Document.Placemarks;
                List<Path3D> geoPaths = new List<Path3D>();

                foreach (KmlPlacemark pm in placemarks)
                {
                    if(pm.Shape is KmlPoint point)
                    {
                        TacticalElement generated = new()
                        {
                            Category = TEType.Other,
                            FFS = FriendFoeStatus.Pending,
                            Position = Conversions.LLToXYZ(point.Coordinates, Conversions.EarthRadius),
                            Radius = 0,
                            Source = null,
                            Name = pm.name ?? "Imported TE",
                            SpecialInfo = $"Imported from {kml.Document.name}",
                        };

                        ProgramData.Database.AddTE(generated);

                        continue;
                    }

                    List<Vector64> points = new List<Vector64>(((KmlShape)pm.Shape).GetPoints()
                        .Select(x => Conversions.LLToXYZ(
                            new LatLonCoord
                            {
                                Lat_Degrees = x.Lat,
                                Lon_Degrees = x.Lon,
                                Alt = x.Alt / Conversions.EarthRadius
                            })));
                    geoPaths.Add(new Path3D { Name = pm.name, Points = points.ToArray(), ConformToSurface = conformToSurface, Closed = false });
                }

                if(geoPaths.Count <= 0 ) return new MapGeometry() { Name = kml.Document.name, FilePath = filepath, ConformToSurface = conformToSurface, Paths3D = Array.Empty<Path3D>() };
                else return new MapGeometry { Name = geoPaths[0]?.Name, FilePath = filepath, ConformToSurface = conformToSurface, Paths3D = geoPaths.ToArray() };
            }
            else throw new FileNotFoundException($"KML File at {filepath} was not found!");
        }

        public static MapGeometry LoadFromSVG (string filepath, bool conformal = true)
        {
            if(File.Exists(filepath))
            {
                SVGDoc doc;
                using (FileStream stream = new FileStream(filepath, FileMode.Open))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(SVGDoc));
                    doc = (SVGDoc)ser.Deserialize(stream);
                }

                var paths = GetAllPaths(doc);
                var size = ParseImageSize(doc.viewBox);

                MapGeometry newGeo = new MapGeometry
                {
                    Name = paths[0]?.name ?? "unnamed geometry",
                    FilePath = filepath,
                    imageSize = size,
                    Paths3D = ConvertGeometryTo3D(paths, size),
                    ConformToSurface = conformal,
                };

                return newGeo;
            }
            else throw new FileNotFoundException($"SVG File at {filepath} was not found!");
        }

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

        public void CachePaths(TangentMatrix cameraMatrix)
        {
            CachedPaths = ConvertToSKPaths(Paths3D, cameraMatrix);
        }

        public static SKPath[] ConvertToSKPaths(Path3D[] paths, TangentMatrix cameraMatrix)
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

        public static SVGPath[] GetAllPaths (SVGDoc svg)
        {
            List<SVGPath> paths = new List<SVGPath>(0);

            if (svg.paths != null)
            {
                foreach (SVGPath path in svg.paths)
                {
                    System.Diagnostics.Debug.WriteLine($"Found path {path.name}");
                    paths.Add(path);
                }
            }
            if (svg.groups != null)
            {

                foreach (SVGGroup group in svg.groups)
                {
                    foreach (SVGPath g_p in group.paths)
                    {
                        paths.Add(g_p);
                    }
                }
            }

            return paths.ToArray();
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
                    Path3D convertedPath = ConvertSVGToPath(subpath, imageSize);
                    paths.Add(convertedPath);
                }
            }

            Path3D[] pathsArray = paths.ToArray();

            return pathsArray;
        }

        private static Path3D ConvertSVGToPath (SVGPath path, Size imageSize)
        {
            Point[] points = path.GetPoints();
            Vector64[] points3D = new Vector64[points.Length];

            System.Diagnostics.Debug.WriteLine(path.name + ": " + points.Length + " points");

            for (int i = 0; i < points.Length; i++)
            {
                LatLonCoord latLon = SVGPointToLatLon(points[i], imageSize);
                points3D[i] = Conversions.LLToXYZ(latLon);
            }

            return new Path3D { Name = path.name, Points = points3D };
        }

        public static LatLonCoord SVGPointToLatLon (Point point, Size imageSize)
        {
            double percentDown = point.Y / imageSize.Height;
            double percentAcross = point.X / imageSize.Width;

            double Lat = (0.5 - percentDown) * 180;
            double Lon = (percentAcross - 0.5) * 360;

            return new LatLonCoord { Lat_Degrees = Lat, Lon_Degrees = Lon };
        }
    }
}
