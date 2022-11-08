using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using loki_bms_csharp.MathL;
using loki_bms_csharp.Settings;
using loki_bms_csharp.Geometry;
using loki_bms_csharp.Geometry.SVG;
using System.Reflection;

namespace loki_bms_csharp
{
    public static class ProgramData
    {
        public static MainWindow MainWindow;
        public static SourceWindow SrcWindow;

        public static MapGeometry WorldLandmasses;
        public static MapGeometry DCSMaps;
        public static List<SVGPath> DataSymbols { get; private set; }

        public static ViewSettings ViewSettings;
        public static List<Database.DataSource> DataSources;

        public static string AppDataPath;
        private static string Delimiter = "\\";
        private static Assembly execAssy;

        public static void Initialize(MainWindow window)
        {
            execAssy = Assembly.GetExecutingAssembly();

            AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"{Delimiter}Loki-BMS{Delimiter}";
            if (!Directory.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);

            Debug.WriteLine($"[PROGRAM]: AppData at {AppDataPath}");

            MainWindow = window;

            LoadPermanentMapData();
            LoadSymbology();

            ViewSettings = LoadViewSettings(AppDataPath + "Views.xml");
            DataSources = LoadDataSources(AppDataPath + "DataSources.xml");
            //SymbolSettings = LoadSymbolSettings(AppDataPath + "Symbology.xml");

            WorldLandmasses.CachePaths(ViewSettings.CameraMatrix);
            DCSMaps.CachePaths(ViewSettings.CameraMatrix);

            ViewSettings.OnViewCenterChanged += WorldLandmasses.CachePaths;
            ViewSettings.OnViewCenterChanged += DCSMaps.CachePaths;

            Debug.Write("Data Sources: ");
            foreach (var source in DataSources)
            {
                Debug.Write($"{source.Name}, Active = {source.Active}; ");
            }
            Debug.WriteLine($"Total: {DataSources.Count}");
        }

        public static void LoadPermanentMapData()
        {
            Stream Landmasses = execAssy.GetManifestResourceStream("loki_bms_csharp.Resources.WorldLandmasses.svg");
            WorldLandmasses = MapGeometry.LoadGeometryFromStream(Landmasses);

            Stream mapBounds = execAssy.GetManifestResourceStream("loki_bms_csharp.Resources.DCSMapExtents.svg");
            DCSMaps = MapGeometry.LoadGeometryFromStream(mapBounds);
            foreach (var path in DCSMaps.Paths3D)
            {
                path.ConformToSurface = true;
            }
        }

        public static void LoadSymbology()
        {
            Stream symbolsStream = execAssy.GetManifestResourceStream("loki_bms_csharp.Resources.DataSymbols.svg");
            XmlSerializer ser = new XmlSerializer(typeof(SVGDoc));
            SVGDoc svg = (SVGDoc)ser.Deserialize(symbolsStream);

            DataSymbols = new List<SVGPath>(0);

            if (svg.paths != null)
            {
                foreach (SVGPath path in svg.paths)
                {
                    //System.Diagnostics.Debug.WriteLine($"Found path {path.name}");
                    DataSymbols.Add(path);
                }
            }
            if (svg.groups != null)
            {

                foreach (SVGGroup group in svg.groups)
                {
                    foreach (SVGPath g_p in group.paths)
                    {
                        DataSymbols.Add(g_p);
                    }
                }
            }

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

        public static List<Database.DataSource> LoadDataSources(string filePath)
        {
            if (File.Exists(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DataSourceDoc));

                using var stream = new FileStream(filePath, FileMode.OpenOrCreate);

                var foundSources = (DataSourceDoc)serializer.Deserialize(stream);

                if (foundSources.Items.Length > 0)
                {
                    return new List<Database.DataSource>(foundSources.Items);
                }
            }

            return new List<Database.DataSource> { new Database.DataSource() };
        }

        public static void Shutdown ()
        {
            SrcWindow?.Close();

            SaveViewSettings(AppDataPath + "Views.xml");
            SaveDataSources(AppDataPath + "DataSources.xml");

            foreach (var source in DataSources)
            {
                source.Deactivate();
            }
        }

        public static bool SaveViewSettings (string filePath)
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
                ser.Serialize(stream, new DataSourceDoc { Items = DataSources.ToArray() });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    [XmlRoot("DataSources")]
    public class DataSourceDoc
    {
        public DataSourceDoc() { }

        [XmlElement("source")]
        public Database.DataSource[] Items { get; set; }
    }

    [XmlRoot("ZoomPresets")]
    public class PresetsDoc
    {
        public PresetsDoc() { }

        [XmlElement("preset")]
        public UserInterface.ZoomPreset[] presets { get; set; }
    }
}
