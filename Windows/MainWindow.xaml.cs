using loki_bms_csharp.Database;
using loki_bms_csharp.UserInterface;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace loki_bms_csharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty DebugProperty =
            DependencyProperty.Register("DrawDebug", typeof(bool), typeof(MainWindow));
        public bool DrawDebug
        {
            get { return (bool) GetValue(DebugProperty); }
            set { SetValue(DebugProperty, value); }
        }

        public ScopeRenderer ScopeRenderer = new ScopeRenderer();

        public int FPS { get; private set; } = 30;
        public double MSPerFrame
        {
            get { return 1000 / FPS; }
        }

        public MainWindow()
        {
            InitializeComponent();

            BeginInit();

            ProgramData.MainWindow = this;

            ScopeCanvas.PaintSurface += OnPaintSurface;
            DrawDebug = false;

            EndInit();
        }

        private void PrimaryDisplay_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProgramData.Shutdown();
        }

        public void Redraw ()
        {
            try
            {
                Dispatcher.Invoke(delegate ()
                {
                    ScopeCanvas.InvalidateVisual();
                }, System.Windows.Threading.DispatcherPriority.Render);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Missed a frame draw due to an exception!\n" + e.Message);
            }
        }

        private void OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs args)
        {
            ScopeRenderer.Redraw(args, ProgramData.ViewSettings.CameraMatrix, ProgramData.ViewSettings.VerticalFOV);
        }

        public LatLonCoord GetLatLonAltSliders()
        {
            double lat = Lat_Slider.Value;
            double lon = Lon_Slider.Value;
            double alt = Zoom_Slider.Value;

            return new LatLonCoord { Lat_Degrees = lat, Lon_Degrees = lon, Alt = alt };
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            CheckForRedraw(ScopeHotkeys.OnKeyDown(e));

        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            CheckForRedraw(ScopeHotkeys.OnKeyUp(e));
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            CheckForRedraw(ScopeMouseInput.OnMouseDown(e));
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            CheckForRedraw(ScopeMouseInput.OnMouseUp(e));
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            CheckForRedraw(ScopeMouseInput.OnMouseWheel(e));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            CheckForRedraw(ScopeMouseInput.OnMouseMove(e));
        }

        private void CheckForRedraw (InputData dat)
        {
            if(dat.RequiresRedraw)
            {
                Redraw();
            }
        }

        private void SourceMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if(ProgramData.SrcWindow == null)
            {
                ProgramData.SrcWindow = new SourceWindow();
                ProgramData.SrcWindow.Show();
            }
            else
            {
                ProgramData.SrcWindow.Focus();
            }
        }

        private void GeometryButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProgramData.GeoWindow == null)
            {
                ProgramData.GeoWindow = new Windows.GeometryWindow();
                ProgramData.GeoWindow.Show();
            }
            else
            {
                ProgramData.GeoWindow.Focus();
            }
        }
    }
}
