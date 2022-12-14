using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
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

        public static ObservableCollection<TrackFile> LiveTracks;
        public static List<TrackDatum> ProcessedData;
        public static List<TrackDatum> FreshData;

        public static Dictionary<FriendFoeStatus, SkiaSharp.SKPaint> StrokeByFFS =
            new Dictionary<FriendFoeStatus, SkiaSharp.SKPaint>()
            {
                {FriendFoeStatus.KnownFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Blue, StrokeWidth = 2 } },
                {FriendFoeStatus.AssumedFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Green, StrokeWidth = 2 } },
                {FriendFoeStatus.Neutral, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Purple, StrokeWidth = 2 }  },
                {FriendFoeStatus.Suspect, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Orange, StrokeWidth = 2 }  },
                {FriendFoeStatus.Hostile, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Red, StrokeWidth = 2 }  },
                {FriendFoeStatus.Unknown, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Yellow, StrokeWidth = 2 }  },
                {FriendFoeStatus.Pending, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Gray, StrokeWidth = 2 }  },
            };
        public static Dictionary<FriendFoeStatus, SkiaSharp.SKPaint> FillByFFS =
            new Dictionary<FriendFoeStatus, SkiaSharp.SKPaint>()
            {
                {FriendFoeStatus.KnownFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Blue.WithAlpha(128) } },
                {FriendFoeStatus.AssumedFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Green.WithAlpha(128) } },
                {FriendFoeStatus.Neutral, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Purple.WithAlpha(128) }  },
                {FriendFoeStatus.Suspect, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Orange.WithAlpha(128) }  },
                {FriendFoeStatus.Hostile, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Red.WithAlpha(128) }  },
                {FriendFoeStatus.Unknown, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Yellow.WithAlpha(128) }  },
                {FriendFoeStatus.Pending, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Gray.WithAlpha(128) }  },
            };
        private static System.Timers.Timer UpdateClock;
        private static DateTime LastUpdate;
        private static float MaxDatumAge = 30;
        
        public static void Initialize (float tickRate = 100)
        {
            LiveTracks = new ObservableCollection<TrackFile>();
            ProcessedData = new List<TrackDatum>();
            FreshData = new List<TrackDatum>();

            LastUpdate = DateTime.Now;

            UpdateClock = new System.Timers.Timer(tickRate);
            UpdateClock.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs args)
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
                    newTrack.Heading = datum.Heading;
                    newTrack.Altitude = datum.Altitude;

                    foreach (object exd in datum.ExtraData)
                    {
                        string[] exdString = ((string)exd).Split(':', StringSplitOptions.TrimEntries);

                        switch (exdString[0])
                        {
                            case "Type":
                                string trimmed = exdString[1].Replace("-", "");

                                var actual = ProgramData.SpecTypeSymbols.Keys.ToList().Find(x => trimmed.StartsWith(x) && x != "") ?? "";
                                newTrack.SpecType = actual;

                                break;
                            case "Coalition":
                                newTrack.FFS = exdString[1] switch
                                {
                                    "Red" => FriendFoeStatus.Suspect,
                                    "Blue" => FriendFoeStatus.AssumedFriend,
                                    "Neutral" => FriendFoeStatus.Neutral,
                                    _ => FriendFoeStatus.Pending,
                                };
                                break;
                            case "Callsign":
                                newTrack.Callsign = exdString[1];
                                break;
                            default:
                                break;
                        }
                    }
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
