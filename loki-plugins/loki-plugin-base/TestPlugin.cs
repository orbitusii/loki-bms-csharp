using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_plugin_base
{
    public class TestPlugin : LokiPlugin
    {
        public override PluginManifest Detect()
        {
            return new PluginManifest("Test Plugin", "A test plugin for LOKI's Plugin loader system", "0.0.0", "Any");
        }

        public override void Init()
        {
            Debug.WriteLine("Test Plugin Initialized");
        }
    }
}
