using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_bms_csharp.Extensions
{
    public static class Extensions
    {
        public static SKPaint GetPaint (this LokiDataSource src)
        {
            var color = SKColor.TryParse(src.DataColor, out var _parsed) ? _parsed : SKColors.White;
            return new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = color };
        }
    }
}
