﻿using System;
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
            int fallbackIndex = NamesListBox.SelectedIndex;
            UpdateContext(fallbackIndex);
        }

        private void UpdateContext (int fallbackIndex = 0)
        {
            NamesListBox.ItemsSource = ProgramData.DataSources.Select(x => x.Name);
            try
            {
                SourceDetails.DataContext = ProgramData.DataSources[NamesListBox.SelectedIndex] ?? new Database.DataSource();
            }
            catch
            {
                NamesListBox.SelectedIndex = fallbackIndex;
                SourceDetails.DataContext = ProgramData.DataSources[fallbackIndex];
            }
        }

        private void CheckBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            BlockableDetails.GetBindingExpression(IsEnabledProperty).UpdateTarget();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ProgramData.SrcWindow = null;
        }
    }
}