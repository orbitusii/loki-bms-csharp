using loki_bms_common.Database;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace loki_bms_csharp.Windows.Controls
{
    /// <summary>
    /// Interaction logic for RightClickMenu.xaml
    /// </summary>
    public partial class ContextMenu : UserControl
    {
        internal Window ParentWindow;
        internal Vector64 WorldClickPoint;
        TacticalElement? SelectedTE;
        TrackFile? SelectedTrack;

        public ContextMenu()
        {
            InitializeComponent();
        }

        public void Reinit (Vector64 worldPos)
        {
            this.DataContext = ProgramData.SelectedObject;
            SelectedTE = null;
            SelectedTrack = null;
            WorldClickPoint = worldPos;

            if (DataContext is TacticalElement te) SelectedTE = te;
            else if (DataContext is TrackFile tf) SelectedTrack = tf;
        }

        private void AddBullseye_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTE is not null && !ProgramData.Bullseyes.Contains(SelectedTE))
            {
                Dispatcher.Invoke(() =>
                {
                    ProgramData.Bullseyes.Add(SelectedTE);
                });
            }

            ParentWindow.Hide();
        }

        private void RemoveBullseye_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedTE is not null)
            {
                Dispatcher.Invoke(() =>
                {
                    ProgramData.Bullseyes.Remove(SelectedTE);
                });
            }

            ParentWindow.Hide();
        }

        private void CreateTEButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTE is null)
            {
                TacticalElement newTE = new TacticalElement
                {
                    Category = TEType.None,
                    FFS = FriendFoeStatus.Pending,
                    Name = "New TE",
                    Position = WorldClickPoint,
                    Radius = 0,
                    Source = null,
                    SpecialInfo = "User created",
                };

                Dispatcher.Invoke(() =>
                {
                    ProgramData.Database.TEs.Add(newTE);
                });
            }

            ParentWindow.Hide();
        }

        private void DeleteTEButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTE is not null)
            {
                Dispatcher.Invoke(() =>
                {
                    ProgramData.Database.MarkForDeletion(SelectedTE);
                    ProgramData.Bullseyes.Remove(SelectedTE);
                });

                ProgramData.SelectedObject = null;
            }

            ParentWindow.Hide();
        }
    }
}
