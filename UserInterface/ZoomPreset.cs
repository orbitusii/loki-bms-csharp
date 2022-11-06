using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace loki_bms_csharp.UserInterface
{
    public class ZoomPreset
    {
        [XmlElement("ViewCenter")]
        public LatLonCoord Center;
        [XmlElement("Zoom")]
        public double Zoom;

        public ZoomPreset() { }

        public ZoomPreset(LatLonCoord _center, double _zoom)
        {
            Center = _center;
            Zoom = _zoom;
        }
    }
}
