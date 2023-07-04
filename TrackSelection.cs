using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using loki_bms_common.Database;

namespace loki_bms_csharp
{
    public class TrackSelection : INotifyPropertyChanged, IDisposable
    {
        private TrackFile? _track;
        public TrackFile? Track
        {
            get => _track;
            set
            {
                _track = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Track)));
            }
        }

        public TrackSelection() {
            ProgramData.SelectionChanged += SelectionChanged;
        }

        public void SelectionChanged(object sender, SelectionChangedArgs args)
        {
            Track = (TrackFile)args.NewSelection;
        }

        public void Dispose ()
        {
            ProgramData.SelectionChanged -= SelectionChanged;
            GC.SuppressFinalize(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
