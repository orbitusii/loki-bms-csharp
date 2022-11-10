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

            IDSelection.ItemsSource = Enum.GetValues(typeof(Database.FriendFoeStatus)).Cast<Database.FriendFoeStatus>();
            SpecTypeSelection.ItemsSource = ProgramData.SpecTypeSymbols.Keys;

            EndInit();
        }
    }
}
