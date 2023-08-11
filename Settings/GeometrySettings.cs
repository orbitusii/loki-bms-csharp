using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using loki_bms_csharp.Geometry;
using loki_bms_common.MathL;
using System.IO;
using System.Diagnostics;

namespace loki_bms_csharp.Settings
{
    [XmlRoot("GeometrySettings")]
    public class GeometrySettings : SerializableSettings<GeometrySettings>
    {
        public override GeometrySettings Original => this;

        [XmlIgnore]
        public MapGeometry Landmasses { get; set; }

        [XmlElement("Geometry")]
        public MapGeometry[] _serializedGeometry
        {
            get => Geometries.ToArray();
            set
            {
                Geometries.Clear();
                foreach (var rawGeo in value)
                    LoadGeoFile(rawGeo);
            }
        }
        [XmlIgnore]
        public ObservableCollection<MapGeometry> Geometries { get; set; } = new ObservableCollection<MapGeometry>();

        private void LoadGeoFile(MapGeometry geo)
        {
            try
            {
                MapGeometry loaded = MapGeometry.LoadFromFile(geo.FilePath, geo.ConformToSurface);

                geo.Paths3D = loaded.Paths3D;

                Geometries.Add(geo);
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"[GEOMETRY][ERROR] Tried to load Gemoetry from {geo.FilePath} but the file doesn't exist!");
            }
        }

        public void CacheGeometry(TangentMatrix matrix)
        {
            Landmasses?.CachePaths(matrix);
            foreach (MapGeometry geo in Geometries)
            {
                geo.CachePaths(matrix);
            }
        }
    }
}
