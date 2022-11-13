using Grpc.Net.Client;
using RurouniJones.Dcs.Grpc.V0.Common;
using RurouniJones.Dcs.Grpc.V0.Hook;
using RurouniJones.Dcs.Grpc.V0.Mission;
using RurouniJones.Dcs.Grpc.V0.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows;
using System.ComponentModel;

namespace loki_bms_csharp.Database
{
    public class DataSource: INotifyPropertyChanged
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

        [XmlAttribute]
        public string DataSymbol { get; set; } = "LineVert";

        private string _dataColor = "#dd6600";
        [XmlAttribute("Color")]
        public string DataColor
        {
            get => _dataColor;
            set
            {
                _dataColor = value;

                var color = SkiaSharp.SKColor.TryParse(DataColor, out var _parsed) ? _parsed : SkiaSharp.SKColors.White;
                _paintCached = new SkiaSharp.SKPaint { Style = SkiaSharp.SKPaintStyle.Stroke, StrokeWidth = 2, Color = color };
            }
        }
        public SkiaSharp.SKPath GetSKPath => ProgramData.DataSymbols.Find(x => x.name == DataSymbol)?.SKPath;

        private SkiaSharp.SKPaint _paintCached;
        public SkiaSharp.SKPaint GetSKPaint
        {
            get
            {
                if(_paintCached == null)
                {
                    var color = SkiaSharp.SKColor.TryParse(DataColor, out var _parsed) ? _parsed : SkiaSharp.SKColors.White;
                    _paintCached = new SkiaSharp.SKPaint { Style = SkiaSharp.SKPaintStyle.Stroke, StrokeWidth = 2, Color = color };
                }

                return _paintCached;
            }
        }

        // DONE: make this value reflect properly on SourcesWindow when it fails to connect
        private bool _active = false;
        [XmlAttribute]
        public bool Active
        {
            get => _active;
            set
            {
                if (value) Activate();
                else Deactivate();

            }
        }
        [XmlIgnore]
        public bool CanEditPollRate
        {
            get => !_active;
        }

        [XmlIgnore]
        public GrpcChannel Channel;// = GrpcChannel.ForAddress("127.0.0.1:50051");
        [XmlAttribute]
        public int MaxReconnectAttempts = 5;
        [XmlAttribute]
        public float ReconnectDelay = 5;
        private int CurrentReconnectAttempts;


        private CancellationTokenSource cancelTokenSource;
        private Dictionary<uint, TrackDatum> UpdatedData = new Dictionary<uint, TrackDatum>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged (string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public DataSource () { }

        public DataSource(string address = "127.0.0.1", string port = "50051")
        {
            Address = address;
            Port = port;
            Channel = GrpcChannel.ForAddress($"http://{Address}:{Port}");
        }

        public void Activate ()
        {
            if (Active) return;

            _active = true;
            OnPropertyChanged("Active");
            OnPropertyChanged("CanEditPollRate");

            Debug.WriteLine($"Activating DataSource {Name}");
            if (Channel == null) Channel = GrpcChannel.ForAddress($"http://{Address}:{Port}");
            cancelTokenSource = new CancellationTokenSource();

            Task t = Task.Run(TryStream, cancelTokenSource.Token); //DONE: add reconnect attempts
        }

        public async Task TryStream ()
        {
            CurrentReconnectAttempts = 1;
            while(Active)
            {
                try
                {
                    var HookClient = new HookService.HookServiceClient(Channel);
                    var missionName = HookClient.GetMissionName(new GetMissionNameRequest { });
                    Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [DataSource \"{Name}\"]: Connected to {Address}:{Port}! Mission: {missionName?.Name}");

                    CurrentReconnectAttempts = 1;
                    await StreamData();
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [DataSource \"{Name}\"]: Failed to connect to {Address}:{Port}! Attempt {CurrentReconnectAttempts}/{MaxReconnectAttempts} Error: {e.Message}");
                    if (++CurrentReconnectAttempts > MaxReconnectAttempts)
                    {
                        Deactivate();
                        return;
                    }
                    Thread.Sleep((int)(1000 * ReconnectDelay));
                }
            }
        }

        public async Task StreamData ()
        {
            try
            {
                var MissionClient = new MissionService.MissionServiceClient(Channel);
                var NetClient = new NetService.NetServiceClient(Channel);

                uint pr = uint.Parse(PollRate);
                uint spr = uint.Parse(SlowPollrate);

                var units = MissionClient.StreamUnits(new StreamUnitsRequest { PollRate = pr, MaxBackoff = spr });

                while (await units.ResponseStream.MoveNext(cancelTokenSource.Token))
                {
                    var unit = units.ResponseStream.Current;

                    if (unit.Unit != null)
                    {
                        UpdatedData[unit.Unit.Id] = ConvertFromDCSTrack(unit.Unit);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [DataSource \"{Name}\"]: failed to get data from {Address}:{Port}: {e.Message}\n\t{e.StackTrace}");
                //Deactivate();
            }
        }

        private TrackDatum ConvertFromDCSTrack (Unit unit)
        {
            var position = unit.Position;

            LatLonCoord positLL = new LatLonCoord { Lat_Degrees = position.Lat, Lon_Degrees = position.Lon, Alt = 0 };
            Vector64 posXYZ = MathL.Conversions.LLToXYZ(positLL, MathL.Conversions.EarthRadius);
            //Debug.WriteLine($"Data for {unit.Callsign}: {unit.Speed}");

            double speed = unit.Speed;
            double heading = unit.Heading * MathL.Conversions.ToRadians;

            TrackCategory cat = unit.Category switch
            {
                GroupCategory.Airplane or GroupCategory.Helicopter => TrackCategory.Air,
                GroupCategory.Ground or GroupCategory.Train => TrackCategory.Ground,
                GroupCategory.Ship => TrackCategory.Ship,
                _ => TrackCategory.None,
            };

            Vector64 vel = MathL.Conversions.GetTangentVelocity(positLL, heading, speed);

            string CoalitionToString = unit.Coalition switch
            {
                Coalition.Neutral => "Neutral",
                Coalition.Red => "Red",
                Coalition.Blue => "Blue",
                _ => "Unknown",
            };

            return new TrackDatum
            {
                ID = new TrackNumber.External { Value = (short)(unit.Id + TNRange.TNMin) },
                Position = posXYZ,
                Velocity = vel,
                Timestamp = DateTime.Now,
                Category = cat,
                Origin = this,
                Altitude = unit.Position.Alt,
                Heading = unit.Heading,
                ExtraData = new string[] {$"Coalition:{CoalitionToString}", $"Type:{unit.Type}", $"Callsign:{unit.Callsign}"}
            };
        }

        public Dictionary<uint, TrackDatum> PullData(bool clearQueue = true)
        {
            if(UpdatedData.Count > 0)
            {
                lock(UpdatedData)
                {
                    Dictionary<uint, TrackDatum> latest = new Dictionary<uint, TrackDatum>(UpdatedData);

                    if (clearQueue)
                        UpdatedData.Clear();

                    return latest;
                }
            }

            return new Dictionary<uint, TrackDatum>(0);
        }

        public void Deactivate ()
        {
            if (!Active) return;

            //Debug.WriteLine($"Deactivating DataSource {Name}");

            _active = false;
            OnPropertyChanged("Active");
            OnPropertyChanged("CanEditPollRate");

            cancelTokenSource.Cancel();
            Debug.WriteLine($"Deactivated DataSource {Name}");
        }
    }
}
