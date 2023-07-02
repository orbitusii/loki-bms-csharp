using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace loki_bms_common.Database
{
    public class TrackNumberRange
    {
        private short _tnMin;
        private short _tnMax;

        [XmlAttribute("Min")]
        public short TNMin
        {
            get => _tnMin;
            set
            {
                short parsed;
                if(short.TryParse(value.ToString(), out parsed))
                {
                    _tnMin = parsed;
                }
            }
        }

        [XmlAttribute("Max")]
        public short TNMax
        {
            get => _tnMax;
            set
            {
                short parsed;
                if (short.TryParse(value.ToString(), out parsed))
                {
                    _tnMax = parsed;
                }
            }
        }

        [XmlIgnore]
        public short Range
        {
            get => (short)(TNMax - TNMin);
        }

        private short _next = 0;
        [XmlIgnore]
        public short NextTN
        {
            get => (short)(TNMin + (_next++ % Range));
        }
    }
}
