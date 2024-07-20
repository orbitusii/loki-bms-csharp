using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using loki_bms_common;

namespace loki_bms_common.Database
{
    public class TrackFile : ISelectableObject, INotifyPropertyChanged
    {
        public List<TrackNumber> TrackNumbers { get; set; }
        public Vector64 RawPosition { get; private set; }
        public Vector64 Position { get; set; }
        public LatLonCoord LatLon => MathL.Conversions.XYZToLL(Position);
        public Vector64 Velocity { get; private set; }

        public double Heading
        {
            get => Heading_Rads * MathL.Conversions.ToDegrees;
            set => Heading_Rads = value * MathL.Conversions.ToRadians;
        }
        public double Heading_Rads { get; set; }

        public double Altitude { get; set; }

        public IFFData[] IFFCodes { get; private set; }
        public IFFType IFFTypes { get; private set; }

        public DateTime Timestamp { get; private set; }
        public TrackStatus Status { get; private set; } = TrackStatus.Live;
        private bool freshData = false;
        public TrackType TrackType { get; private set; }
        public TrackCategory Category { get; set; } = TrackCategory.None;

        // TrackFile Amplifying Data
        public FriendFoeStatus FFS { get; set; }

        public string Callsign { get; set; }

        public string SpecType { get; set; }

        public bool ShowHistory { get; set; }
        public Queue<Vector64> History { get; private set; }
        private DateTime NewestHistory;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TrackFile () { }

        public TrackFile(TrackNumber.Internal tn, Vector64 pos, Vector64 vel, IFFData[] codes, FriendFoeStatus _ffs = FriendFoeStatus.Pending, TrackType type = TrackType.Sim, string vcs = "", string spec = "")
        {
            TrackNumbers = new List<TrackNumber>() { tn };
            RawPosition = pos;
            Position = pos;
            Velocity = vel;
            IFFCodes = codes;
            IFFTypes = 0;
            TrackType = type;
            FFS = _ffs;

            Timestamp = DateTime.UtcNow;

            Callsign = vcs;
            SpecType = spec;

            ShowHistory = true;
            History = new Queue<Vector64>(10);
            NewestHistory = Timestamp - TimeSpan.FromSeconds(30);
        }

        public void AddNewData (TrackDatum data, IFFData[] codes)
        {
            RawPosition = data.Position;
            Position = data.Position;

            Velocity = data.Velocity;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LatLon)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Velocity)));

            if (data is TrackDatum)
            {
                var _asDatum = (TrackDatum)data;

                Altitude = _asDatum.Altitude;
                Heading= _asDatum.Heading_Deg;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Altitude)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Heading)));
            }

            IFFTypes = IFFType.None;
            IFFCodes = codes;
            foreach (var code in IFFCodes)
            {
                IFFTypes = IFFTypes & code.Type;
            }

            MakeAlive(data.Timestamp);
            freshData = true;

            UpdateHistory();
        }

        public void MakeAlive (DateTime timestamp)
        {
            if (Status == TrackStatus.Drop_Auto) Status = TrackStatus.Live;
            if (Status == TrackStatus.Live || Status == TrackStatus.Protected || Status == TrackStatus.Generated)
            {
                Timestamp = timestamp;
            }
        }

        public void SetTrackStatus (TrackStatus newStatus)
        {
            Status = newStatus;

            if (Status == TrackStatus.Drop_Manual)
                Timestamp = DateTime.UtcNow;
        }

        protected void UpdateHistory()
        {
            DateTime nextHist = NewestHistory + TimeSpan.FromSeconds(60);

            if(Timestamp.CompareTo(nextHist) >= 0)
            {
                if(History.Count >= 10) History.Dequeue();
                History.Enqueue(RawPosition);
                NewestHistory = Timestamp;
            }
        }

        public void UpdateVisual (float dt)
        {
            if (freshData)
            {
                freshData = false;
                return;
            }

            ConformalMove(dt);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LatLon)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Velocity)));
        }

        public void ConformalMove (float dt)
        {
            Vector64 outAxis = Position.normalized;
            Vector64 forwardAxis = Velocity.normalized;

            double arcLength = Velocity.magnitude * dt;
            double angle = arcLength / MathL.Conversions.EarthRadius;

            double sine = Math.Sin(angle);
            double cosine = Math.Cos(angle);

            Position += MathL.Conversions.EarthRadius * (forwardAxis * sine
                - outAxis * (1 - cosine));

            Velocity += arcLength * (-(1 - cosine) * forwardAxis - sine * outAxis);
        }
    }
}
