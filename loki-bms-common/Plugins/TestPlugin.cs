using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loki_bms_common.Plugins;

[assembly: LokiPlugin(typeof(TestPlugin), "Test Plugin", "999.0.0", Author = "Dani Lodholm")]
namespace loki_bms_common.Plugins
{
    public class TestPlugin : LokiPlugin
    {
        public override void Init()
        {
            Debug.WriteLine("Test Plugin Initialized");
        }
    }
}
