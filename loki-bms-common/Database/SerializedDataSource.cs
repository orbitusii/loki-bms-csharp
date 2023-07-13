using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace loki_bms_common.Database
{
    public class SerializedDataSource
    {
        [XmlAttribute]
        public string SourceType = "unknown";
        [XmlAttribute]
        public string Name = "";

        [XmlAttribute]
        public string Address { get; set; } = "127.0.0.1";
        [XmlAttribute]
        public string Port { get; set; } = "50051";
        [XmlAttribute]
        public string PollRate { get; set; } = "10";
        [XmlAttribute]
        public string SlowPollrate { get; set; } = "30";
        [XmlElement]
        public TrackNumberRange TNRange { get; set; } = new TrackNumberRange { TNMin = -1, TNMax = -1 };
        [XmlAttribute("Symbol")]
        public string DataSymbol { get; set; } = "LineVert";
        [XmlAttribute("Color")]
        public string DataColor { get; set; } = "#ff6600";

        [XmlElement("Extradata")]
        public string[] Extradata { get; set; } = Array.Empty<string>();

        public static SerializedDataSource From(LokiDataSource s)
        {
            SerializedDataSource sds = new SerializedDataSource();

            sds.SourceType = s.GetType().Name;
            sds.Name = s.Name;
            sds.Address = s.Address;
            sds.Port = s.Port;
            sds.PollRate = s.PollRate;
            sds.SlowPollrate = s.SlowPollrate;
            sds.TNRange = s.TNRange;
            sds.DataSymbol = s.DataSymbol;
            sds.DataColor = s.DataColor;

            return sds;
        }
    }
}
