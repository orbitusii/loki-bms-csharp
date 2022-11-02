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

namespace loki_bms_csharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Brush StrokeBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        private Brush FillBrush = new SolidColorBrush(Color.FromArgb(128,255, 0, 0));

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
