using System.ComponentModel;
using System.Xml.Serialization;

namespace loki_bms_common.Database
{
    /// <summary>
    /// Base class to inherit from when implementing a custom Data Source. Plugins without any
    /// classes that derive from LokiDataSource will not be able to download any data from a server
    /// into Loki's database and will be restricted to operating on local data only.
    /// </summary>
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
        /// The address at which to access this source. Example: 127.0.0.1 or dcs.server.com. Do not include any port numbers!
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
        /// This is used by the Loki Database to deconflict tracks from multiple sources.
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
        /// A property that provides information on this source. Set this to provide read-only information to users within the DataSources menu.
        /// </summary>
        [XmlIgnore]
        public virtual string SourceInfo => string.Empty;

        /// <summary>
        /// An enum representing the status of a source in a more granular manner than a boolean
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

        /// <summary>
        /// The current state of this data source, e.g. "Online" indicates that the source should be receiving data.
        /// See the SourceStatus Enum documentation for more information on what each value means.
        /// </summary>
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

        /// <summary>
        /// Get the values of all fields for this DataSource and pack them into a SerializedDataSource object for saving to %AppData%/Loki BMS/DataSources.xml.
        /// Can be overridden to include custom fields not present on the base class.
        /// </summary>
        /// <returns></returns>
        public virtual SerializedDataSource GetSerializable()
        {
            return SerializedDataSource.From(this);
        }

        /// <summary>
        /// Loads all settings into this DataSource based on serialized values that were previously saved to a file.
        /// </summary>
        /// <param name="sds">A SerializedDataSource object, most likely parsed from XML, to convert into a proper DataSource object</param>
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
        /// THIS CALL IS BLOCKING! If you need behavior that might not immediately get data (e.g. testing if a server is alive and sending data), 
        /// invoke that behavior as a Task rather than running it synchronously in this call.
        /// </summary>
        public abstract void Activate();
        /// <summary>
        /// Method to call when DEACTIVATING this source. Put any logic necessary to stop and clean up the connection here.
        /// </summary>
        public abstract void Deactivate();
        /// <summary>
        /// Method to call in order to see if the source is alive and providing data.
        /// This doesn't necessarily need to be implemented if your server doesn't provide feedback on whether it's alive or not.
        /// THIS CALL IS BLOCKING! If you need behavior that might not immediately get data (e.g. testing if a server is alive and sending data), 
        /// invoke that behavior as a Task rather than running it synchronously in this call.
        /// </summary>
        /// <returns></returns>
        public abstract bool CheckAlive();

        /// <summary>
        /// Called by a Database to retrieve any new data for tracks from this DataSource.
        /// The data should already have been collected and parsed, this simply returns it for integration into the database.
        /// Remember to clear the collection of fresh data once this call is completed, otherwise the Database will attempt
        /// to intake stale data.
        /// </summary>
        /// <returns></returns>
        public abstract TrackDatum[] GetFreshData();
        /// <summary>
        /// Called by a Database to get any Tactical Elements for the mission. Tactical Elements do not update like Tracks do and will not overwrite old TEs.
        /// This method is called by manually clicking a button in the Data Source UI, not automatically like GetFreshData().
        /// </summary>
        /// <returns></returns>
        public virtual TacticalElement[] GetTEs()
        {
            return Array.Empty<TacticalElement>();
        }
    }
}
