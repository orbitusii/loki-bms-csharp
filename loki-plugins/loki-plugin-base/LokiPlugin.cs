namespace loki_plugin_base
{
    public abstract class LokiPlugin
    {
        public abstract PluginManifest Detect();

        public abstract void Init();

        public Type[] DataSourceTypes = Array.Empty<Type>();
        public Type[] CustomMenuTypes = Array.Empty<Type>();
    }

    public struct PluginManifest
    {
        public string Name;
        public string Description;
        public string Version;
        public string DesiredLokiVersion;

        public PluginManifest(string Name, string Description, string Version, string DesiredLokiVersion)
        {
            this.Name = Name;
            this.Description = Description;
            this.Version = Version;
            this.DesiredLokiVersion = DesiredLokiVersion;
        }
    }
}