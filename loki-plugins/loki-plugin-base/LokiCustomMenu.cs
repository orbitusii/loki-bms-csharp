using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_plugin_base
{
    public abstract class LokiCustomMenu
    {
        public string XamlSourcePath = string.Empty;
        public object? GeneratedElements;
        public string ButtonText = "???";
    }
}
