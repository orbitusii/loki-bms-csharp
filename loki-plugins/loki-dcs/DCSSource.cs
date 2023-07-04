using loki_bms_common;
using loki_bms_common.Database;
using System.Xml.Serialization;
using RurouniJones.Dcs.Grpc.V0;
using RurouniJones.Dcs.Grpc.V0.Mission;
using Grpc.Net.Client;
using RurouniJones.Dcs.Grpc.V0.Hook;
using RurouniJones.Dcs.Grpc.V0.Net;
using RurouniJones.Dcs.Grpc.V0.World;
using System.Diagnostics.CodeAnalysis;
using RurouniJones.Dcs.Grpc.V0.Common;
using loki_bms_common.MathL;
using System.IO.Compression;
using System.Diagnostics;
using Grpc.Core;

namespace loki_dcs
{
    [XmlInclude(typeof(DCSSource))]
    public class DCSSource : LokiDataSource
    {
        internal GrpcChannel Channel => GrpcChannel.ForAddress($"http://{Address}:{Port}");

        internal HookService.HookServiceClient Hook;
        internal MissionService.MissionServiceClient Mission;
        internal NetService.NetServiceClient Net;
        internal WorldService.WorldServiceClient World;

        private CancellationTokenSource StopTokenSource = new CancellationTokenSource();

        private Dictionary<uint, TrackDatum?> FreshData = new Dictionary<uint, TrackDatum?>();

        public override string SourceInfo =>
            $"{GetType().Name} at {Address}:{Port}\n" +
            $"Mission Name: {MissionName}\n" +
            $"        Time: {MissionTime}\n";

        private string MissionName = "Unknown";
        private string MissionTime = "Unknown";

        public override SerializedDataSource GetSerializable()
        {
            var sds = base.GetSerializable();
            sds.Extradata = new string[] { $"Extra Data Test from DCSSource {Name}" };

            return sds;
        }

        public override void LoadSerializable(SerializedDataSource sds)
        {
            base.LoadSerializable(sds);

            Hook = new HookService.HookServiceClient(Channel);
            Mission = new MissionService.MissionServiceClient(Channel);
            Net = new NetService.NetServiceClient(Channel);
            World = new WorldService.WorldServiceClient(Channel);
        }

        public override void Activate()
        {
            Debug.WriteLine("[DCS-grpc][LOG] Trying to start the DCS Source");
            Status = SourceStatus.Starting;

            Task.Run(TryActivateAsync);
        }

        public async void TryActivateAsync ()
        {
            if (!CheckAlive())
            {
                Active = false;
                Status = SourceStatus.Disconnected;
                return;
            }

            StopTokenSource = new CancellationTokenSource();

            Active = true;
            Status = SourceStatus.Active;
            Debug.WriteLine($"[DCS-grpc][LOG] Success! Connected to {Channel.Target}");

            await StreamData();
        }

        public async Task StreamData()
        {
            try
            {
                uint pr = uint.Parse(PollRate);
                uint spr = uint.Parse(SlowPollrate);

                var units = Mission.StreamUnits(new StreamUnitsRequest { PollRate = pr, MaxBackoff = spr });

                while (Active && await units.ResponseStream.MoveNext(StopTokenSource.Token))
                {
                    var unit = units.ResponseStream.Current;

                    if (unit.Unit is Unit)
                    {
                        FreshData[unit.Unit.Id] = ConvertData(unit.Unit);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(
                    $"[DCS-grpc][ERROR] Something broke the stream?! StopToken Cancelled? {StopTokenSource.IsCancellationRequested}\n" +
                    $"                  Inner exception: {e}");
                Active = false;
                Status = SourceStatus.Disconnected;
            }
        }

        private TrackDatum ConvertData(Unit unit)
        {
            var positRaw = unit.Position;

            LatLonCoord positLL = new LatLonCoord { Lat_Degrees = positRaw.Lat, Lon_Degrees = positRaw.Lon, Alt = 0 };
            Vector64 positCart = Conversions.LLToXYZ(positLL, Conversions.EarthRadius);

            double speed = unit.Velocity.Speed;
            double hdg_rads = unit.Velocity.Heading * Conversions.ToRadians;
            Vector64 vel = Conversions.GetTangentVelocity(positLL, hdg_rads, speed);

            TrackCategory categ = unit.Group.Category switch
            {
                GroupCategory.Airplane or GroupCategory.Helicopter => TrackCategory.Air,
                GroupCategory.Ground or GroupCategory.Train => TrackCategory.Ground,
                GroupCategory.Ship => TrackCategory.Ship,
                _ => TrackCategory.None,
            };

            string CoalitionToString = unit.Coalition switch
            {
                Coalition.Neutral => "Neutral",
                Coalition.Red => "Red",
                Coalition.Blue => "Blue",
                _ => "Unknown",
            };

            TrackNumber TN = new TrackNumber.External { Value = (short)(unit.Id + TNRange.TNMin) };

            return new TrackDatum(TN, this, positCart, vel)
            {
                Category = categ,
                Altitude = unit.Position.Alt,
                Heading_Rads = hdg_rads,
                ExtraData = new string[] { $"Coalition:{CoalitionToString}", $"Type:{unit.Type}", $"Callsign:{unit.Callsign}" }
            };
        }

        public override bool CheckAlive()
        {
            try
            {
                var response = Hook.GetMissionName(new GetMissionNameRequest { });
                MissionName = response.Name;
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[DCS-grpc][LOG] Source at {Address}:{Port} wasn't reachable: {e}");
                return false;
            }
        }

        public override void Deactivate()
        {
            if (Status != SourceStatus.Active) return;

            Active = false;
            Status = SourceStatus.Offline;
        }

        public override TrackDatum[] GetFreshData()
        {
            // Suppresses this null coalescing complaint because it's going to get filtered
#pragma warning disable CS8619
            TrackDatum[] values = FreshData.Values.Where(x => x is TrackDatum).ToArray();
            Debug.WriteLine($"{values.Length} fresh TrackData");
#pragma warning restore CS8619

            // Purge the data but keep space allocated
            foreach(uint key in FreshData.Keys)
            {
                FreshData[key] = null;
            }

            MissionTime = Mission.GetScenarioCurrentTime(new GetScenarioCurrentTimeRequest { }).Datetime;

            return values;
        }

        public override TacticalElement[] GetTEs()
        {
            return base.GetTEs();
        }
    }
}
