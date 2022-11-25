using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using loki_bms_csharp.Geometry;

namespace loki_bms_csharp.Settings
{
    [XmlRoot("Geometry")]
    public class GeometrySettings : SerializableSettings<GeometrySettings>, INotifyPropertyChanged
    {
        [XmlIgnore]
        public MapGeometry WorldGeometry { get; set; }

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

        [XmlIgnore]
        public ObservableCollection<MapGeometry> Geometries { get; set; }

        [XmlElement]
        public List<MapGeometry> serializedGeometry
        {
            get => Geometries.ToList();
            set
            {
                Geometries = new(value.Select(x => MapGeometry.LoadFromKML(x.FilePath)));
            }
        }
    }
}
