using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using loki_bms_common;
using System.Diagnostics;

namespace loki_bms_csharp.Plugins
{
    internal class PluginLoader
    {
        public string PluginFolder = Path.Join(".", "Plugins");
        public LokiVersion Version;
        public Dictionary<LokiPlugin, LokiPluginAttribute> LoadedPlugins = new Dictionary<LokiPlugin, LokiPluginAttribute>();

        public PluginLoader(LokiVersion Version)
        {
            this.Version = Version;
        }

        public PluginLoader(string PluginFolder, LokiVersion Version)
        {
            this.PluginFolder = PluginFolder;
            this.Version = Version;
        }

        public void LoadPlugins()
        {
            if(!Directory.Exists(PluginFolder))
            {
                Directory.CreateDirectory(PluginFolder);
                Debug.WriteLine("[PLUGINS][LOG] Plugins folder did not exist. Created it, no plugins loaded.");
                return;
            }

            var files = Directory.GetFiles(PluginFolder);

            foreach (var file in files)
            {
                if (!file.EndsWith(".dll"))
                    continue;

                var assembly = Assembly.LoadFrom(file);

                if (assembly.GetCustomAttribute(typeof(LokiPluginAttribute)) is LokiPluginAttribute lpa)
                {
                    if ((lpa.DesiredLokiVersion & Version) != Version && lpa.DesiredLokiVersion != LokiVersion.Any)
                        continue;

                    LokiPlugin? Root = (LokiPlugin)Activator.CreateInstance(lpa.RootType);
                    if (Root is null)
                        continue;

                    Root.Init();
                    LoadedPlugins.Add(Root, lpa);

                    Debug.WriteLine($"[PLUGINS][LOG] Loaded plugin \"{lpa.PluginName}\", version {lpa.Version} by {lpa.Author}");
                }
            }
        }
    }
}
