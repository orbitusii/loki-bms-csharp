using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using loki_bms_common.MathL;

namespace loki_bms_common.Database
{
    public class TrackDatabase
    {
        public short NextITN
        {
            get { return _itn++; }
        }
        private short _itn = 1;

        public ObservableCollection<LokiDataSource> DataSources { get; set; } = new ObservableCollection<LokiDataSource>();

        public ObservableCollection<TrackFile> LiveTracks = new ObservableCollection<TrackFile>();
        public List<TrackDatum> ProcessedData = new List<TrackDatum>();
        public List<TrackDatum> FreshData = new List<TrackDatum>();

        public ObservableCollection<TacticalElement> TEs = new ObservableCollection<TacticalElement>();

        private System.Timers.Timer UpdateClock;
        private DateTime LastUpdate;
        private float MaxDatumAge = 30;

        public delegate void DatabaseUpdatedCallback();
        public DatabaseUpdatedCallback? OnDatabaseUpdated;

        public TrackDatabase (float tickRate = 100)
        {
            LiveTracks = new ObservableCollection<TrackFile>();
            ProcessedData = new List<TrackDatum>();
            FreshData = new List<TrackDatum>();

            LastUpdate = DateTime.Now;

            UpdateClock = new System.Timers.Timer(tickRate);
            UpdateClock.Elapsed += delegate (object? sender, System.Timers.ElapsedEventArgs args)
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

                    OnDatabaseUpdated?.Invoke();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"[DATABASE][ERROR][{DateTime.UtcNow:h:mm:ss.fff}] Missed a Database tick! Exception: {e.Message}\n\t{e.StackTrace}");
                }
            };
            UpdateClock.Start();
        }

        public TrackFile InitiateTrack (LatLonCoord latLon, double heading = 0, double speed = 0, double vertSpeed = 0, TrackType trackType = TrackType.Sim)
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

        public TrackFile InitiateTrack (Vector64 position, Vector64 velocity, TrackType trackType = TrackType.Sim)
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

        public void PullNewData ()
        {
            foreach (var src in DataSources)
            {
                if (src.Active)
                {
                    FreshData.AddRange(src.GetFreshData());
                }
            }
        }

        public void UpdateTracks (float dt)
        {
            for (int i = 0; i < FreshData.Count; i++)
            {
                var datum = FreshData[i];

                if (!Correlate_ByETN(datum))
                {
                    var newTrack = InitiateTrack(datum.Position, datum.Velocity, TrackType.External);
                    newTrack.TrackNumbers.Add(datum.ID);
                    newTrack.Category = datum.Category;
                    newTrack.Heading = datum.Heading_Deg;
                    newTrack.Altitude = datum.Altitude;

                    foreach (object exd in datum.ExtraData)
                    {
                        string[] exdString = ((string)exd).Split(':', StringSplitOptions.TrimEntries);

                        switch (exdString[0])
                        {
                            case "Type":
                                string trimmed = exdString[1].Replace("-", "");

                                newTrack.SpecType = trimmed;

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
                TimeSpan age = DateTime.Now - track.Timestamp;

                // Track Drop logic
                // If a track is older than 20 seconds, set it to pending
                // If a track is pending and older than 30 seconds, drop it
                // This isn't complete logic, but it'll work for now.
                if (age > TimeSpan.FromSeconds(30) && track.FFS == FriendFoeStatus.Pending)
                {
                    LiveTracks.Remove(track);
                }
                else if (age > TimeSpan.FromSeconds(20))
                {
                    track.FFS = FriendFoeStatus.Pending;
                }
            }
        }

        private bool Correlate_ByETN (TrackDatum datum)
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

        public void PurgeOldData()
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
