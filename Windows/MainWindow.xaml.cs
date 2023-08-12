using loki_bms_common.Database;
using loki_bms_csharp.UserInterface;
using loki_bms_csharp.Windows;
using loki_bms_csharp.Windows.Controls;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
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
            get { return (bool)GetValue(DebugProperty); }
            set { SetValue(DebugProperty, value); }
        }

        public ScopeRenderer ScopeRenderer = new ScopeRenderer();
        public RightClickWindow RightClickMenu { get; protected set; } = new RightClickWindow();

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
            ProgramData.Database.OnDatabaseUpdated += Redraw;

            ScopeCanvas.PaintSurface += OnPaintSurface;
            DrawDebug = false;

            RightClickMenu.Hide();

            EndInit();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProgramData.Database.OnDatabaseUpdated -= Redraw;
            RightClickMenu.Close();
            ProgramData.Shutdown();
        }

        public void Redraw()
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

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (ScopeCanvas.IsFocused)
                ProcessInput(ScopeHotkeys.OnKeyDown(e));
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (ScopeCanvas.IsFocused)
                ProcessInput(ScopeHotkeys.OnKeyUp(e));
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ScopeCanvas.Focus();
            ProcessInput(ScopeMouseInput.OnMouseDown(e));
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            ProcessInput(ScopeMouseInput.OnMouseUp(e));
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ProcessInput(ScopeMouseInput.OnMouseWheel(e));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            ProcessInput(ScopeMouseInput.OnMouseMove(e));
        }

        private void ProcessInput(InputData dat)
        {
            if (dat.RequiresRedraw)
            {
                Redraw();
            }

            if (dat is MouseInputData mid)
            {
                if (!mid.OnlyMoved) RightClickMenu.Hide();

                if (mid.RightClickMenuOpen)
                {
                    if (mid.RightClickMenuPos is null)
                        throw new NullReferenceException($"Didn't get a screenspace point for the right click menu when one was expected!");
                    if (mid.worldClickPoint is null)
                        throw new NullReferenceException($"Didn't get a Vector64 for the right click menu when one was expected!");

                    Point RealPos = (Point)mid.RightClickMenuPos;

                    RightClickMenu.Popup(RealPos, mid.worldClickPoint ?? Vector64.zero);
                }
            }
        }

        private void SourceMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProgramData.SrcWindow == null)
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

        private void ColorsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProgramData.BrushWindow is TemplateWindow window)
            {
                ProgramData.BrushWindow.Focus();
            }
            else
            {
                ProgramData.BrushWindow = TemplateWindow.Create<BrushMenu>("Brushes", true, ProgramData.ColorSettings, () => { ProgramData.BrushWindow = null; });
            }
        }
    }
}
