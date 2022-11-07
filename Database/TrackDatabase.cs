using System;
using System.Collections.Generic;
using System.Text;
using loki_bms_csharp.MathL;

namespace loki_bms_csharp.Database
{
    public static class TrackDatabase
    {
        public static short NextITN
        {
            get { return _itn++; }
        }
        private static short _itn = 1;

        public static Dictionary<TrackNumber, TrackFile> LiveTracks;
        public static List<TrackDatum> RawData;

        public static Dictionary<FriendFoeStatus, SkiaSharp.SKPaint> ColorByFFS =
            new Dictionary<FriendFoeStatus, SkiaSharp.SKPaint>()
            {
                {FriendFoeStatus.KnownFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Blue } },
                {FriendFoeStatus.AssumedFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Green } },
                {FriendFoeStatus.Neutral, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Purple }  },
                {FriendFoeStatus.Suspect, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Orange }  },
                {FriendFoeStatus.Hostile, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Red }  },
                {FriendFoeStatus.Unknown, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Yellow }  },
                {FriendFoeStatus.Pending, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Gray }  },
            };
        public static SkiaSharp.SKPaint DatumBrush = new SkiaSharp.SKPaint { Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.DarkOrange };

        private static System.Timers.Timer UpdateClock;
        private static DateTime LastUpdate;
        private static float MaxDatumAge = 30;
        
        public static void Initialize (float tickRate = 100)
        {
            LiveTracks = new Dictionary<TrackNumber, TrackFile>();
            RawData = new List<TrackDatum>();

            LastUpdate = DateTime.Now;

            UpdateClock = new System.Timers.Timer(tickRate);
            UpdateClock.Elapsed += delegate (Object sender, System.Timers.ElapsedEventArgs args)
            {
                DateTime now = args.SignalTime;
                float dt = (float)(now - LastUpdate).TotalSeconds;
                LastUpdate = now;

                try
                {
                    
                    UpdateTracks(dt);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"{DateTime.UtcNow:h:mm:ss.fff} Missed a Database tick! Exception: {e.Message}");
                }
            };
            UpdateClock.Start();
        }

        public static TrackFile InitiateTrack (LatLonCoord latLon, double heading = 0, double speed = 0, double vertSpeed = 0, TrackType trackType = TrackType.Sim)
        {
            Vector64 posit = Conversions.LLToXYZ(latLon, Conversions.EarthRadius);
            Vector64 vel = Conversions.GetTangentVelocity(latLon, heading, speed, vertSpeed);

            TrackFile newTrack = new TrackFile(
                posit,
                vel,
                new IFFData[0],
                type: trackType);

            TrackNumber newTN = new TrackNumber(intl: NextITN);

            LiveTracks.Add(newTN, newTrack);

            return newTrack;
        }

        public static void UpdateTracks (float dt)
        {
            DateTime now = DateTime.Now;

            foreach(var src in ProgramData.DataSources)
            {
                if(src.Active)
                {
                    RawData.AddRange(src.PullData().Values);
                }
            }

            //System.Diagnostics.Debug.WriteLine($"Updating Tracks with dt={dt}");
            foreach(var track in LiveTracks.Values)
            {
                lock(track) track.UpdateVisual(dt);
            }

            List<TrackDatum> oldData = new List<TrackDatum>();

            for (int i = 0; i < RawData.Count; i++)
            {
                var datum = RawData[i];
                if((now - datum.Timestamp).TotalSeconds > MaxDatumAge)
                {
                    oldData.Add(datum);
                }
            }

            foreach(var old in oldData)
            {
                RawData.Remove(old);
            }

            ProgramData.MainWindow.Redraw();
        }
    }
}
