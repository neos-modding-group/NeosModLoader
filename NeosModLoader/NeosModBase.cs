namespace NeosModLoader
{
    // contains public metadata about the mod
    public abstract class NeosModBase
    {
        /// <summary>
        /// Name of the mod. This must be unique.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Mod's author.
        /// </summary>
        public abstract string Author { get; }

        /// <summary>
        /// Semantic version of this mod.
        /// </summary>
        public abstract string Version { get; }

        /// <summary>
        /// Optional hyperlink to this mod's homepage
        /// </summary>
        public virtual string? Link { get; }

        /// <summary>
        /// A circular reference back to the LoadedNeosMod that contains this NeosModBase.
        /// The reference is set once the mod is successfully loaded, and is null before that.
        /// </summary>
        internal LoadedNeosMod? loadedNeosMod;

        /// <returns>This mod's current configuration. This method will always return the same ModConfiguration instance.</returns>
        public ModConfiguration? GetConfiguration()
        {
            if (!FinishedLoading)
            {
                throw new ModConfigurationException($"GetConfiguration() was called before {Name} was done initializing. Consider calling GetConfiguration() from within OnEngineInit()");
            }
            return loadedNeosMod?.ModConfiguration;
        }

        internal bool FinishedLoading { get; set; }
    }
}
