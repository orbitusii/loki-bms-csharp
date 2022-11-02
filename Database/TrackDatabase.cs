﻿using System;
using System.Collections.Generic;
using System.Text;
using loki_bms_csharp.MathL;

namespace loki_bms_csharp.Database
{
    public static class TrackDatabase
    {
        public static short NextITN = 1;
        public static Dictionary<TrackNumber, TrackFile> LiveTracks;
        public static List<TrackDatum> UncorrelatedData;

        public static Dictionary<FriendFoeStatus, SkiaSharp.SKPaint> ColorByFFS =
            new Dictionary<FriendFoeStatus, SkiaSharp.SKPaint>()
            {
                {FriendFoeStatus.KnownFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.StrokeAndFill, Color = SkiaSharp.SKColors.Blue } },
                {FriendFoeStatus.AssumedFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.StrokeAndFill, Color = SkiaSharp.SKColors.Green } },
                {FriendFoeStatus.Neutral, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.StrokeAndFill, Color = SkiaSharp.SKColors.Purple }  },
                {FriendFoeStatus.Suspect, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.StrokeAndFill, Color = SkiaSharp.SKColors.Orange }  },
                {FriendFoeStatus.Hostile, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.StrokeAndFill, Color = SkiaSharp.SKColors.Red }  },
                {FriendFoeStatus.Unknown, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.StrokeAndFill, Color = SkiaSharp.SKColors.Yellow }  },
                {FriendFoeStatus.Pending, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.StrokeAndFill, Color = SkiaSharp.SKColors.Gray }  },
            };

        private static System.Timers.Timer UpdateClock;
        private static DateTime LastUpdate;
        
        public static void Initialize (float tickRate = 100)
        {
            LiveTracks = new Dictionary<TrackNumber, TrackFile>();
            UncorrelatedData = new List<TrackDatum>();

            UpdateClock = new System.Timers.Timer(tickRate);
            UpdateClock.Elapsed += delegate (Object sender, System.Timers.ElapsedEventArgs args)
            {
                try
                {
                    DateTime now = DateTime.UtcNow;
                    float dt = (float)(now - LastUpdate).TotalSeconds;

                    UpdateTracks(dt);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"{DateTime.UtcNow:h:mm:ss.fff} Missed a Database tick! Exception: {e.Message}");
                }
            };
        }

        public static void InitiateTrack (LatLonCoord latLon, double heading = 0, double speed = 0, double vertSpeed = 0, TrackType trackType = TrackType.Sim)
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
        }

        public static void UpdateTracks (float dt)
        {
            foreach(var track in LiveTracks.Values)
            {
                track.UpdateVisual(dt);
            }
        }
    }
}
