using System;
using System.Collections.Generic;
using System.Linq;

namespace loki_bms_csharp.Database
{
    public class TrackFile : IKinematicData
    {
        public List<TrackNumber> TrackNumbers { get; set; }
        public Vector64 RawPosition { get; private set; }
        public Vector64 Position { get; set; }
        public Vector64 Velocity { get; private set; }

        public IFFData[] IFFCodes { get; private set; }
        public IFFType IFFTypes { get; private set; }

        public DateTime Timestamp { get; private set; }
        public TrackType TrackType { get; private set; }

        // TrackFile Amplifying Data
        public FriendFoeStatus FFS;

        public string Callsign;

        public int SpecType;

        public bool ShowHistory;
        public List<Vector64> History { get; private set; }
        private DateTime NewestHistory;
        private DateTime OldestHistory;

        public TrackFile(Vector64 pos, Vector64 vel, IFFData[] codes, FriendFoeStatus _ffs = FriendFoeStatus.Pending, TrackType type = TrackType.Sim, string vcs = "", int spec = 0)
        {
            RawPosition = pos;
            Position = pos;
            Velocity = vel;
            IFFCodes = codes;
            IFFTypes = 0;
            TrackType = type;
            FFS = _ffs;

            Timestamp = DateTime.UtcNow;

            Callsign = vcs;
            SpecType = spec;

            ShowHistory = false;
            History = new List<Vector64>(0);
            NewestHistory = Timestamp;
            OldestHistory = Timestamp;
        }

        public void AddNewData (IKinematicData data, IFFData[] codes)
        {
            RawPosition = data.Position;
            Position = data.Position;

            Velocity = data.Velocity;

            IFFTypes = IFFType.None;
            IFFCodes = codes;
            foreach (var code in IFFCodes)
            {
                IFFTypes = IFFTypes & code.Type;
            }

            Timestamp = data.Timestamp;
        }

        public void UpdateVisual (float dt)
        {
            //Velocity -= 9.80665 * Position.normalized * dt;

            //System.Diagnostics.Debug.WriteLine($"Moving a Track from {Position} to {Position + Velocity * dt}");

            //Position = Position + Velocity * dt;

            ConformalMove(dt);

            if(DateTime.UtcNow - NewestHistory > TimeSpan.FromSeconds(10))
            {
                History.Add(Position);
                NewestHistory = DateTime.UtcNow;
            }
            if(DateTime.UtcNow - OldestHistory > TimeSpan.FromSeconds(60) || History.Count > 6)
            {
                History.RemoveAt(0);
                OldestHistory += TimeSpan.FromSeconds(10);
            }
        }

        public void ConformalMove (float dt)
        {
            Vector64 outAxis = Position.normalized;
            Vector64 forwardAxis = Velocity.normalized;

            double arcLength = Velocity.magnitude * dt;
            double angle = arcLength / Position.magnitude;

            double sine = Math.Sin(angle);
            double cosine = Math.Cos(angle);

            Position += Position.magnitude * (forwardAxis * sine
                - outAxis * (1 - cosine));

            Velocity += arcLength * (-(1 - cosine) * forwardAxis - sine * outAxis);
        }
    }
}
