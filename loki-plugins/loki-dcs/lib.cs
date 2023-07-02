global using loki_bms_common;
using loki_dcs;
using loki_dcs.Services;

[assembly: LokiPlugin(
    RootType = typeof(LokiDCSPlugin),
    PluginName = "LOKI for DCS",
    Version = "0.0.1",
    DesiredLokiVersion = LokiVersion.Any,
    Author = "Dani Lodholm",
    Description = "A plugin for LOKI that interfaces with DCS World",
    DataSourceTypes = new Type[] { typeof(DCSSource) },
    CustomMenuTypes = new Type[] { })]

namespace loki_dcs
{
    public class LokiDCSPlugin : LokiPlugin
    {
        public LokiDCSPlugin() { }

        public override void Init()
        {
            throw new NotImplementedException();
        }
    }
}