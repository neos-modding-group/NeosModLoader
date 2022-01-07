using System.Collections.Generic;

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
        public virtual string Link { get; }

        /// <returns>This mod's current configuration. This method will always return the same ModConfiguration instance.</returns>
        public ModConfiguration GetConfiguration()
        {
            return ModLoader.ModBaseLookupMap[this].ModConfiguration;
        }

        public override bool Equals(object obj)
        {
            return obj is NeosModBase @base &&
                   Name == @base.Name &&
                   Author == @base.Author &&
                   Version == @base.Version;
        }

        public override int GetHashCode()
        {
            int hashCode = 1366530947;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Author);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Version);
            return hashCode;
        }
    }
}
