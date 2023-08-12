using loki_bms_csharp.Geometry;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for GeometryWindow.xaml
    /// </summary>
    public partial class GeometryWindow : Window
    {
        public GeometryWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProgramData.GeoWindow = null;
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "KML files (*.kml)|*.kml|All files (*.*)|*.*";
            fileDialog.InitialDirectory = ProgramData.AppDataPath;

            if (fileDialog.ShowDialog() == true)
            {
                foreach (string filename in fileDialog.FileNames)
                {
                    MapGeometry geo = MapGeometry.LoadFromKML(filename);
                    geo.CachePaths(ProgramData.ViewSettings.CameraMatrix);

                    ProgramData.GeometrySettings.Geometries.Add(geo);
                }
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            MapGeometry toRemove = (MapGeometry)GeometryList.SelectedItem;
            ProgramData.GeometrySettings.Geometries.Remove(toRemove);
        }

        private void ForceTextInputToNumerals(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !TextIsNumeric(e.Text);
        }

        private void PasteOnlyNumbers(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string input = (string)e.DataObject.GetData(typeof(string));
                if (!TextIsNumeric(input)) e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool TextIsNumeric(string input)
        {
            char separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            return input.All(c => char.IsNumber(c) || c.Equals(separator));
        }
    }
}
