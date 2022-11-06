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

            BeginInit();

            EndInit();
        }

        private void SourcesWin_Loaded(object sender, RoutedEventArgs e)
        {
            NamesListBox.SelectedIndex = 0;
            UpdateContext();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateContext();
        }

        private void OnSourceUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateContext();
        }

        private void UpdateContext ()
        {
            NamesListBox.ItemsSource = ProgramData.DataSources.Select(x => x.Name);
            SourceDetails.DataContext = ProgramData.DataSources[NamesListBox.SelectedIndex] ?? new Database.DataSource();
        }
    }
}
