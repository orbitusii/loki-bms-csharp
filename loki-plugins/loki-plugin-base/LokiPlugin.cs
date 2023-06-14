namespace loki_plugin_base
{
    public abstract class LokiPlugin
    {
        public abstract void Init();

        public Type[] DataSourceTypes = Array.Empty<Type>();
        public Type[] CustomMenuTypes = Array.Empty<Type>();
    }
}