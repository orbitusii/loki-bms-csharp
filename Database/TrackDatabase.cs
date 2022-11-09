using System;
using System.Collections.Generic;
using System.Linq;
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

        public static List<TrackFile> LiveTracks;
        public static List<TrackDatum> ProcessedData;
        public static List<TrackDatum> FreshData;

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
            LiveTracks = new List<TrackFile>();
            ProcessedData = new List<TrackDatum>();
            FreshData = new List<TrackDatum>();

            LastUpdate = DateTime.Now;

            UpdateClock = new System.Timers.Timer(tickRate);
            UpdateClock.Elapsed += delegate (Object sender, System.Timers.ElapsedEventArgs args)
            {
                DateTime now = args.SignalTime;
                float dt = (float)(now - LastUpdate).TotalSeconds;
                LastUpdate = now;

                try
                {
                    lock (FreshData)
                    {
                        PullNewData();

                        lock (LiveTracks)
                        {
                            UpdateTracks(dt);
                        }

                        ProcessedData.AddRange(FreshData);
                        FreshData.Clear();

                        lock (ProcessedData)
                        {
                            PurgeOldData();
                        }
                    }

                    ProgramData.MainWindow.Redraw();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"{DateTime.UtcNow:h:mm:ss.fff} Missed a Database tick! Exception: {e.Message}\n\t{e.StackTrace}");
                }
            };
            UpdateClock.Start();
        }

        public static TrackFile InitiateTrack (LatLonCoord latLon, double heading = 0, double speed = 0, double vertSpeed = 0, TrackType trackType = TrackType.Sim)
        {
            Vector64 posit = Conversions.LLToXYZ(latLon, Conversions.EarthRadius);
            Vector64 vel = Conversions.GetTangentVelocity(latLon, heading, speed, vertSpeed);

            var newTN = new TrackNumber.Internal { Value = NextITN };

            TrackFile newTrack = new TrackFile(
                newTN,
                posit,
                vel,
                new IFFData[0],
                type: trackType);


            LiveTracks.Add(newTrack);

            return newTrack;
        }

        public static TrackFile InitiateTrack (Vector64 position, Vector64 velocity, TrackType trackType = TrackType.Sim)
        {
            var newTN = new TrackNumber.Internal { Value = NextITN };

            TrackFile newTrack = new TrackFile(
                newTN,
                position,
                velocity,
                new IFFData[0],
                type: trackType);


            LiveTracks.Add(newTrack);

            return newTrack;
        }

        public static void PullNewData ()
        {
            foreach (var src in ProgramData.DataSources)
            {
                if (src.Active)
                {
                    FreshData.AddRange(src.PullData().Values);
                }
            }
        }

        public static void UpdateTracks (float dt)
        {
            for (int i = 0; i < FreshData.Count; i++)
            {
                var datum = FreshData[i];

                if (!Correlate_ByETN(datum))
                {
                    var newTrack = InitiateTrack(datum.Position, datum.Velocity, TrackType.External);
                    newTrack.TrackNumbers.Add(datum.ID);
                    newTrack.Category = datum.Category;
                }
            }
            
            //System.Diagnostics.Debug.WriteLine($"Updating Tracks with dt={dt}");
            foreach (var track in LiveTracks)
            {
                track.UpdateVisual(dt);
            }
        }

        private static bool Correlate_ByETN (TrackDatum datum)
        {
            var query = from TrackFile track in LiveTracks where track.TrackNumbers.Contains(datum.ID) select track;
            try
            {
                if (query.ToArray().Length > 0)
                {
                    var existingTrack = query.ToArray()[0];

                    if (existingTrack != null)
                    {
                        existingTrack.AddNewData(datum, new IFFData[0]);

                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public static void PurgeOldData()
        {
            DateTime now = DateTime.Now;

            List<TrackDatum> oldData = new List<TrackDatum>();

            for (int i = 0; i < ProcessedData.Count; i++)
            {
                if ((now - ProcessedData[i].Timestamp).TotalSeconds > MaxDatumAge)
                {
                    oldData.Add(ProcessedData[i]);
                }
            }

            foreach (var old in oldData)
            {
                ProcessedData.Remove(old);
            }
        }
    }
}
