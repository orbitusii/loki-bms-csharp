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

namespace loki_bms_csharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Brush StrokeBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        private Brush FillBrush = new SolidColorBrush(Color.FromArgb(128,255, 0, 0));
        private bool hasShape;

        private int FPS = 30;
        readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private Thread TimerThread;

        public delegate void RepaintDelegate();
        public RepaintDelegate RPD;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_0_Click(object sender, RoutedEventArgs e)
        {
            if(hasShape)
            {
                ScopeCanvas.Children.Clear();
            }
            else
            {
                Ellipse newE = new Ellipse
                {
                    Width = 40,
                    Height = 40,
                    Stroke = StrokeBrush,
                    Fill = FillBrush,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                };
                Canvas.SetLeft(newE, X_Slider.Value - newE.Width / 2);
                Canvas.SetBottom(newE, Y_Slider.Value - newE.Height / 2);

                ScopeCanvas.Children.Add(newE);
            }

            hasShape = !hasShape;
        }

        public void ScheduleRepaint (Object source, System.Timers.ElapsedEventArgs e)
        {
            Debug.WriteLine($"Repainting Main Scope {DateTime.Now:h:mm:ss.fff}");
            Dispatcher.BeginInvoke(RPD, System.Windows.Threading.DispatcherPriority.Render);
        }

        public void RepaintCanvas ()
        {
            if (ScopeCanvas.Children.Count < 1) return;

            var shape = ScopeCanvas.Children[0];

            if(shape != null)
            {
                double width = (double)shape.GetValue(WidthProperty);
                double height = (double)shape.GetValue(HeightProperty);

                Canvas.SetLeft(shape, X_Slider.Value - width / 2);
                Canvas.SetBottom(shape, Y_Slider.Value - height / 2);
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RPD += RepaintCanvas;

            int frameDelay = (int)(1000f / FPS);

            ThreadStart ts = new ThreadStart(delegate ()
            {
                Debug.WriteLine("Timer thread started!");
                var timer = new System.Timers.Timer(frameDelay);
                timer.AutoReset = true;

                /*timer.Elapsed += delegate (Object s, System.Timers.ElapsedEventArgs e)
                {
                    Debug.WriteLine($"Tick tock! {e.SignalTime:h:mm:ss.fff)}");
                };*/
                timer.Elapsed += ScheduleRepaint;

                timer.Enabled = true;

                while(true)
                {
                    if(tokenSource.IsCancellationRequested)
                    {
                        Debug.WriteLine("Closing Rendering thread");
                        timer.Enabled = false;
                        break;
                    }
                }
            });

            Debug.WriteLine("Starting thread for timer?");
            TimerThread = new Thread(ts);
            TimerThread.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tokenSource.Cancel();
            Debug.WriteLine("All threads closed");
        }
    }
}
