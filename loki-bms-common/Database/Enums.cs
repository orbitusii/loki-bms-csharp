using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_common.Database
{
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

    public enum TrackType
    {
        Sim = 0,
        Internal = 1,
        External = 2,
        PPLI = 3,
    }

    public enum TrackCategory
    {
        None,
        Air,
        Ground,
        Ship,
    }

    [Flags]
    public enum IFFType
    {
        None = 0,
        Mode1 = 1,
        Mode2 = 2,
        Mode3 = 4,
        Mode4 = 8,
        TADIL = 16,
    }

    public enum TEType
    {
        None,
        SAM,
        Airbase,
        Markpoint,
        Waypoint,
        Bullseye,
        DefendedPoint,
        DownedAircraft,
        Other,
    }
}
