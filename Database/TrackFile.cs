using System;
using System.Collections.Generic;
using System.Linq;

namespace loki_bms_csharp.Database
{
    public struct TrackFile : IReturnData
    {
        public Vector64 RawPosition { get; private set; }
        public Vector64 Position { get; private set; }
        public Vector64 Velocity { get; private set; }

        public IFFData[] IFFCodes { get; private set; }
        public IFFData.IFFType IFFTypes { get; private set; }

        public DateTime Timestamp { get; private set; }
        public bool IsSimTrack

        // TrackFile Amplifying Data
        public FriendFoeStatus FFS;

        public string Callsign;

        public int SpecType;

        public List<Vector64> History { get; private set; }
        private DateTime NewestHistory;
        private DateTime OldestHistory;

        public void AddNewData (IReturnData data)
        {
            RawPosition = data.Position;
            Position = data.Position;

            Velocity = data.Velocity;

            IFFTypes = IFFData.IFFType.None;
            IFFCodes = data.IFFCodes;
            foreach (var code in IFFCodes)
            {
                IFFTypes = IFFTypes & code.Type;
            }

            Timestamp = data.Timestamp;
        }

        public void UpdateVisual (float dt)
        {
            Velocity -= 9.80665 * Position.normalized * dt;

            Position += Velocity * dt;

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
    }
}
