using Grpc.Net.Client;
using RurouniJones.Dcs.Grpc.V0.Hook;
using RurouniJones.Dcs.Grpc.V0.Mission;
using RurouniJones.Dcs.Grpc.V0.Net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace loki_bms_csharp.Database
{
    public class DataSource
    {
        public string Address;
        public int Port;
        public GrpcChannel Channel;// = GrpcChannel.ForAddress("127.0.0.1:50051");
        public MissionService.MissionServiceClient MissionClient;
        public HookService.HookServiceClient HookClient;
        private CancellationTokenSource cancelTokenSource;
        public Dictionary<string, TrackDatum> UpdatedData;

        public DataSource(string address = "127.0.0.1", int port = 50051)
        {
            Address = address;
            Port = port;
            Channel = GrpcChannel.ForAddress($"http://{Address}:{Port}");
            cancelTokenSource = new CancellationTokenSource();
        }

        public async Task Activate ()
        {
            try
            {
                MissionClient = new MissionService.MissionServiceClient(Channel);
                HookClient = new HookService.HookServiceClient(Channel);
                var NetClient = new NetService.NetServiceClient(Channel);

                //var missionName = HookClient.GetMissionName(new GetMissionNameRequest { });
                var units = MissionClient.StreamUnits(new StreamUnitsRequest { PollRate = 10, MaxBackoff = 30});

                var missionName = HookClient.GetMissionName(new GetMissionNameRequest { });

                System.Diagnostics.Debug.WriteLine($"{DateTime.Now:h:mm:ss:fff} [DataSource]: checking mission name: {missionName?.Name}\n");

                //NetClient.SendChat(new SendChatRequest { Coalition = RurouniJones.Dcs.Grpc.V0.Common.Coalition.All, Message = "A LOKI BMS instance is now watching your server" });

                while(await units.ResponseStream.MoveNext(cancelTokenSource.Token))
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

        public void Deactivate ()
        {
            cancelTokenSource.Cancel();
            System.Diagnostics.Debug.WriteLine("Deactivated this data source");
        }
    }
}
