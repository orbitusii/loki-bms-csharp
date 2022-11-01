using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.Database
{
    public struct TrackNumber
    {
        public short Internal;
        public short External;
        public short Datalink;

        public TrackNumber (short intl = 0, short extl = -1, short dtlk = -1)
        {
            Internal = intl;
            External = extl;
            Datalink = dtlk;
        }
    }
}
