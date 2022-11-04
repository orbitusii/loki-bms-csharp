using System;
using System.Collections.Generic;
using System.Text;

namespace loki_bms_csharp.UserInterface
{
    public class ZoomPreset
    {
        public LatLonCoord Center;
        public double Zoom;

        public ZoomPreset(LatLonCoord _center, double _zoom)
        {
            Center = _center;
            Zoom = _zoom;
        }
    }
}
