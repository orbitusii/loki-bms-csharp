﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using loki_bms_csharp.Database;
using loki_bms_csharp.Settings;
using loki_bms_csharp.Geometry;
using loki_bms_csharp.Geometry.SVG;
using System.Reflection;
using loki_bms_csharp.Windows;

namespace loki_bms_csharp
{
    public static class ProgramData
    {
        public static MainWindow MainWindow;
        public static UserInterface.ScopeRenderer MainScopeRenderer => MainWindow.ScopeRenderer;

        public static TrackSelection TrackSelection { get; set; } = new TrackSelection();

        public static SourceWindow SrcWindow;
        public static GeometryWindow GeoWindow;

        public static MapGeometry WorldLandmasses;
        public static ObservableCollection<MapGeometry> UserGeometry { get; set; }

        public static List<SVGPath> DataSymbols { get; private set; }
        public static Dictionary<TrackCategory, SymbolGroup> TrackSymbols { get; private set; }
        public static Dictionary<string, SVGPath> SpecTypeSymbols { get; private set; }

        public static ViewSettings ViewSettings;
        public static ObservableCollection<DataSource> DataSources;
        // TODO: add source reordering in the SourcesWindow

        public static string AppDataPath;
        public static string EmbeddedPath;
        private static string Delimiter = "\\";
        private static Assembly execAssy;

        public static void Initialize()
        {
            execAssy = Assembly.GetExecutingAssembly();
            EmbeddedPath = "loki_bms_csharp.Resources.";

            AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"{Delimiter}Loki-BMS{Delimiter}";
            if (!Directory.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);

            Debug.WriteLine($"[PROGRAM]: AppData at {AppDataPath}");

            LoadPermanentMapData();
            LoadSymbology();

            ViewSettings = LoadViewSettings(AppDataPath + "Views.xml");
            DataSources = LoadDataSources(AppDataPath + "DataSources.xml");
            //SymbolSettings = LoadSymbolSettings(AppDataPath + "Symbology.xml");

            foreach (DataSource source in DataSources)
            {
                if(source.TNRange == null || source.TNRange.TNMin < 0)
                {
                    source.TNRange = new TrackNumberRange
                    {
                        TNMin = (short)(1000 * DataSources.IndexOf(source)),
                        TNMax = (short)(source.TNRange.TNMin + 500)
                    };
                }
            }

            WorldLandmasses.CachePaths(ViewSettings.CameraMatrix);
            UserGeometry[0].CachePaths(ViewSettings.CameraMatrix);

            ViewSettings.OnViewCenterChanged += WorldLandmasses.CachePaths;
            ViewSettings.OnViewCenterChanged += UserGeometry[0].CachePaths;

            Debug.Write("Data Sources: ");
            foreach (var source in DataSources)
            {
                Debug.Write($"{source.Name}, Active = {source.Active}; ");
            }
            Debug.WriteLine($"Total: {DataSources.Count}");
        }

        public static void LoadPermanentMapData()
        {
            Stream Landmasses = execAssy.GetManifestResourceStream(EmbeddedPath + "WorldLandmasses.svg");
            WorldLandmasses = MapGeometry.LoadGeometryFromStream(Landmasses);

            Stream mapBounds = execAssy.GetManifestResourceStream(EmbeddedPath + "DCSMapExtents.svg");
            UserGeometry = new ObservableCollection<MapGeometry>() { MapGeometry.LoadGeometryFromStream(mapBounds) };
            UserGeometry[0].Name = "DCS Map Extents";
            UserGeometry[0].FillColor = "#00000000";
            UserGeometry[0].StrokeColor = "#88ffffff";

            foreach (var path in UserGeometry[0].Paths3D)
            {
                path.ConformToSurface = true;
            }
        }

        public static void LoadSymbology()
        {
            DataSymbols = GetPathsFromEmbeddedFile("DataSymbols.svg");

            TrackSymbols = new Dictionary<TrackCategory, SymbolGroup>();
            TrackSymbols[TrackCategory.None] = new SymbolGroup(GetPathsFromEmbeddedFile("Tracks_General.svg"));
            TrackSymbols[TrackCategory.Air] = new SymbolGroup(GetPathsFromEmbeddedFile("Tracks_Air.svg"));
            TrackSymbols[TrackCategory.Ship] = new SymbolGroup(new List<SVGPath>());
            TrackSymbols[TrackCategory.Ground] = new SymbolGroup(new List<SVGPath>());

            SpecTypeSymbols = new Dictionary<string, SVGPath>() { { "", null }, };
            var specTypeList = GetPathsFromEmbeddedFile("SpecTypes.svg");

            foreach(var specType in specTypeList)
            {
                SpecTypeSymbols[specType.name] = specType;
            }
        }

        public static List<SVGPath> GetPathsFromEmbeddedFile (string file)
        {
            Stream symbolsStream = execAssy.GetManifestResourceStream(EmbeddedPath + file);
            XmlSerializer ser = new XmlSerializer(typeof(SVGDoc));
            SVGDoc svg = (SVGDoc)ser.Deserialize(symbolsStream);

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

        public static ObservableCollection<DataSource> LoadDataSources(string filePath)
        {
            if (File.Exists(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DataSourceDoc));

                using var stream = new FileStream(filePath, FileMode.OpenOrCreate);

                var foundSources = (DataSourceDoc)serializer.Deserialize(stream);

                if (foundSources.Items.Length > 0)
                {
                    return new ObservableCollection<DataSource>(foundSources.Items);
                }
            }


            return new ObservableCollection<DataSource> { new DataSource() };
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
                ser.Serialize(stream, new DataSourceDoc { Items = new List<Database.DataSource>(DataSources).ToArray() });
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
