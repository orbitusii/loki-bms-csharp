using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_bms_common
{
    public abstract class LokiCustomMenu
    {
        public string XamlSourcePath = string.Empty;
        public object? GeneratedElements;
        public string ButtonText = "???";
    }
}
