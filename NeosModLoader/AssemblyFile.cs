using System.Reflection;

namespace NeosModLoader
{
    internal class AssemblyFile
    {
        public static readonly AssemblyFileComparer AssemblyComparer = new AssemblyFileComparer();
        internal string File { get; }
        internal Assembly Assembly { get; set; }
        internal AssemblyFile(string file, Assembly assembly)
        {
            File = file;
            Assembly = assembly;
        }
    }

    // compares two AssemblyFile objects, based on whether they contain identical assemblies or not.
    class AssemblyFileComparer : System.Collections.Generic.IEqualityComparer<AssemblyFile>
    {
        public bool Equals(AssemblyFile x, AssemblyFile y) { return x.Assembly == y.Assembly; }

        public int GetHashCode(AssemblyFile obj) { return obj.Assembly.GetHashCode() ^ obj.Assembly.GetHashCode(); }
    }
}
