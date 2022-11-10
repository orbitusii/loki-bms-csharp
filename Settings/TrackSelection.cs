using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using loki_bms_csharp.Database;

namespace loki_bms_csharp.Settings
{
    public class TrackSelection: INotifyPropertyChanged
    {
        private TrackFile _track;
        public TrackFile Track
        {
            get => _track;
            set
            {
                _track = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Track)));
            }
        }

        public TrackSelection() { }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
