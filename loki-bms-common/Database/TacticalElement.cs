﻿using loki_bms_common.MathL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_bms_common.Database
{
    public class TacticalElement: ISelectableObject
    {
        public Vector64 Position { get; set; }
        public LatLonCoord LatLon => Conversions.XYZToLL(Position);
        public virtual Vector64 Velocity => Vector64.zero;

        public string Name = "New TE";
        public string SpecialInfo = string.Empty;

        public FriendFoeStatus FFS { get; set; } = FriendFoeStatus.Pending;
        public TEType Category = TEType.None;

        public LokiDataSource? Source;
        public DateTime Timestamp { get; }

        public double Radius = 0;
    }
}
