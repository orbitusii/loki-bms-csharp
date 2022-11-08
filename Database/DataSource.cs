﻿using Grpc.Net.Client;
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
        [XmlAttribute]
        public string Name { get; set; } = "New Data Source";
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
        
        [XmlElement]
        public string DataSymbol { get; set; }
        [XmlAttribute("Color")]
        public string DataColor { get; set; }
        public SkiaSharp.SKColor GetSKColor
        {
            get => SkiaSharp.SKColor.TryParse(DataColor, out var color) ? color : SkiaSharp.SKColors.White;
        }

        // TODO: make this value reflect properly on SourcesWindow when it fails to connect
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
        private Dictionary<string, TrackDatum> UpdatedData = new Dictionary<string, TrackDatum>();

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

            Task t = Task.Run(TryStream, cancelTokenSource.Token); //TODO: add reconnect attempts
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
                        UpdatedData[unit.Unit.Callsign] = ConvertFromDCSTrack(unit.Unit);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [DataSource \"{Name}\"]: failed to get data from {Address}:{Port}: {e.Message}\n\t{e.StackTrace}");
                //Deactivate();
            }
        }

        private TrackDatum ConvertFromDCSTrack (RurouniJones.Dcs.Grpc.V0.Common.Unit unit)
        {
            var position = unit.Position;

            LatLonCoord positLL = new LatLonCoord { Lat_Degrees = position.Lat, Lon_Degrees = position.Lon };
            Vector64 posXYZ = MathL.Conversions.LLToXYZ(positLL, MathL.Conversions.EarthRadius);
            //Debug.WriteLine($"Data for {unit.Callsign}: {unit.Speed}");

            double speed = unit.Speed;
            double heading = unit.Heading * MathL.Conversions.ToRadians;

            Vector64 vel = MathL.Conversions.GetTangentVelocity(positLL, heading, speed);

            return new TrackDatum { ID = new TrackNumber.External { Value = (short)(unit.Id + TNRange.TNMin) }, Position = posXYZ, Velocity = vel, Timestamp = DateTime.Now };
        }

        public Dictionary<string, TrackDatum> PullData(bool clearQueue = true)
        {
            if(UpdatedData.Count > 0)
            {
                lock(UpdatedData)
                {
                    Dictionary<string, TrackDatum> latest = new Dictionary<string, TrackDatum>(UpdatedData);

                    if (clearQueue)
                        UpdatedData.Clear();

                    return latest;
                }
            }

            return new Dictionary<string, TrackDatum>(0);
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
