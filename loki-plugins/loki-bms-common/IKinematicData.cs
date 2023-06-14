using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_common
{
    public interface IKinematicData
    {
        public Vector64 Position { get; }
        public Vector64 Velocity { get; }

        public DateTime Timestamp { get; }
    }
}
