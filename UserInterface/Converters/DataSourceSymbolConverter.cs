using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using loki_bms_csharp.Database;
using loki_bms_csharp.Geometry.SVG;

namespace loki_bms_csharp.UserInterface.Converters
{
    [ValueConversion(typeof(string), typeof(int))]
    public class DataSourceSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ds = (string)value;
            SVGPath path = ProgramData.DataSymbols.Find(x => x.name == ds);
            return ProgramData.DataSymbols.IndexOf(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = (int)value;
            SVGPath path = ProgramData.DataSymbols[index];
            return path.name;
        }
    }
}
