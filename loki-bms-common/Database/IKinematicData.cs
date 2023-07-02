using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.Database
{
    public interface IKinematicData
    {
        public Vector64 Position { get; }
        public Vector64 Velocity { get; }

        public DateTime Timestamp { get; }
    }
}
