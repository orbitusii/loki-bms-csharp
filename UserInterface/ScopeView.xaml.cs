using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace loki_bms_csharp.UserInterface
{
    /// <summary>
    /// Interaction logic for ScopeView.xaml
    /// </summary>
    public partial class ScopeView : UserControl
    {
        public static readonly DependencyProperty RenderProperty =
            DependencyProperty.Register(
                "RenderEnabled", typeof(bool), typeof(ScopeView),
                new PropertyMetadata(true));

        public bool RenderEnabled
        {
            get { return (bool)GetValue(RenderProperty); }
            set { SetValue(RenderProperty, value); }
        }

        public int FPS { get; private set; } = 15;
        private Timer RenderTimer = new Timer();
        private delegate void RenderDelegate ();

        public List<IScopeSymbol> Children;

        public SolidColorBrush TestBrush;
        private DateTime StartTime;

        public ScopeView()
        {
            InitializeComponent();

            BeginInit();

            UpdateFPS(FPS);
            ConfigureTimer();
            Children = new List<IScopeSymbol>();
            StartTime = DateTime.UtcNow;
            TestBrush = new SolidColorBrush(Color.FromRgb(0, 15, 60));

            EndInit();
        }

        public void UpdateFPS(int newFPS)
        {
            VerifyAccess();

            FPS = newFPS;

            float frameDelayMs = 1000f / FPS;
            RenderTimer.Interval = frameDelayMs;
        }

        private void ConfigureTimer()
        {
            RenderTimer.Elapsed += InvokeRepaint;
            RenderTimer.Enabled = RenderEnabled;
        }

        public void InvokeRepaint (Object source, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new RenderDelegate(Repaint), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void Repaint ()
        {
            VerifyAccess();

            var runTime = (DateTime.UtcNow - StartTime).TotalSeconds;

            byte r = (byte)(127.5 * Math.Abs(Math.Sin(runTime/2 + Math.PI / 2)));
            byte g = (byte)(127.5 * Math.Abs(Math.Sin(runTime/3)));
            byte b = (byte)(127.5 * Math.Abs(Math.Sin(runTime/5 - Math.PI / 2)));

            SolidColorBrush br = this.FindResource("TestPattern") as SolidColorBrush;
            if(br != null)
            {
                System.Diagnostics.Debug.WriteLine($"t={runTime}, color={r}, {g}, {b}");
                br.Color = Color.FromRgb(r, g, b);
            }
            

            if (Children.Count < 1) return;

            

        }
    }
}
