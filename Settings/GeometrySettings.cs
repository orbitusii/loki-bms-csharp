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

namespace loki_bms_csharp.Settings
{
    [XmlRoot("GeometrySettings")]
    public class GeometrySettings : SerializableSettings<GeometrySettings>, INotifyPropertyChanged
    {
        [XmlIgnore]
        public MapGeometry Landmasses { get; set; }

        private string _ocean = "#ff0E131B";
        [XmlElement]
        public string OceanColor
        {
            get => _ocean;
            set
            {
                _ocean = value;
                PropertyChanged?.Invoke(this, new(nameof(OceanColor)));
            }
        }

        private string _landmass = "#ff303030";
        [XmlElement]
        public string LandmassColor
        {
            get => _landmass;
            set
            {
                _landmass = value;
                PropertyChanged?.Invoke(this, new(nameof(LandmassColor)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        [XmlElement("Geometry")]
        public MapGeometry[] _serializedGeometry
        {
            get => Geometries.ToArray();
            set
            {
                Geometries.Clear();
                foreach (var rawGeo in value)
                {
                    MapGeometry loaded = MapGeometry.LoadFromFile(rawGeo.FilePath, rawGeo.ConformToSurface);

                    rawGeo.Paths3D = loaded.Paths3D;

                    Geometries.Add(rawGeo);
                }
            }
        }
        [XmlIgnore]
        public ObservableCollection<MapGeometry> Geometries { get; set; } = new ObservableCollection<MapGeometry>();

        public void CacheGeometry (TangentMatrix matrix)
        {
            Landmasses?.CachePaths(matrix);
            foreach(MapGeometry geo in Geometries)
            {
                geo.CachePaths(matrix);
            }
        }

        public void SaveToFile (string filename)
        {
            SaveToFile(filename, this);
        }
    }
}
