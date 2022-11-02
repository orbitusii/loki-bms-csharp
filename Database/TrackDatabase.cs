using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.Database
{
    public static class TrackDatabase
    {
        public static short NextITN = 1;
        public static Dictionary<TrackNumber, TrackFile> LiveTracks;
        public static List<TrackDatum> UncorrelatedData;
        
        public static void Initialize ()
        {
            LiveTracks = new Dictionary<TrackNumber, TrackFile>();
        }

        public static void InitiateTrack (double Lat = 0, double Lon = 0, double alt = 0, float heading = 0, float speed = 0, TrackType trackType = TrackType.Sim)
        {


            TrackFile newTrack = new TrackFile
            {
                FFS = FriendFoeStatus.Pending,

            };
        }
    }
}
