global using loki_bms_common;
global using loki_bms_common.MathL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using loki_bms_common.Database;
using loki_bms_csharp.Settings;
using loki_bms_csharp.Geometry;
using loki_bms_csharp.Geometry.SVG;
using System.Reflection;
using loki_bms_csharp.Windows;
using loki_bms_csharp.Plugins;
using System.Linq;
using System.Windows.Documents;

namespace loki_bms_csharp
{
    public static class ProgramData
    {
        public static PluginLoader PluginLoader = new PluginLoader(LokiVersion.v0_2_0);

        public static MainWindow MainWindow;
        public static UserInterface.ScopeRenderer MainScopeRenderer => MainWindow.ScopeRenderer;

        public static TrackSelection TrackSelection { get; set; } = new TrackSelection();

        public static SourceWindow SrcWindow;
        public static GeometryWindow GeoWindow;

        public static GeometrySettings GeometrySettings;
        internal static ColorSettings ColorSettings;

        public static List<SVGPath> DataSymbols { get; private set; }
        public static Dictionary<TrackCategory, SymbolGroup> TrackSymbols { get; private set; }
        public static Dictionary<string, SVGPath> SpecTypeSymbols { get; private set; }

        public static LatLonCoord BullseyePos { get; set; }
        public static Vector64 BullseyeCartesian => Conversions.LLToXYZ(BullseyePos, Conversions.EarthRadius);

        public static ViewSettings ViewSettings;
        public static ObservableCollection<LokiDataSource> DataSources => Database.DataSources;
        public static TrackDatabase Database;
        // TODO: add source reordering in the SourcesWindow

        public static string AppDataPath { get; private set; }
        public static string EmbeddedPath { get; private set; }
        public static string BaseDirPath { get; private set; }
        public static string ResourcesPath { get; private set; }
        private static string Delimiter = "\\";
        private static Assembly execAssy;

        public static void Initialize()
        {
            execAssy = Assembly.GetExecutingAssembly();
            EmbeddedPath = "loki_bms_csharp.Resources.";

            BaseDirPath = AppDomain.CurrentDomain.BaseDirectory;
            ResourcesPath = BaseDirPath + "Resources" + Delimiter;

            AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"{Delimiter}Loki-BMS{Delimiter}";
            if (!Directory.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);

            Debug.WriteLine($"[PROGRAM]: AppData at {AppDataPath}");

            PluginLoader.LoadPlugins();

            LoadSymbology();
            LoadGeometries();

            ViewSettings = LoadViewSettings(AppDataPath + "Views.xml");

            ColorSettings = LoadColorSettings(AppDataPath + "Colors.xml");

            Database = new TrackDatabase(1000);
            Database.DataSources = LoadDataSources(AppDataPath + "DataSources.xml");

            //SymbolSettings = LoadSymbolSettings(AppDataPath + "Symbology.xml");

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
                        MapGeometry.LoadFromSVG(ResourcesPath + "DCSMapExtents.svg"),
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
                catch(InvalidOperationException)
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

        internal static ColorSettings LoadColorSettings(string filepath)
        {
            if (File.Exists(filepath))
            {
                XmlSerializer ser = new XmlSerializer(typeof(ColorSettings));

                using var stream = new FileStream(filepath, FileMode.OpenOrCreate);

                ColorSettings? cs = (ColorSettings)ser.Deserialize(stream);

                if (cs is not null) return cs;
            }

            return new ColorSettings();
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

    [XmlRoot("DataSources")]
    public class DataSourceDoc
    {
        public DataSourceDoc() { }

        [XmlElement("source")]
        public SerializedDataSource[] Items { get; set; }
    }

    [XmlRoot("ZoomPresets")]
    public class PresetsDoc
    {
        public PresetsDoc() { }

        [XmlElement("preset")]
        public UserInterface.ZoomPreset[] presets { get; set; }
    }
}
