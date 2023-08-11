using loki_bms_common.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace loki_bms_csharp.Settings
{
    public class ColorSettings: SerializableSettings<ColorSettings>, INotifyPropertyChanged
    {
        public override ColorSettings Original => this;

        private string _ocean = "#ff0E131B";
        [XmlElement]
        public string OceanColor
        {
            get => _ocean;
            set
            {
                _ocean = value;
                PropertyChanged?.Invoke(this, new(nameof(OceanColor)));
            }
        }

        private string _landmass = "#ff303030";
        [XmlElement]
        public string LandmassColor
        {
            get => _landmass;
            set
            {
                _landmass = value;
                PropertyChanged?.Invoke(this, new(nameof(LandmassColor)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [XmlElement]
        public string Stroke_Friend = "#FF0000FF";
        [XmlElement]
        public string Stroke_AsFnd = "#FF008000";
        [XmlElement]
        public string Stroke_Neutral = "#FF800080";
        [XmlElement]
        public string Stroke_Suspect = "#FFFFA500";
        [XmlElement]
        public string Stroke_Hostile = "#FFFF0000";
        [XmlElement]
        public string Stroke_Unknown = "#FFFFFF00";
        [XmlElement]
        public string Stroke_Pending = "#FF808080";

        [XmlElement]
        public string Fill_Friend = "#800000FF";
        [XmlElement]
        public string Fill_AsFnd = "#80008000";
        [XmlElement]
        public string Fill_Neutral = "#80800080";
        [XmlElement]
        public string Fill_Suspect = "#80FFA500";
        [XmlElement]
        public string Fill_Hostile = "#80FF0000";
        [XmlElement]
        public string Fill_Unknown = "#80FFFF00";
        [XmlElement]
        public string Fill_Pending = "#80808080";

        [XmlIgnore]
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
        [XmlIgnore]
        public Dictionary<FriendFoeStatus, SkiaSharp.SKPaint> FillByFFS =
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

        public override void OnLoad()
        {

        }
    }
}
