using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_common
{
    public interface IPositionedObject
    {
        public Vector64 Position { get; }
        public Vector64 Velocity { get; }
    }
}
