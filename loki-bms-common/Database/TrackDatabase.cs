using loki_bms_common.MathL;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;

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
        private List<TrackFile> forceDropTracks = new List<TrackFile>();

        protected List<TrackDatum> ProcessedData = new List<TrackDatum>();
        protected List<TrackDatum> FreshData = new List<TrackDatum>();

        protected ObservableCollection<TacticalElement> TEs = new ObservableCollection<TacticalElement>();
        private List<TacticalElement> deletedTEs = new List<TacticalElement>();

        private System.Timers.Timer UpdateClock;
        private DateTime LastUpdate;
        private float MaxDatumAge = 30;

        public delegate void DatabaseUpdatedCallback();
        public DatabaseUpdatedCallback? OnDatabaseUpdated;

        public Logger? Log { get; private set; }

        public TrackDatabase(float tickRate = 1000, bool withLog = true)
        {
            if (withLog) Log = new Logger("Database", false);

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
                    Log?.LogError("Missed a database tick! The specific exception is visible if you set Log.MaxLevel to 'Debug'.", Logger.LogLevel.Normal);
                    Log?.LogException(e, Logger.LogLevel.Debug);
                }
            };
            UpdateClock.Start();
        }

        public TrackFile[] GetTracks (Func<TrackFile, bool>? predicate = null)
        {
            if (predicate is null) return LiveTracks.ToArray();
            else return LiveTracks.Where(predicate).ToArray();
        }

        public TrackDatum[] GetDataMarks() => ProcessedData.ToArray();
        public TacticalElement[] GetTacticalElements (Func<TacticalElement, bool>? predicate = null)
        {
            if (predicate is null) return TEs.ToArray();
            else return TEs.Where(predicate).ToArray();
        }

        public bool AddTE (TacticalElement newTE)
        {
            TEs.Add(newTE);
            return true;
        }

        public TrackFile InitiateTrack(LatLonCoord latLon, double heading = 0, double speed = 0, double vertSpeed = 0, TrackType trackType = TrackType.Sim)
        {
            Vector64 posit = Conversions.LLToXYZ(latLon, Conversions.EarthRadius);
            Vector64 vel = Conversions.GetTangentVelocity(latLon, heading, speed, vertSpeed);
            IFFData[] transponder = new IFFData[0];
            var newTN = new TrackNumber.Internal { Value = NextITN };

            TrackFile newTrack = new TrackFile(
                newTN,
                posit,
                vel,
                transponder,
                type: trackType);


            LiveTracks.Add(newTrack);

            Log?.LogMessageMultiline(Logger.LogLevel.Verbose,
                $"Initiated a new Track #{newTN}",
                $"Kinematics: Position {posit}; Velocity {vel}",
                $"Category: {trackType}; IFF: {transponder}");
            return newTrack;
        }

        public TrackFile InitiateTrack(Vector64 position, Vector64 velocity, TrackType trackType = TrackType.Sim)
        {
            var newTN = new TrackNumber.Internal { Value = NextITN };
            IFFData[] transponder = new IFFData[0];

            TrackFile newTrack = new TrackFile(
                newTN,
                position,
                velocity,
                transponder,
                type: trackType);


            LiveTracks.Add(newTrack);

            Log?.LogMessageMultiline(Logger.LogLevel.Verbose,
                $"Initiated a new Track #{newTN}",
                $"Kinematics: Position {position}; Velocity {velocity}",
                $"Category: {trackType}; IFF: {transponder}");
            return newTrack;
        }

        public void MarkForDeletion (ISelectableObject item)
        {
            if (item is TrackFile tf && LiveTracks.Contains(tf))
            {
                Log?.LogMessage($"Marked TrackFile {tf.TrackNumbers[0]} for deletion", Logger.LogLevel.Verbose);
                forceDropTracks.Add(tf);
            }
            else if (item is TacticalElement te && TEs.Contains(te))
            {
                Log?.LogMessage($"Marked TacticalElement {te.Name} for deletion", Logger.LogLevel.Verbose);
                deletedTEs.Add(te);
            }
        }

        public void PullNewData()
        {
            var ActiveSources = DataSources.Where(x => x.Status == LokiDataSource.SourceStatus.Active).ToList();

            foreach (var src in ActiveSources)
            {
                if (src.Active)
                {
                    FreshData.AddRange(src.GetFreshData());
                }
            }
        }

        public void UpdateTracks(float dt)
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
            }
        }

        private bool Correlate_ByETN(TrackDatum datum)
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

            lock(ProcessedData)
            {
                Log?.LogMessage($"Removing {oldData.Count} old data symbols", Logger.LogLevel.Verbose);
                foreach (var old in oldData)
                {
                    ProcessedData.Remove(old);
                }
            }

            lock(LiveTracks)
            {
                var OldTracks = LiveTracks.Where(x => DateTime.Now - x.Timestamp > TimeSpan.FromSeconds(30)).ToList();

                if(OldTracks.Count > 0 || forceDropTracks.Count > 0)
                    Log?.LogMessage($"Removing {OldTracks.Count} stale tracks and {forceDropTracks.Count} dropped tracks", Logger.LogLevel.Verbose);
                
                foreach (var track in LiveTracks)
                {
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

                foreach (var deleted in forceDropTracks)
                {
                    LiveTracks.Remove(deleted);
                }
            }

            lock(TEs)
            {
                if(deletedTEs.Count > 0)
                    Log?.LogMessage($"Removing {deletedTEs} deleted TEs", Logger.LogLevel.Verbose);

                foreach (var deleted in deletedTEs)
                {
                    TEs.Remove(deleted);
                }
            }

            forceDropTracks.Clear();
            deletedTEs.Clear();
        }
    }
}
