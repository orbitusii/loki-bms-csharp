using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using loki_bms_csharp.Database;

namespace loki_bms_csharp
{
    /// <summary>
    /// Interaction logic for SourcesWindow.xaml
    /// </summary>
    public partial class SourceWindow : Window
    {
        public SourceWindow()
        {
            InitializeComponent();
        }

        private void SourcesWin_Loaded(object sender, RoutedEventArgs e)
        {
            NamesListBox.SelectedIndex = 0;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProgramData.SrcWindow = null;
        }

        private void ForceTextInputToNumerals(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextIsNumeric(e.Text);
        }

        private void PasteOnlyNumbers(object sender, DataObjectPastingEventArgs e)
        {
            if(e.DataObject.GetDataPresent(typeof(string)))
            {
                string input = (string)e.DataObject.GetData(typeof(string));
                if (!TextIsNumeric(input)) e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool TextIsNumeric (string input)
        {
            return input.All(c => char.IsDigit(c));
        }

        private void ClickAddSourceButton(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgramData.DataSources.Add(new Database.DataSource());
            }), System.Windows.Threading.DispatcherPriority.Normal);
        }

        private void ClickRemoveSourceButton(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgramData.DataSources.RemoveAt(NamesListBox.SelectedIndex);
            }), System.Windows.Threading.DispatcherPriority.Normal);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataSource ds = (DataSource)SourceDetails.DataContext;
            LatLonCoord bullsPos = new LatLonCoord { Alt = 0, Lat_Degrees = ds.Bullseye.Lat, Lon_Degrees = ds.Bullseye.Lon };
            ProgramData.BullseyePos = bullsPos;
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            DataSource ds = (DataSource)SourceDetails.DataContext;
            ds.PauseUnpause();
        }
    }
}
