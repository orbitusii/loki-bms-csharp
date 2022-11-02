using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using loki_bms_csharp.UserInterface;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace loki_bms_csharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Brush StrokeBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        private Brush FillBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        public int FPS { get; private set; } = 30;
        public double MSPerFrame
        {
            get { return 1000 / FPS; }
        }
        private System.Timers.Timer RenderClock;

        public MainWindow()
        {
            InitializeComponent();

            BeginInit();

            ConfigureClock();
            ScopeCanvas.PaintSurface += OnPaintSurface;

            EndInit();
        }

        private void ConfigureClock ()
        {
            RenderClock = new System.Timers.Timer();
            RenderClock.Interval = MSPerFrame;
            RenderClock.Elapsed += delegate (Object s, System.Timers.ElapsedEventArgs args)
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
                    Debug.WriteLine("Missed a frame due to an exception!\n" + e.Message);
                }
            };
            RenderClock.Start();
        }

        private void OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs args)
        {
            (double lat, double lon, double alt) = GetLatLonAltSliders();
            MathL.TangentMatrix camMatrix = MathL.TangentMatrix.FromLatLon(lat, lon, false);

            var renderer = new ScopeRenderer(args, camMatrix);
            renderer.VerticalSize = Math.Pow(2, alt) * 200;

            renderer.DrawCircle((0, 0, 0), 6378137, SKColors.Gray);

            renderer.DrawAxisLines();
        }

        public (double LA, double LO, double AL) GetLatLonAltSliders()
        {
            double lat = Lat_Slider.Value;
            double lon = Lon_Slider.Value;
            double alt = Zoom_Slider.Value;

            return (lat, lon, alt);
        }

        private void PrimaryDisplay_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RenderClock.Stop();
        }
    }
}
