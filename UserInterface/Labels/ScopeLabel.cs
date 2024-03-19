using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace loki_bms_csharp.UserInterface.Labels
{
    public class ScopeLabel<T>
    {
        public List<LabelItem> LabelItems { get; set; }

        public int Margins { set
            {
                MarginLeft = value;
                MarginRight = value;
                MarginTop= value;
                MarginBottom= value;
            } }

        public int HorizontalMargins
        {
            set
            {
                MarginLeft = value;
                MarginRight = value;
            }
        }
        public int VerticalMargins
        {
            set
            {
                MarginTop = value;
                MarginBottom = value;
            }
        }

        public int MarginLeft = 1;
        public int MarginRight = 1;
        public int MarginTop = 1;
        public int MarginBottom = 1;

        public int LineSpacing = 4;

        public SKFont Font = new(
            SKTypeface.FromFile(Path.Join(ProgramData.ResourcesPath, "Fonts", "CascadiaMono.ttf")),
            size: 12);
        public int charWidth => 7;
        public int charHeight => 8;

        public ScopeLabel ()
        {
            //Debug.WriteLine($"Font typeface: {Font.Typeface}");
        }

        public string[] Evaluate (ISelectableObject target, out SKRect TextBounds, out SKRect BorderBounds)
        {
            string concatted = string.Join("", LabelItems.Select(x => x.Evaluate(target)));

            string[] splitNewLines = concatted.Split('\n');

            int longestLine = -1;

            foreach (string line in splitNewLines)
            {
                line.Trim(' ');
                longestLine = line.Length > longestLine ? line.Length : longestLine;
            }

            BorderBounds = new SKRect(0, 0,
                (longestLine * charWidth) + MarginLeft + MarginRight,
                (splitNewLines.Count() * charHeight) + ((splitNewLines.Count() -1) * LineSpacing) + MarginTop + MarginBottom);
            TextBounds = new SKRect(
                MarginLeft,
                MarginTop + charHeight,
                longestLine * charWidth,
                charHeight + MarginTop);
            return splitNewLines;
        }
    }
}
