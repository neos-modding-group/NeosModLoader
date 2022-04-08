using System.Reflection;

namespace NeosModLoader
{
    internal class AssemblyFile
    {
        internal string File { get; }
        internal Assembly Assembly { get; set; }
        internal AssemblyFile(string file, Assembly assembly)
        {
            File = file;
            Assembly = assembly;
        }
    }
}
