using System.Reflection;

namespace NeosModLoader
{
    internal class ModAssembly
    {
        internal string File { get; }
        internal Assembly Assembly { get; set; }
        internal ModAssembly(string file)
        {
            File = file;
        }
    }
}
