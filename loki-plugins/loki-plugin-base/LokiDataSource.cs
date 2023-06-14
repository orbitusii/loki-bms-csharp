using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_plugin_base
{
    public abstract class LokiDataSource
    {
        public abstract void Activate();
        public abstract void Deactivate();

        public abstract bool CheckAlive();

    }
}
