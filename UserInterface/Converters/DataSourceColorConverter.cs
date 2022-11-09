using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace loki_bms_csharp.UserInterface.Converters
{
    [ValueConversion(typeof(string), typeof(Color))]
    public class DataSourceColorConverter : IValueConverter
    {
        // Gets the end type (Color)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string colorValue = (string)value;
            Color color = (Color)ColorConverter.ConvertFromString(colorValue);
            return color;
        }

        // Gets the source type (string)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = (Color)value;
            return color.ToString();
        }
    }
}
