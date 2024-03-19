using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using loki_bms_common.Database;
using Color = System.Windows.Media.Color;

namespace loki_bms_csharp.UserInterface.Converters
{
    [ValueConversion(typeof(LokiDataSource.SourceStatus), typeof(SolidColorBrush))]
    public class DataSourceStatusConverter: IValueConverter
    {
        public static Color green = (Color)ColorConverter.ConvertFromString("#FF228B22");
        public static Color orange = (Color)ColorConverter.ConvertFromString("#FFFFA500");
        public static Color darkred = (Color)ColorConverter.ConvertFromString("#FF8B0000");
        public static Color darkgray = (Color)ColorConverter.ConvertFromString("#FFA9A9A9");

        static readonly SolidColorBrush activeBrush = new SolidColorBrush(green);
        static readonly SolidColorBrush startingBrush = new SolidColorBrush(orange);
        static readonly SolidColorBrush disconnectedBrush = new SolidColorBrush(darkred);
        static readonly SolidColorBrush offlineBrush = new SolidColorBrush(darkgray);

        public object Convert(object value, Type targetType, object param, CultureInfo _)
        {
            LokiDataSource.SourceStatus? status = (LokiDataSource.SourceStatus)value;

            return status switch
            {
                LokiDataSource.SourceStatus.Active => activeBrush,
                LokiDataSource.SourceStatus.Starting => startingBrush,
                LokiDataSource.SourceStatus.Disconnected => disconnectedBrush,
                _ => offlineBrush,
            };
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo _)
        {
            Color color = (Color)value;

            return LokiDataSource.SourceStatus.Offline;
        }
    }
}
