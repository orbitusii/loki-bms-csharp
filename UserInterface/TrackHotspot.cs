using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace loki_bms_csharp.UserInterface
{
    public class TrackHotspot
    {
        public SKRect Bounds;
        public int Index;

        public bool Contains (SKPoint point)
        {
            return Bounds.Contains(point);
        }
    }
}
