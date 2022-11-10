using loki_bms_csharp.Database;
using loki_bms_csharp.UserInterface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace loki_bms_csharp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            System.Diagnostics.Debug.WriteLine("Initializing App stuff...");
            TrackNumber.Test();
            ProgramData.Initialize();
            TrackDatabase.Initialize(1000);
        }
    }
}
