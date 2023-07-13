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
using System.ComponentModel;
using loki_bms_common.Plugins;

namespace loki_bms_csharp
{
    public static partial class ProgramData
    {
        public static PluginLoader PluginLoader = new PluginLoader(LokiVersion.v0_2_0);

        public static MainWindow MainWindow;
        public static UserInterface.ScopeRenderer MainScopeRenderer => MainWindow.ScopeRenderer;

        public static ISelectableObject? SelectedObject
        {
            get => _selected;
            set
            {
                _selected = value;
                SelectionChanged?.Invoke(null, new SelectionChangedArgs { NewSelection = _selected });
            }
        }
        public static ISelectableObject? _selected;

        public delegate void SelectionChangedEventHandler(object sender, SelectionChangedArgs args);
        public static event SelectionChangedEventHandler SelectionChanged;

        public static TacticalElement? SelectedTE => (TacticalElement)SelectedObject;
        public static TrackSelection TrackSelection { get; } = new TrackSelection();

        public static SourceWindow SrcWindow;
        public static GeometryWindow GeoWindow;

        public static GeometrySettings GeometrySettings;
        internal static ColorSettings ColorSettings;

        public static List<SVGPath> DataSymbols { get; private set; }
        public static Dictionary<TrackCategory, SymbolGroup> TrackSymbols { get; private set; }
        public static Dictionary<string, SVGPath> SpecTypeSymbols { get; private set; }

        public static ObservableCollection<ISelectableObject> Bullseyes { get; set; } = new ObservableCollection<ISelectableObject>();
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
