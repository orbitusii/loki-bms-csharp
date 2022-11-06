using System;
using System.Collections.Generic;
using System.Text;
using loki_bms_csharp.MathL;
using loki_bms_csharp.Settings;
using loki_bms_csharp.Geometry;

namespace loki_bms_csharp
{
    public static class ProgramData
    {
        public static MainWindow MainWindow;
        public static ViewSettings ViewSettings;
        public static MapGeometry WorldLandmasses;

        public static void Initialize (MainWindow window)
        {
            MainWindow = window;

            LoadViewSettings();

            string landSVG = Encoding.UTF8.GetString(Properties.Resources.WorldLandmasses);
            WorldLandmasses = MapGeometry.LoadGeometryFromFile(landSVG);
            WorldLandmasses.CachePaths(ViewSettings.CameraMatrix);

            ViewSettings.OnViewCenterChanged += WorldLandmasses.CachePaths;

        }

        public static void LoadViewSettings ()
        {
            ViewSettings = new ViewSettings();

            ViewSettings.UpdateViewPosition(new LatLonCoord { Lat_Degrees = 0, Lon_Degrees = 0 });
            ViewSettings.SetZoom(16);
        }
    }
}
