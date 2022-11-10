using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using loki_bms_csharp.MathL;

namespace loki_bms_csharp.Settings
{
    [XmlRoot("ViewSettings")]
    public class ViewSettings
    {
        [XmlIgnore]
        public bool Debug;

        [XmlElement]
        public LatLonCoord ViewCenter { get; set; }
        [XmlIgnore]
        public TangentMatrix CameraMatrix { get; private set; }

        [XmlElement("Zoom")]
        public double ZoomIncrement = 16;
        public double VerticalFOV
        {
            get => Math.Pow(2, ZoomIncrement) * 200;
        }

        [XmlArray]
        public UserInterface.ZoomPreset[] ZoomPresets { get; set; } = new UserInterface.ZoomPreset[10];

        public bool DrawDebug = false;

        public delegate void ViewCenterChangedCallback(TangentMatrix mat);
        [XmlIgnore]
        public ViewCenterChangedCallback OnViewCenterChanged;

        public void SetViewPreset(int index, UserInterface.ZoomPreset preset)
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

        public bool SnapToView(int index)
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

        public LatLonCoord UpdateViewPosition(LatLonCoord newView)
        {
            ViewCenter = new LatLonCoord { Lat_Rad = newView.Lat_Rad, Lon_Rad = newView.Lon_Rad, Alt = 0 };

            UpdateCameraMatrix();
            try
            {
                OnViewCenterChanged(CameraMatrix);
            }
            catch { }

            return ViewCenter;
        }

        /// <summary>
        /// Sets the View's Zoom level to a specific value
        /// </summary>
        /// <param name="Level">The zoom level to use. Will be clamped between 0 and 16.</param>
        /// <returns>Resulting Vertical Field of View</returns>
        public double SetZoom(double Level)
        {
            ZoomIncrement = Math.Clamp(Level, 0, 16);
            ProgramData.MainWindow.Zoom_Slider.Value = ZoomIncrement;
            return VerticalFOV;
        }

        /// <summary>
        /// Moves the View's Zoom level by amount delta
        /// </summary>
        /// <param name="delta">change in zoom level</param>
        /// <returns></returns>
        public double IncrementZoom (double delta)
        {
            ZoomIncrement = Math.Clamp(ZoomIncrement + delta, 0, 16);
            ProgramData.MainWindow.Zoom_Slider.Value = ZoomIncrement;
            return VerticalFOV;
        }

        /// <summary>
        /// Sets the view's zoom level based on a desired Vertical FOV
        /// </summary>
        /// <param name="FOV">The Vertical FOV, in meters, to use for the new Zoom increment</param>
        /// <returns>The new Zoom increment (between 0 and 16)</returns>
        public double SetZoomByFOV(double FOV)
        {
            FOV /= 200;

            SetZoom(Math.Log2(FOV));
            return ZoomIncrement;
        }

        public TangentMatrix UpdateCameraMatrix()
        {
            CameraMatrix = TangentMatrix.FromLatLon(ViewCenter);
            CameraMatrix.SetOrigin(CameraMatrix.Out * Conversions.EarthRadius);

            return CameraMatrix;
        }
    }
}
