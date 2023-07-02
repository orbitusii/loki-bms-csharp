using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;

namespace loki_bms_csharp.UserInterface.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    class TrackDetailsUnitConverter : IValueConverter
    {
        // First to second (second is displayed)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string param = (string)parameter;
            switch (param)
            {
                case "MetersPerSecToKts":
                    return (double)value * Conversions.MetersPerSecToKnots;
                case "MetersToFeet":
                    return (double)value * Conversions.MetersToFeet;
                case "MetersToNM":
                    return (double)value * Conversions.MetersToNM;
                default:
                    return (double)value;
            }
        }

        // Second to first (first is stored)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string param = (string)parameter;
            switch (param)
            {
                case "MetersPerSecToKts":
                    return (double)value / Conversions.MetersPerSecToKnots;
                case "MetersToFeet":
                    return (double)value / Conversions.MetersToFeet;
                case "MetersToNM":
                    return (double)value / Conversions.MetersToNM;
                default:
                    return (double)value;
            }
        }
    }
}
