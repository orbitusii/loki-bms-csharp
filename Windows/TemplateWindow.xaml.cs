using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace loki_bms_csharp.Windows
{
    /// <summary>
    /// Interaction logic for TemplateWindow.xaml
    /// </summary>
    public partial class TemplateWindow : Window
    {
        public delegate void ClosingCallback();
        private ClosingCallback ClosingCallbacks;

        private TemplateWindow()
        {
            InitializeComponent();
        }

        public static TemplateWindow Create<T> (string Title, bool CanResize, object? DataContext, ClosingCallback? closingCallback) where T: UserControl
        {
            var window = new TemplateWindow();
            window.Title = Title;
            
            window.ResizeMode = CanResize ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;

            UserControl contentControl = (UserControl)Activator.CreateInstance (typeof (T));
            window.Width = contentControl.MinWidth;
            window.Height = contentControl.MinHeight + 50;
            window.ContentGrid.Children.Add(contentControl);
            contentControl.DataContext = DataContext;

            window.ClosingCallbacks += closingCallback;

            window.Show();
            return window;
        }

        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClosingCallbacks?.Invoke();
        }
    }
}
