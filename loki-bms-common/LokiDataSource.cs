using loki_bms_common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel;
using loki_bms_common.Database;

namespace loki_bms_common
{
    public abstract class LokiDataSource
    {
        private string _name = "New Data Source";
        [XmlAttribute]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        [XmlAttribute]
        public string Address { get; set; } = "127.0.0.1";
        [XmlAttribute]
        public string Port { get; set; } = "50051";
        [XmlAttribute]
        public string PollRate { get; set; } = "10";
        [XmlAttribute]
        public string SlowPollrate { get; set; } = "30";
        [XmlElement]
        public TrackNumberRange TNRange { get; set; } = new TrackNumberRange { TNMin = -1, TNMax = -1 };

        [XmlAttribute("Symbol")]
        public string DataSymbol { get; set; } = "LineVert";

        [XmlAttribute("Color")]
        private string _dataColor = "#dd6600";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public abstract void Activate();
        public abstract void Deactivate();

        public abstract bool CheckAlive();

        public abstract RawTrackDatum[] GetFreshData();

        public virtual TacticalElement[] GetTEs() { } 
    }
}
