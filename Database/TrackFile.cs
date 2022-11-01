using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.Database
{
    public struct TrackFile : IReturnData
    {
        public Vector64 Position { get; }
        public Vector64 Velocity { get; }

        public IFFData[] IFFCodes { get; }

        public DateTime Timestamp { get; }

        // TrackFile Amplifying Data
        public FriendFoeStatus FFS;

        public string Callsign;

        public int SpecType;

        public List<Vector64> History;
    }

    public enum FriendFoeStatus
    {
        KnownFriend,
        AssumedFriend,
        Neutral,
        Suspect,
        Hostile,
        Unknown,
        Pending,
    }
}
