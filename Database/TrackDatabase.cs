using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.Database
{
    public static class TrackDatabase
    {
        public static Dictionary<TrackNumber, TrackFile> LiveTracks;
        
        public static void Initialize ()
        {
            LiveTracks = new Dictionary<TrackNumber, TrackFile>();
        }
    }
}
