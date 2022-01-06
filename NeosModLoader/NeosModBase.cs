using System.Collections.Generic;

namespace NeosModLoader
{
    // contains public metadata about the mod
    public abstract class NeosModBase
    {
        public abstract string Name { get; }
        public abstract string Author { get; }
        public abstract string Version { get; }
        public virtual string Link { get; }

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
