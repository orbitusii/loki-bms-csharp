using loki_bms_common.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace loki_bms_csharp.Settings
{
    public class ColorSettings: SerializableSettings<ColorSettings>, INotifyPropertyChanged
    {
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

        public void SaveToFile (string filename)
        {
            SaveToFile(filename, this);
        }
    }
}
