using loki_bms_common.Database;
using loki_bms_csharp.Geometry.SVG;
using loki_bms_csharp.Geometry;
using loki_bms_csharp.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace loki_bms_csharp
{
    public static partial class ProgramData
    {
        public static void Initialize()
        {
            execAssy = Assembly.GetExecutingAssembly();
            EmbeddedPath = "loki_bms_csharp.Resources.";

            BaseDirPath = AppDomain.CurrentDomain.BaseDirectory;
            ResourcesPath = BaseDirPath + "Resources" + Delimiter;
            Debug.WriteLine($"[PROGRAM]: Resources at {ResourcesPath}");

            AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"{Delimiter}Loki-BMS{Delimiter}";
            if (!Directory.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);

            Debug.WriteLine($"[PROGRAM]: AppData at {AppDataPath}");

            PluginLoader.LoadPlugins();

            LoadSymbology();
            LoadGeometries();

            ViewSettings = LoadViewSettings(AppDataPath + "Views.xml");

            ColorSettings = LoadColorSettings();

            Database = new TrackDatabase(1000);
            Database.DataSources = LoadDataSources(AppDataPath + "DataSources.xml");

            foreach (LokiDataSource source in DataSources)
            {
                if (source.TNRange == null || source.TNRange.TNMax < 0)
                {
                    source.TNRange = new TrackNumberRange
                    {
                        TNMin = (short)(1000 * DataSources.IndexOf(source)),
                        TNMax = (short)(source.TNRange.TNMin + 500)
                    };
                }
            }

            GeometrySettings.CacheGeometry(ViewSettings.CameraMatrix);

            ViewSettings.OnViewCenterChanged += GeometrySettings.CacheGeometry;

            Debug.Write("Data Sources: ");
            foreach (var source in DataSources)
            {
                Debug.Write($"{source.Name}, Active = {source.Active}; ");
            }
            Debug.WriteLine($"Total: {DataSources.Count}");
        }

        public static void LoadSymbology()
        {
            DataSymbols = GetPathsFromFile(ResourcesPath + "DataSymbols.svg");

            TrackSymbols = new Dictionary<TrackCategory, SymbolGroup>();
            TrackSymbols[TrackCategory.None] = new SymbolGroup(GetPathsFromFile(ResourcesPath + "Tracks_General.svg"));
            TrackSymbols[TrackCategory.Air] = new SymbolGroup(GetPathsFromFile(ResourcesPath + "Tracks_Air.svg"));
            TrackSymbols[TrackCategory.Ship] = new SymbolGroup(new List<SVGPath>());
            TrackSymbols[TrackCategory.Ground] = new SymbolGroup(new List<SVGPath>());

            SpecTypeSymbols = new Dictionary<string, SVGPath>() { { "", null }, };
            var specTypeList = GetPathsFromFile(ResourcesPath + "SpecTypes.svg");

            foreach (var specType in specTypeList)
            {
                SpecTypeSymbols[specType.name] = specType;
            }
        }

        public static List<SVGPath> GetPathsFromFile(string filepath)
        {
            FileStream symbolStream = new FileStream(filepath, FileMode.Open);
            XmlSerializer ser = new XmlSerializer(typeof(SVGDoc));
            SVGDoc svg = (SVGDoc)ser.Deserialize(symbolStream);

            List<SVGPath> paths = new List<SVGPath>(0);

            if (svg.paths != null)
            {
                paths.AddRange(svg.paths);
            }
            if (svg.groups != null)
            {
                foreach (SVGGroup group in svg.groups)
                {
                    paths.AddRange(group.paths);
                }
            }

            return paths;
        }

        public static void LoadGeometries()
        {
            GeometrySettings? loaded = GeometrySettings.LoadFromFile(AppDataPath + "Geometry.xml");

            if (loaded is null)
            {
                loaded = new GeometrySettings()
                {
                    Geometries = new ObservableCollection<MapGeometry>()
                    {
                        MapGeometry.LoadFromKML(ResourcesPath + "DCSMaps.kml"),
                    },
                };
                loaded.Geometries[0].Name = "DCS Map Extents";
                loaded.Geometries[0].ConformToSurface = true;
                loaded.Geometries[0].StrokeColor = "#84ffffff";
                loaded.Geometries[0].FillColor = "#00ffffff";
            }
            loaded.Landmasses = MapGeometry.LoadFromSVG(ResourcesPath + "WorldLandmasses.svg");

            GeometrySettings = loaded;
        }

        public static ViewSettings LoadViewSettings(string filePath)
        {
            if (File.Exists(filePath))
            {
                XmlSerializer ser = new XmlSerializer(typeof(ViewSettings));
                using var stream = new FileStream(filePath, FileMode.OpenOrCreate);

                var vs = (ViewSettings)ser.Deserialize(stream);
                vs.UpdateViewPosition(vs.ViewCenter);
                vs.SetZoom(vs.ZoomIncrement);

                return vs;
            }
            else
            {
                var _viewSettings = new ViewSettings();

                _viewSettings.UpdateViewPosition(new LatLonCoord { Lat_Degrees = 0, Lon_Degrees = 0 });
                _viewSettings.SetZoom(16);

                return _viewSettings;
            }
        }

        public static ObservableCollection<LokiDataSource> LoadDataSources(string filePath)
        {
            ObservableCollection<LokiDataSource> sources = new ObservableCollection<LokiDataSource>();

            if (File.Exists(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DataSourceDoc));

                using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
                DataSourceDoc? foundSources;

                try
                {
                    foundSources = (DataSourceDoc)serializer.Deserialize(stream);
                }
                catch (InvalidOperationException)
                {
                    return sources;
                }

                if (foundSources is DataSourceDoc && foundSources.Items.Length > 0)
                {
                    foreach (var source in foundSources.Items)
                    {
                        Type t;
                        if (PluginLoader.DataSourceTypes.TryGetValue(source.SourceType, out t))
                        {
                            Debug.WriteLine($"[SETTINGS][LOG] Successfully found a type for SDS of type {source.SourceType}");
                            LokiDataSource ds = Activator.CreateInstance(t) as LokiDataSource;
                            ds.LoadSerializable(source);

                            sources.Add(ds);
                        }
                        else
                        {
                            Debug.WriteLine($"[SETTINGS][WARNING] Unable to find a valid type for Serialized Data Source of type {source.SourceType}");
                            continue;
                        }
                    }
                }
            }

            Debug.WriteLine($"Deserialized {sources.Count} Data Sources successfully");
            return sources;
        }

        internal static ColorSettings LoadColorSettings()
        {
            ColorSettings? cs = ColorSettings.LoadFromFile(AppDataPath + "Colors.xml");

            return cs ?? new ColorSettings();
        }

        public static (double dist, double heading_rads) GetPositionRelativeToBullseye(Vector64 pos)
        {
            Vector64 BEpos = BullseyeCartesian;

            double angle = Vector64.AngleBetween(BEpos, pos);
            double arcLength = Conversions.EarthRadius * angle;

            var hdg = Conversions.GetSurfaceMotion(BEpos, pos - BEpos).heading;

            return (arcLength, hdg);
        }

        public static void Shutdown()
        {
            SrcWindow?.Close();
            GeoWindow?.Close();

            SaveViewSettings(AppDataPath + "Views.xml");
            SaveDataSources(AppDataPath + "DataSources.xml");
            GeometrySettings.SaveToFile(AppDataPath + "Geometry.xml");
            ColorSettings.SaveToFile(AppDataPath + "Colors.xml");

            foreach (var source in DataSources)
            {
                source.Deactivate();
            }
        }

        public static bool SaveViewSettings(string filePath)
        {
            if (!File.Exists(filePath)) File.Create(filePath).Close();

            XmlSerializer ser = new XmlSerializer(typeof(ViewSettings));
            using var stream = new FileStream(filePath, FileMode.Truncate);

            try
            {
                ser.Serialize(stream, ViewSettings);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool SaveDataSources(string filePath)
        {
            if (!File.Exists(filePath)) File.Create(filePath).Close();

            XmlSerializer ser = new XmlSerializer(typeof(DataSourceDoc));
            using var stream = new FileStream(filePath, FileMode.Truncate);

            try
            {
                ser.Serialize(stream, new DataSourceDoc { Items = DataSources.Select(x => x.GetSerializable()).ToArray() });
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[SETTINGS][ERROR] Failed to store Data Sources. Thrown: {e}");
                return false;
            }
        }
    }
}
