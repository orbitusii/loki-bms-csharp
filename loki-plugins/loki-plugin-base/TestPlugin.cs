using loki_plugin_base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: LokiPlugin(typeof(TestPlugin), "Test Plugin", "Test Plugin", "999", Author = "Dani Lodholm")]
namespace loki_plugin_base
{
    public class TestPlugin : LokiPlugin
    {
        public override void Init()
        {
            Debug.WriteLine("Test Plugin Initialized");
        }
    }
}
