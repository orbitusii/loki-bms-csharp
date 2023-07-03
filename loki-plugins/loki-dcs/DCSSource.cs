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

namespace loki_dcs
{
    [XmlInclude(typeof(DCSSource))]
    public class DCSSource : LokiDataSource
    {
        internal GrpcChannel Channel => GrpcChannel.ForAddress($"https://{Address}:{Port}");

        internal HookService.HookServiceClient Hook;
        internal MissionService.MissionServiceClient Mission;
        internal NetService.NetServiceClient Net;
        internal WorldService.WorldServiceClient World;

        private CancellationTokenSource StopTokenSource = new CancellationTokenSource(0);
        private CancellationToken StopToken;

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
            if (!CheckAlive())
            {
                Active = false;
                return;
            }

            if (!StopTokenSource.TryReset()) StopTokenSource = new CancellationTokenSource(0);
            StopToken = StopTokenSource.Token;

            Task.Run(StreamData, StopToken);
        }

        public async Task StreamData()
        {
            try
            {
                uint pr = uint.Parse(PollRate);
                uint spr = uint.Parse(SlowPollrate);

                var units = Mission.StreamUnits(new StreamUnitsRequest { PollRate = pr, MaxBackoff = spr });

                while (await units.ResponseStream.MoveNext(StopToken))
                {
                    if (StopToken.IsCancellationRequested) break;

                    var unit = units.ResponseStream.Current;

                    if (unit.Unit is null) continue;

                    FreshData[unit.Unit.Id] = ConvertData(unit.Unit);
                }
            }
            finally
            {
                Deactivate();
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
            return Task.Run(CheckAliveAsync).Result;
        }

        private async Task<bool> CheckAliveAsync()
        {
            try
            {
                var response = await Hook.GetMissionNameAsync(new GetMissionNameRequest { });
                MissionName = response.Name;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Deactivate()
        {
            StopTokenSource.Cancel();
        }

        public override TrackDatum[] GetFreshData()
        {
            // Suppresses this null coalescing complaint because it's going to get filtered
#pragma warning disable CS8619
            TrackDatum[] values = FreshData.Values.Where(x => x is TrackDatum).ToArray();
#pragma warning restore CS8619

            // Purge the data but keep space allocated
            foreach(uint key in FreshData.Keys)
            {
                FreshData[key] = null;
            }

            return values;
        }

        public override TacticalElement[] GetTEs()
        {
            return base.GetTEs();
        }
    }
}
