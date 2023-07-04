using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using loki_bms_common.Database;

namespace loki_bms_csharp
{
    /// <summary>
    /// Interaction logic for SourcesWindow.xaml
    /// </summary>
    public partial class SourceWindow : Window
    {
        TrackDatabase DB => (TrackDatabase)DataContext;
        LokiDataSource? selectedSource => (LokiDataSource)SourcesListBox.SelectedItem;

        private bool SelectingType = false;

        public SourceWindow()
        {
            InitializeComponent();
        }

        private void SourcesWin_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = ProgramData.Database;
            SourcesListBox.SelectedIndex = 0;
            TypesBox.Visibility = Visibility.Collapsed;
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
            TypesBox.Visibility= Visibility.Visible;
            TypesBox.SelectedIndex = -1;
            SelectingType = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                // TODO: add support for DataSource factory methods of some sort
                //DB.DataSources.Add(new LokiDataSource());
            }), System.Windows.Threading.DispatcherPriority.Normal);
        }

        private void ClickRemoveSourceButton(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (SourcesListBox.SelectedIndex < 0 || SourcesListBox.SelectedIndex >= DB.DataSources.Count) return;

                DB.DataSources.RemoveAt(SourcesListBox.SelectedIndex);
            }), System.Windows.Threading.DispatcherPriority.Normal);
        }

        private void Bullseye_Select(object sender, RoutedEventArgs e)
        {
            LokiDataSource? ds = (LokiDataSource)SourceDetails.DataContext;

            if (ds is null || ds.Status != LokiDataSource.SourceStatus.Active) return;

            Task.Run(() =>
            {
                ImportTEs(ds);
            });
        }

        private void ImportTEs (LokiDataSource ds)
        {
            TacticalElement[] TEs = ds.GetTEs();

            foreach ( var te in TEs )
            {
                DB.TEs.Add(te);
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            LokiDataSource ds = (LokiDataSource)SourceDetails.DataContext;
            //ds.PauseUnpause();
        }

        private void ClickSourceType(object sender, SelectionChangedEventArgs e)
        {
            if (!SelectingType) return;

            TypesBox.Visibility = Visibility.Collapsed;
            SelectingType = false;

            Type sourceType = ProgramData.PluginLoader.DataSourceTypes[((Type)TypesBox.SelectedValue).Name];
            LokiDataSource? newSource = (LokiDataSource)Activator.CreateInstance(sourceType);

            if (newSource is null)
            {
                Debug.WriteLine($"[DATABASE][ERROR] Unable to create a Data Source of type \"{sourceType.Name}\"");
                return;
            }

            DB.DataSources.Add(newSource);
        }

        private void ActivateDeactivateButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSource is null) return;

            if (selectedSource.Status == LokiDataSource.SourceStatus.Starting) return;
            else if(selectedSource.Status == LokiDataSource.SourceStatus.Active)
            {
                selectedSource.Deactivate();
            }
            else
            {
                selectedSource.Activate();
            }
        }
    }
}
