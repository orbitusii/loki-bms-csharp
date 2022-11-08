using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace loki_bms_csharp.Geometry.SVG
{
    public class SVGPath
    {
        [XmlAttribute("id")]
        public string name { get; set; }
        [XmlAttribute("d")]
        public string data { get; set; }
        [XmlAttribute("style")]
        public string style { get; set; }

        [XmlIgnore]
        public char delimiter = 'Z';
        [XmlIgnore]
        public string[] Subpaths
        {
            get
            {
                if(data.Length > 0)
                {
                    string[] splits = data.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                    return splits;
                }
                return new string[0];
            }
        }

        public SVGPath[] Subdivide (char nextDelim = 'L')
        {
            var _subpaths = Subpaths;
            SVGPath[] paths = new SVGPath[_subpaths.Length];

            for (int i = 0; i < _subpaths.Length; i++)
            {
                paths[i] = new SVGPath { name = $"{this.name}_{i}", data = _subpaths[i], delimiter = nextDelim };
            }

            return paths;
        }

        public System.Windows.Point[] GetPoints ()
        {
            string[] possiblePoints = Subpaths;

            List<System.Windows.Point> pointsList = new List<System.Windows.Point>(0);

            foreach (string point in possiblePoints)
            {
                string cleaned = point.Replace("M", "").Replace("Z", "");
                string[] split = cleaned.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 2)
                {
                    double x, y;
                    bool x_success = double.TryParse(split[0], out x);
                    bool y_success = double.TryParse(split[1], out y);

                    if (x_success && y_success) pointsList.Add(new System.Windows.Point(x, y));
                }
            }

            return pointsList.ToArray();
        }
    }
}
