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
using loki_bms_csharp.Database;
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
        public static readonly DependencyProperty DebugProperty =
            DependencyProperty.Register("DrawDebug", typeof(bool), typeof(MainWindow));
        public bool DrawDebug
        {
            get { return (bool) GetValue(DebugProperty); }
            set { SetValue(DebugProperty, value); }
        }

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

            UserData.MainWindow = this;
            UserData.UpdateViewPosition(new LatLonCoord { Lat_Degrees = 0, Lon_Degrees = 0 });
            UserData.SetZoom(16);

            ConfigureClock();
            ScopeCanvas.PaintSurface += OnPaintSurface;
            DrawDebug = false;

            TrackDatabase.Initialize(1000);
            var FndTrack = TrackDatabase.InitiateTrack(new LatLonCoord { Lat_Degrees = 0, Lon_Degrees = 0, Alt = 0 },heading: Math.PI/4, speed: 1);
            var HosTrack = TrackDatabase.InitiateTrack(new LatLonCoord { Lat_Degrees = -0.0001, Lon_Degrees = 0, Alt = 0 }, heading: Math.PI / 4, speed: 1);
            var PndTrack = TrackDatabase.InitiateTrack(new LatLonCoord { Lat_Degrees = 0, Lon_Degrees = -0.0001, Alt = 0 }, heading: Math.PI / 4, speed: 1);

            FndTrack.FFS = FriendFoeStatus.KnownFriend;
            HosTrack.FFS = FriendFoeStatus.Hostile;
            PndTrack.FFS = FriendFoeStatus.AssumedFriend;

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
            var renderer = new ScopeRenderer(args, UserData.UpdateCameraMatrix());
            renderer.SetVerticalSize(UserData.VerticalFOV);

            renderer.DrawCircle((0, 0, 0), 6378137, SKColor.FromHsl(215, 30, 8));

            if (DrawDebug) renderer.DrawAxisLines();

            renderer.DrawFromDatabase();
        }

        public LatLonCoord GetLatLonAltSliders()
        {
            double lat = Lat_Slider.Value;
            double lon = Lon_Slider.Value;
            double alt = Zoom_Slider.Value;

            return new LatLonCoord { Lat_Degrees = lat, Lon_Degrees = lon, Alt = alt };
        }

        private void PrimaryDisplay_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RenderClock.Stop();
        }

        private void ScopeCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScopeMouseInput.ProcessScrollWheel(e);
        }

        private void ScopeCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ScopeMouseInput.ProcessMouseClick(e);
        }
    }
}
