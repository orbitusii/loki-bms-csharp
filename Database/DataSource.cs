using Grpc.Net.Client;
using RurouniJones.Dcs.Grpc.V0.Hook;
using RurouniJones.Dcs.Grpc.V0.Mission;
using RurouniJones.Dcs.Grpc.V0.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace loki_bms_csharp.Database
{
    public class DataSource
    {
        [XmlAttribute]
        public string Name { get; set; } = "New Data Source";
        [XmlAttribute]
        public string Address { get; set; } = "127.0.0.1";
        [XmlAttribute]
        public string Port { get; set; } = "50051";

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
        public GrpcChannel Channel;// = GrpcChannel.ForAddress("127.0.0.1:50051");

        private CancellationTokenSource cancelTokenSource;
        private Dictionary<string, TrackDatum> UpdatedData = new Dictionary<string, TrackDatum>();

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
            Debug.WriteLine($"Activating DataSource {Name}");
            if (Channel == null) Channel = GrpcChannel.ForAddress($"http://{Address}:{Port}");
            cancelTokenSource = new CancellationTokenSource();

            Task t = Task.Run(StreamData);
        }

        public async Task StreamData ()
        {
            try
            {
                var MissionClient = new MissionService.MissionServiceClient(Channel);
                var HookClient = new HookService.HookServiceClient(Channel);
                var NetClient = new NetService.NetServiceClient(Channel);

                var missionName = HookClient.GetMissionName(new GetMissionNameRequest { });

                var units = MissionClient.StreamUnits(new StreamUnitsRequest { PollRate = 10, MaxBackoff = 30 });

                Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [DataSource]: checking mission name: {missionName?.Name}");

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
                Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [DataSource]: failed to get data from {Address}:{Port}: {e.Message}\n\t{e.StackTrace}");
            }
        }

        private TrackDatum ConvertFromDCSTrack (RurouniJones.Dcs.Grpc.V0.Common.Unit unit)
        {
            var position = unit.Position;
            LatLonCoord positLL = new LatLonCoord { Lat_Degrees = position.Lat, Lon_Degrees = position.Lon };
            Vector64 posXYZ = MathL.Conversions.LLToXYZ(positLL, MathL.Conversions.EarthRadius);
            //Debug.WriteLine($"Data for {unit.Callsign}: {positLL} => {posXYZ}");

            return new TrackDatum { Position = posXYZ, Timestamp = DateTime.Now };
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
            cancelTokenSource.Cancel();
            Debug.WriteLine($"Deactivated DataSource {Name}");
        }
    }
}
