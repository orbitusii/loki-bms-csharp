﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace loki_bms_csharp.Database
{
    public class TrackFile : IKinematicData
    {
        public List<TrackNumber> TrackNumbers { get; set; }
        public Vector64 RawPosition { get; private set; }
        public Vector64 Position { get; set; }
        public Vector64 Velocity { get; private set; }

        public IFFData[] IFFCodes { get; private set; }
        public IFFType IFFTypes { get; private set; }

        public DateTime Timestamp { get; private set; }
        private bool freshData = false;
        public TrackType TrackType { get; private set; }
        public TrackCategory Category { get; set; } = TrackCategory.None;

        // TrackFile Amplifying Data
        public FriendFoeStatus FFS { get; set; }

        public string Callsign { get; set; }

        public int SpecType { get; set; }

        public bool ShowHistory { get; set; }
        public List<Vector64> History { get; private set; }
        private DateTime NewestHistory;
        private DateTime OldestHistory;

        public TrackFile () { }

        public TrackFile(TrackNumber.Internal tn, Vector64 pos, Vector64 vel, IFFData[] codes, FriendFoeStatus _ffs = FriendFoeStatus.Pending, TrackType type = TrackType.Sim, string vcs = "", int spec = 0)
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

            ShowHistory = false;
            History = new List<Vector64>(0);
            NewestHistory = Timestamp;
            OldestHistory = Timestamp;
        }

        public void AddNewData (IKinematicData data, IFFData[] codes)
        {
            RawPosition = data.Position;
            Position = data.Position;

            Velocity = data.Velocity;

            //System.Diagnostics.Debug.WriteLine($"{data.Position} vs {Position} @{Velocity}");

            IFFTypes = IFFType.None;
            IFFCodes = codes;
            foreach (var code in IFFCodes)
            {
                IFFTypes = IFFTypes & code.Type;
            }

            Timestamp = data.Timestamp;
            freshData = true;
        }

        public void UpdateVisual (float dt)
        {
            if (freshData)
            {
                freshData = false;
                return;
            }

            ConformalMove(dt);

            /*if(DateTime.UtcNow - NewestHistory > TimeSpan.FromSeconds(10))
            {
                History.Add(Position);
                NewestHistory = DateTime.UtcNow;
            }
            if(DateTime.UtcNow - OldestHistory > TimeSpan.FromSeconds(60) || History.Count > 6)
            {
                History.RemoveAt(0);
                OldestHistory += DateTime.UtcNow - OldestHistory;
            }*/
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
