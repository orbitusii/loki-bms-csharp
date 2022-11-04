using System;
using System.Collections.Generic;
using System.Text;
using loki_bms_csharp.MathL;

namespace loki_bms_csharp
{
    public static class UserData
    {
        public static MainWindow MainWindow;
        public static LatLonCoord ViewCenter { get; private set; }
        public delegate void ViewCenterChangedCallback(TangentMatrix mat);
        public static ViewCenterChangedCallback OnViewCenterChanged;

        public static TangentMatrix CameraMatrix { get; private set; }
        public static double ZoomIncrement = 16;
        public static double VerticalFOV
        {
            get
            {
                return Math.Pow(2, ZoomIncrement) * 200;
            }
        }
        private static UserInterface.ZoomPreset[] ZoomPresets = new UserInterface.ZoomPreset[10];

        public static DateTime StartupTime = DateTime.Now;
        public static double RunTime
        {
            get
            {
                return (DateTime.Now - StartupTime).TotalSeconds;
            }
        }

        public static void SetViewPreset (int index, UserInterface.ZoomPreset preset)
        {
            try
            {
                ZoomPresets[index] = preset;
            }
            catch
            {
                return;
            }
        }

        public static bool SnapToView (int index)
        {
            UserInterface.ZoomPreset preset = ZoomPresets[index];
            if (preset == null)
            {
                return false;
            }

            UpdateViewPosition(preset.Center);
            SetZoom(preset.Zoom);

            return true;
        }

        public static LatLonCoord UpdateViewPosition (LatLonCoord newView)
        {
            ViewCenter = new LatLonCoord { Lat_Rad = newView.Lat_Rad, Lon_Rad = newView.Lon_Rad, Alt = 0 };

            UpdateCameraMatrix();
            OnViewCenterChanged(CameraMatrix);

            return ViewCenter;
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
            CameraMatrix = TangentMatrix.FromLatLon(ViewCenter);
            CameraMatrix.SetOrigin(CameraMatrix.Out * Conversions.EarthRadius);

            return CameraMatrix;
        }
    }
}
