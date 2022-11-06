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

        private Brush StrokeBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        private Brush FillBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        public int FPS { get; private set; } = 30;
        public double MSPerFrame
        {
            get { return 1000 / FPS; }
        }
        private System.Timers.Timer RenderClock;
        public DataSource DataSource;

        public MainWindow()
        {
            InitializeComponent();

            BeginInit();

            ProgramData.Initialize(this);

            ref Settings.ViewSettings viewSettings = ref ProgramData.ViewSettings;

            ConfigureClock();
            ScopeCanvas.PaintSurface += OnPaintSurface;
            DrawDebug = false;

            TrackDatabase.Initialize(1000);
            var FndTrack = TrackDatabase.InitiateTrack(new LatLonCoord { Lat_Degrees = 0, Lon_Degrees = 0, Alt = 0 },heading: Math.PI/4, speed: 50);
            var HosTrack = TrackDatabase.InitiateTrack(new LatLonCoord { Lat_Degrees = -0.0001, Lon_Degrees = 0, Alt = 0 }, heading: Math.PI / 2, speed: 50);
            var PndTrack = TrackDatabase.InitiateTrack(new LatLonCoord { Lat_Degrees = 0, Lon_Degrees = -0.0001, Alt = 0 }, heading: Math.PI * 3 / 4, speed: 50);

            FndTrack.FFS = FriendFoeStatus.KnownFriend;
            HosTrack.FFS = FriendFoeStatus.Hostile;
            PndTrack.FFS = FriendFoeStatus.AssumedFriend;

            //DataSource = new DataSource();
            //_ = DataSource.Activate();

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
            //RenderClock.Start();
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
            using(var renderer = new ScopeRenderer(args, ProgramData.ViewSettings.CameraMatrix))
            {
                renderer.SetVerticalSize(ProgramData.ViewSettings.VerticalFOV);

                renderer.DrawEarth();
                //renderer.DrawGeometry();
                renderer.DrawFromDatabase();

                if (DrawDebug) renderer.DrawAxisLines();

                if(ScopeMouseInput.ClickState == MouseClickState.Left)
                {
                    renderer.DrawMeasureLine(ScopeMouseInput.clickStartPoint, ScopeMouseInput.clickDragPoint, SKColors.White, 1);
                }
            }
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
            ProgramData.Shutdown();
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
    }
}
