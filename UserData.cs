using System;
using System.Collections.Generic;
using System.Text;
using loki_bms_csharp.MathL;

namespace loki_bms_csharp
{
    public static class UserData
    {
        public static MainWindow MainWindow;
        public static LatLonCoord ViewPosition { get; private set; }
        public static TangentMatrix CameraMatrix { get; private set; }
        public static double ZoomIncrement = 16;
        public static double VerticalFOV
        {
            get
            {
                return Math.Pow(2, ZoomIncrement) * 200;
            }
        }

        public static DateTime StartupTime = DateTime.Now;
        public static double RunTime
        {
            get
            {
                return (DateTime.Now - StartupTime).TotalSeconds;
            }
        }

        public static LatLonCoord UpdateViewPosition (LatLonCoord newView)
        {
            ViewPosition = new LatLonCoord { Lat_Rad = newView.Lat_Rad, Lon_Rad = newView.Lon_Rad, Alt = 0 };

            return ViewPosition;
        }

        /// <summary>
        /// Sets the View's Zoom level to a specific value
        /// </summary>
        /// <param name="Level">The zoom level to use. Will be clamped between 0 and 16.</param>
        /// <returns>Resulting Vertical Field of View</returns>
        public static double SetZoom (double Level)
        {
            ZoomIncrement = Math.Clamp(Level, 0, 16);
            MainWindow.Zoom_Slider.Value = Level;
            return VerticalFOV;
        }

        /// <summary>
        /// Sets the view's zoom level based on a desired Vertical FOV
        /// </summary>
        /// <param name="FOV">The Vertical FOV, in meters, to use for the new Zoom increment</param>
        /// <returns>The new Zoom increment (between 0 and 16)</returns>
        public static double SetZoomByFOV (double FOV)
        {
            FOV /= 200;

            SetZoom(Math.Log2(FOV));
            return ZoomIncrement;
        }

        public static TangentMatrix UpdateCameraMatrix ()
        {
            CameraMatrix = TangentMatrix.FromLatLon(ViewPosition);
            CameraMatrix.SetOrigin(CameraMatrix.Out * 6378137);

            return CameraMatrix;
        }
    }
}
