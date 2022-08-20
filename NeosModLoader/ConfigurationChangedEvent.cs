namespace NeosModLoader
{
    public class ConfigurationChangedEvent
    {
        /// <summary>
        /// The configuration the change occurred in
        /// </summary>
        public ModConfiguration Config { get; private set; }

        /// <summary>
        /// The specific key who's value changed
        /// </summary>
        public ModConfigurationKey Key { get; private set; }

        /// <summary>
        /// A custom label that may be set by whoever changed the configuration
        /// </summary>
        public string? Label { get; private set; }

        internal ConfigurationChangedEvent(ModConfiguration config, ModConfigurationKey key, string? label)
        {
            Config = config;
            Key = key;
            Label = label;
        }
    }
}
