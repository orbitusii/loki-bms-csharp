using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace loki_bms_csharp.Geometry.SVG
{
    public class SVGGroup
    {
        [XmlAttribute("id")]
        public string name;

        [XmlAnyAttribute]
        public string[] extraAttributes;

        [XmlElement("path")]
        public SVGPath[] paths;
    }
}
