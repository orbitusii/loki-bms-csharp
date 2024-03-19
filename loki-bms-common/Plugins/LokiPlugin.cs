namespace loki_bms_common.Plugins
{
    /// <summary>
    /// The root class for Loki plugins, to be used for plugin-level behavior and functionality.
    /// Do not implement DataSource or Custom Menu behavior here! This should only be used for
    /// initialization or behaviors that must be implemented as a singleton.
    /// </summary>
    public abstract class LokiPlugin
    {
        /// <summary>
        /// Plugin-level initializer. This is called as the application is starting up.
        /// In most cases, you should not need to implement anything here.
        /// </summary>
        public abstract void Init();
    }
}