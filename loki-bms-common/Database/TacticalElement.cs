﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_bms_common.Database
{
    public class TacticalElement: IPositionedObject
    {
        public Vector64 Position { get; set; }
        public virtual Vector64 Velocity => Vector64.zero;

        public string Name = "New TE";
        public string SpecialInfo = string.Empty;

        public FriendFoeStatus FriendFoe = FriendFoeStatus.Pending;
        public TrackCategory Category = TrackCategory.None;
    }
}
