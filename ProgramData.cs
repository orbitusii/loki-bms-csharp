using System;
using System.Collections.Generic;
using System.Text;
using loki_bms_csharp.MathL;
using loki_bms_csharp.Settings;

namespace loki_bms_csharp
{
    public static class ProgramData
    {
        public static MainWindow MainWindow;
        public static ViewSettings ViewSettings;

        public static void Initialize (MainWindow window)
        {
            MainWindow = window;
            LoadViewSettings();
        }

        public static void LoadViewSettings ()
        {
            ViewSettings = new ViewSettings();

            ViewSettings.UpdateViewPosition(new LatLonCoord { Lat_Degrees = 0, Lon_Degrees = 0 });
            ViewSettings.SetZoom(16);
        }
    }
}
