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
using loki_bms_common.Database;

namespace loki_bms_csharp.Windows
{
    /// <summary>
    /// Interaction logic for TrackDetails.xaml
    /// </summary>
    public partial class TrackDetails : UserControl
    {
        public TrackDetails()
        {
            InitializeComponent();

            BeginInit();

            IDSelection.ItemsSource = Enum.GetValues(typeof(FriendFoeStatus)).Cast<FriendFoeStatus>();
            SpecTypeSelection.ItemsSource = ProgramData.SpecTypeSymbols.Keys;

            EndInit();
        }

        private void IDSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
