using loki_bms_common.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_bms_common.Plugins
{
    /// <summary>
    /// Use this assembly attribute to mark your assembly as a Plugin that LOKI can load and utilize.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class LokiPluginAttribute : Attribute
    {
        /// <summary>
        /// A class that inherits from LokiPlugin, used for plugin-level initialization and utilities. You may not need to define one at all.
        /// </summary>
        public Type? RootType;
        /// <summary>
        /// The name of this plugin, which will be displayed within LOKI's UI.
        /// </summary>
        public string PluginName = string.Empty;
        public string Description = string.Empty;
        public string Version = "0.0.0";

        public string Author = "Unknown";

        /// <summary>
        /// This value is used to enforce a minimum version of LOKI BMS required for this plugin. If in doubt, use "Any."
        /// </summary>
        public LokiVersion DesiredLokiVersion = LokiVersion.Any;

        /// <summary>
        /// An array of classes that inherit from LokiDataSource and can be used to retrieve data for the Database. Types that do not derive from LokiDataSource will be ignored.
        /// </summary>
        public Type[]? DataSourceTypes;
        /// <summary>
        /// An array of classes that inherit from LokiCustomMenu and can be used as windows or menus in the Loki UI. Types that do not derive from LokiCustomMenu will be ignored.
        /// </summary>
        public Type[]? CustomMenuTypes;

        public LokiPluginAttribute() { }

        public LokiPluginAttribute(Type RootType,
                                   string PluginName,
                                   string Version)
        {
            this.RootType = RootType;
            this.PluginName = PluginName;
            this.Version = Version;
        }
    }

    /// <summary>
    /// An enum representing published versions of LOKI. Prior to 0.2.0, LOKI did not support plugins.
    /// </summary>
    [Flags]
    public enum LokiVersion
    {
        Any = 0,
        v0_2_0 = 2,
    }
}
