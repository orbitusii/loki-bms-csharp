﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loki_plugin_base
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class LokiPluginAttribute : System.Attribute
    {
        public Type? RootType;
        public string PluginName = string.Empty;
        public string Description = string.Empty;
        public string Version = "0.0.0";

        public string Author = "Unknown";

        public LokiVersion DesiredLokiVersion = LokiVersion.Any;

        public Type[]? DataSourceTypes;
        public Type[]? CustomMenuTypes;

        public LokiPluginAttribute(Type RootType,
                                   string PluginName,
                                   string Description,
                                   string Version)
        {
            this.RootType = RootType;
            this.PluginName = PluginName;
            this.Description = Description;
            this.Version = Version;
        }
    }

    [Flags]
    public enum LokiVersion
    {
        Any = 0,
        v0_2_0 = 2,
    }
}
