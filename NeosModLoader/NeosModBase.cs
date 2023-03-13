namespace NeosModLoader
{
	/// <summary>
	/// Contains public metadata about a mod.
	/// </summary>
	public abstract class NeosModBase
	{
		/// <summary>
		/// Gets the mod's name. This must be unique.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// Gets the mod's author.
		/// </summary>
		public abstract string Author { get; }

		/// <summary>
		/// Gets the mod's semantic version.
		/// </summary>
		public abstract string Version { get; }

		/// <summary>
		/// Gets an optional hyperlink to the mod's homepage.
		/// </summary>
		public virtual string? Link { get; }

		/// <summary>
		/// A circular reference back to the LoadedNeosMod that contains this NeosModBase.
		/// The reference is set once the mod is successfully loaded, and is null before that.
		/// </summary>
		internal LoadedNeosMod? loadedNeosMod;

		/// <summary>
		/// Gets this mod's current <see cref="ModConfiguration"/>.
		/// <para/>
		/// This will always be the same instance.
		/// </summary>
		/// <returns>This mod's current configuration.</returns>
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
