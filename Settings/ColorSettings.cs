using loki_bms_common.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace loki_bms_csharp.Settings
{
    internal class ColorSettings
    {
        [XmlAttribute]
        public Dictionary<FriendFoeStatus, SkiaSharp.SKPaint> StrokeByFFS =
            new Dictionary<FriendFoeStatus, SkiaSharp.SKPaint>()
            {
                {FriendFoeStatus.KnownFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Blue, StrokeWidth = 2 } },
                {FriendFoeStatus.AssumedFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Green, StrokeWidth = 2 } },
                {FriendFoeStatus.Neutral, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Purple, StrokeWidth = 2 }  },
                {FriendFoeStatus.Suspect, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Orange, StrokeWidth = 2 }  },
                {FriendFoeStatus.Hostile, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Red, StrokeWidth = 2 }  },
                {FriendFoeStatus.Unknown, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Yellow, StrokeWidth = 2 }  },
                {FriendFoeStatus.Pending, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Stroke, Color = SkiaSharp.SKColors.Gray, StrokeWidth = 2 }  },
            };
        public static Dictionary<FriendFoeStatus, SkiaSharp.SKPaint> FillByFFS =
            new Dictionary<FriendFoeStatus, SkiaSharp.SKPaint>()
            {
                {FriendFoeStatus.KnownFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Blue.WithAlpha(128) } },
                {FriendFoeStatus.AssumedFriend, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Green.WithAlpha(128) } },
                {FriendFoeStatus.Neutral, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Purple.WithAlpha(128) }  },
                {FriendFoeStatus.Suspect, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Orange.WithAlpha(128) }  },
                {FriendFoeStatus.Hostile, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Red.WithAlpha(128) }  },
                {FriendFoeStatus.Unknown, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Yellow.WithAlpha(128) }  },
                {FriendFoeStatus.Pending, new SkiaSharp.SKPaint{Style = SkiaSharp.SKPaintStyle.Fill, Color = SkiaSharp.SKColors.Gray.WithAlpha(128) }  },
            };
    }
}
