using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace loki_bms_csharp.Geometry
{
    public class PathSVG
    {
        public string name;
        public string data;

        public char delimiter = 'Z';
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

        public PathSVG[] Subdivide (char nextDelim = 'L')
        {
            var _subpaths = Subpaths;
            PathSVG[] paths = new PathSVG[_subpaths.Length];

            for (int i = 0; i < _subpaths.Length; i++)
            {
                paths[i] = new PathSVG { name = $"{this.name}_{i}", data = _subpaths[i], delimiter = nextDelim };
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
