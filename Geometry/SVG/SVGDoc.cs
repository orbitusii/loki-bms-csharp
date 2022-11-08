using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace loki_bms_csharp.Geometry.SVG
{
    [XmlRoot("svg", Namespace = "http://www.w3.org/2000/svg")]
    public class SVGDoc
    {
        [XmlAttribute]
        public string width;
        [XmlAttribute]
        public string height;
        [XmlAttribute]
        public string viewBox;
        [XmlAttribute]
        public string version;
        [XmlAttribute]
        public string style;

        [XmlAnyAttribute]
        public string[] extraAttributes;

        [XmlElement("path")]
        public SVGPath[] paths;

        [XmlElement("g")]
        public SVGGroup[] groups;
    }
}
