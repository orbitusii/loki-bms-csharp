using System.ComponentModel;
using System.Xml.Serialization;

namespace loki_bms_common.Database
{
    public abstract class LokiDataSource : INotifyPropertyChanged
    {
        /// <summary>
        /// Is this source active and attempting to get data? Do not use this property to activate/deactivate the source, call Activate() and Deactivate() instead.
        /// </summary>
        [XmlIgnore]
        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                OnPropertyChanged("Active");
                OnPropertyChanged("CanEditPollRate");
            }
        }
        private bool _active = false;

        public bool CanEditPollRate => !Active;

        /// <summary>
        /// The display name for this source.
        /// </summary>
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
        private string _name = "New Data Source";

        /// <summary>
        /// The address at which to access this source. Example: 127.0.0.1 or dcs.server.com
        /// </summary>
        [XmlAttribute]
        public string Address { get; set; } = "127.0.0.1";

        /// <summary>
        /// The port to use when accessing the source at the specified Address. Example: 50051, 10052
        /// </summary>
        [XmlAttribute]
        public string Port { get; set; } = "50051";

        /// <summary>
        /// Speed at which fast-updating objects should be polled. E.g. aircraft, weapons, etc.
        /// </summary>
        [XmlAttribute]
        public string PollRate { get; set; } = "10";

        /// <summary>
        /// Speed at which slow-updating objects should be polled. E.g. boats, land units, etc.
        /// </summary>
        [XmlAttribute]
        public string SlowPollrate { get; set; } = "30";

        /// <summary>
        /// The range of Track Numbers this data source can use. Once it reaches TNMax, new tracks will roll over back to TNMin.
        /// </summary>
        [XmlElement]
        public TrackNumberRange TNRange { get; set; } = new TrackNumberRange { TNMin = -1, TNMax = -1 };

        /// <summary>
        /// The symbol used to render this source's raw data.
        /// </summary>
        [XmlAttribute("Symbol")]
        public string DataSymbol { get; set; } = "LineVert";

        /// <summary>
        /// The color used to render this source's raw data.
        /// </summary>
        [XmlAttribute("Color")]
        public string DataColor { get; set; } = "#ff6600";

        /// <summary>
        /// A property that provides information on this source. Set this to provide information to users.
        /// </summary>
        [XmlIgnore]
        public virtual string SourceInfo => string.Empty;

        /// <summary>
        /// An enum representing the status of this source in a more granular manner than a boolean
        /// </summary>
        public enum SourceStatus
        {
            /// <summary>
            /// This source is offline deliberately - usually by a user clicking "Deactivate."
            /// </summary>
            Offline,
            /// <summary>
            /// This source is starting up and attempting to connect.
            /// </summary>
            Starting,
            /// <summary>
            /// This source has successfully started and is receiving data (probably!)
            /// </summary>
            Active,
            /// <summary>
            /// This source attempted to connect and get data, but the server either didn't respond or threw an error.
            /// </summary>
            Disconnected,
        }

        [XmlIgnore]
        public SourceStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }
        public SourceStatus _status = SourceStatus.Offline;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public virtual SerializedDataSource GetSerializable()
        {
            return SerializedDataSource.From(this);
        }

        public virtual void LoadSerializable(SerializedDataSource sds)
        {
            Name = sds.Name;
            Address = sds.Address;
            Port = sds.Port;
            PollRate = sds.PollRate;
            SlowPollrate = sds.SlowPollrate;
            TNRange = sds.TNRange;
            DataSymbol = sds.DataSymbol;
            DataColor = sds.DataColor;
        }

        /// <summary>
        /// Method to call when ACTIVATING this source. Put any logic necessary to initiate the connection here.
        /// </summary>
        public abstract void Activate();
        /// <summary>
        /// Method to call when DEACTIVATING this source. Put any logic necessary to stop and clean up the connection here.
        /// </summary>
        public abstract void Deactivate();
        /// <summary>
        /// Method to call in order to see if the source is alive and providing data.
        /// This doesn't necessarily need to be implemented if your server doesn't provide feedback
        /// on whether it's alive or not.
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckAlive();

        /// <summary>
        /// Called by a Database to retrieve any new data for tracks from this DataSource.
        /// The data should already have been collected in another thread,
        /// this simply returns it for integration into the database.
        /// </summary>
        /// <returns></returns>
        public abstract TrackDatum[] GetFreshData();
        /// <summary>
        /// Called by a Database to get any Tactical Elements for the mission.
        /// These shouldn't update much, if at all.
        /// </summary>
        /// <returns></returns>
        public virtual TacticalElement[] GetTEs()
        {
            return Array.Empty<TacticalElement>();
        }
    }
}
