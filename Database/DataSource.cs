using Grpc.Net.Client;
using RurouniJones.Dcs.Grpc.V0.Hook;
using RurouniJones.Dcs.Grpc.V0.Mission;
using RurouniJones.Dcs.Grpc.V0.Net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace loki_bms_csharp.Database
{
    public class DataSource
    {
        [XmlAttribute]
        public string Name = "New Data Source";
        [XmlAttribute]
        public string Address = "127.0.0.1";
        [XmlAttribute]
        public string Port = "50051";

        internal bool _active = false;
        public bool Active
        {
            get => _active;
            set
            {
                if (value) _ = Activate();
                else Deactivate();
            }
        }

        [XmlAttribute]
        public string ActiveByDefault
        {
            get => _active.ToString();
            set
            {
                _active = bool.TryParse(value, out bool v) ? v : false;
            }
        }

        [XmlIgnore]
        public GrpcChannel Channel;// = GrpcChannel.ForAddress("127.0.0.1:50051");
        [XmlIgnore]
        private CancellationTokenSource cancelTokenSource;
        [XmlIgnore]
        public Dictionary<string, TrackDatum> UpdatedData;

        public DataSource () { }

        public DataSource(string address = "127.0.0.1", string port = "50051")
        {
            Address = address;
            Port = port;
            Channel = GrpcChannel.ForAddress($"http://{Address}:{Port}");
        }

        public async Task Activate ()
        {
            if (Active) return;

            _active = true;
            using (cancelTokenSource = new CancellationTokenSource())
            {
                try
                {
                    var MissionClient = new MissionService.MissionServiceClient(Channel);
                    var HookClient = new HookService.HookServiceClient(Channel);
                    var NetClient = new NetService.NetServiceClient(Channel);

                    //var missionName = HookClient.GetMissionName(new GetMissionNameRequest { });
                    var units = MissionClient.StreamUnits(new StreamUnitsRequest { PollRate = 10, MaxBackoff = 30 });

                    var missionName = HookClient.GetMissionName(new GetMissionNameRequest { });

                    System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [DataSource]: checking mission name: {missionName?.Name}\n");

                    //NetClient.SendChat(new SendChatRequest { Coalition = RurouniJones.Dcs.Grpc.V0.Common.Coalition.All, Message = "A LOKI BMS instance is now watching your server" });

                    while (await units.ResponseStream.MoveNext(cancelTokenSource.Token))
                    {
                        var unit = units.ResponseStream.Current;

                        System.Diagnostics.Debug.WriteLine($"\tUnit Name = {unit.Unit.Callsign}");
                        //if (unit.Unit != null) break;
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [DataSource]: failed to get data from {Address}:{Port}: {e.Message}");
                }
            }
        }

        public void Deactivate ()
        {
            if (!Active) return;

            _active = false;
            cancelTokenSource.Cancel();
            System.Diagnostics.Debug.WriteLine("Deactivated this data source");
        }
    }
}
