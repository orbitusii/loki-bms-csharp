using loki_bms_common.Database;
using SkiaSharp;
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
        public string? Stroke_Friend = "#FF0000FF";
        [XmlElement]
        public string? Stroke_AsFnd = "#FF008000";
        [XmlElement]
        public string? Stroke_Neutral = "#FF800080";
        [XmlElement]
        public string? Stroke_Suspect = "#FFFFA500";
        [XmlElement]
        public string? Stroke_Hostile = "#FFFF0000";
        [XmlElement]
        public string? Stroke_Unknown = "#FFFFFF00";
        [XmlElement]
        public string? Stroke_Pending = "#FF808080";

        [XmlElement]
        public string? Fill_Friend = "#800000FF";
        [XmlElement]
        public string? Fill_AsFnd = "#80008000";
        [XmlElement]
        public string? Fill_Neutral = "#80800080";
        [XmlElement]
        public string? Fill_Suspect = "#80FFA500";
        [XmlElement]
        public string? Fill_Hostile = "#80FF0000";
        [XmlElement]
        public string? Fill_Unknown = "#80FFFF00";
        [XmlElement]
        public string? Fill_Pending = "#80808080";

        public enum PaintType
        {
            Stroke,
            Fill
        }

        public (SKPaint stroke, SKPaint fill) GetPaint (ISelectableObject target)
        {
            var strokecolor = target.FFS switch
            {
                FriendFoeStatus.KnownFriend => SKColor.TryParse(Stroke_Friend, out var _out) ? _out : SKColors.Blue,
                FriendFoeStatus.AssumedFriend => SKColor.TryParse(Stroke_AsFnd, out var _out) ? _out : SKColors.Green,
                FriendFoeStatus.Neutral => SKColor.TryParse(Stroke_Neutral, out var _out) ? _out : SKColors.Purple,
                FriendFoeStatus.Suspect => SKColor.TryParse(Stroke_Suspect, out var _out) ? _out : SKColors.Orange,
                FriendFoeStatus.Hostile => SKColor.TryParse(Stroke_Hostile, out var _out) ? _out : SKColors.Red,
                FriendFoeStatus.Unknown => SKColor.TryParse(Stroke_Unknown, out var _out) ? _out : SKColors.Yellow,
                FriendFoeStatus.Pending => SKColor.TryParse(Stroke_Pending, out var _out) ? _out : SKColors.Yellow,
                _ => SKColors.Gray
            };

            var fillcolor = (target.FFS switch
            {
                FriendFoeStatus.KnownFriend => SKColor.TryParse(Fill_Friend, out var _out) ? _out : SKColors.Blue,
                FriendFoeStatus.AssumedFriend => SKColor.TryParse(Fill_AsFnd, out var _out) ? _out : SKColors.Green,
                FriendFoeStatus.Neutral => SKColor.TryParse(Fill_Neutral, out var _out) ? _out : SKColors.Purple,
                FriendFoeStatus.Suspect => SKColor.TryParse(Fill_Suspect, out var _out) ? _out : SKColors.Orange,
                FriendFoeStatus.Hostile => SKColor.TryParse(Fill_Hostile, out var _out) ? _out : SKColors.Red,
                FriendFoeStatus.Unknown => SKColor.TryParse(Fill_Unknown, out var _out) ? _out : SKColors.Yellow,
                FriendFoeStatus.Pending => SKColor.TryParse(Fill_Pending, out var _out) ? _out : SKColors.Yellow,
                _ => SKColors.Gray
            }).WithAlpha(128);

            SKPaint stroke;
            SKPaint fill;

            // Same color present in the cached stroke paint, reuse the cached one
            if (cachedStroke.TryGetValue(target.FFS, out var _stroke) && _stroke.Color.Equals(strokecolor))
            {
                stroke = _stroke;
            }
            else
            {
                stroke = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = strokecolor,
                    StrokeWidth = 2,
                };
                cachedStroke[target.FFS] = stroke;
            }

            // Same color present in the cached fill paint, reuse the cached one
            if (cachedFill.TryGetValue(target.FFS, out var _fill) && _fill.Color.Equals(fillcolor))
            {
                fill = _fill;
            }
            else
            {
                fill = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = fillcolor
                };
                cachedFill[target.FFS] = fill;
            }

            return (stroke, fill);
        }

        [XmlIgnore]
        public Dictionary<FriendFoeStatus, SKPaint> cachedStroke = new Dictionary<FriendFoeStatus, SKPaint>();
        [XmlIgnore]
        public Dictionary<FriendFoeStatus, SKPaint> cachedFill = new Dictionary<FriendFoeStatus, SKPaint>();
    }
}
