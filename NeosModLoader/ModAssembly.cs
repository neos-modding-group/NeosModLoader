using System.Reflection;

namespace NeosModLoader
{
    internal class ModAssembly
    {
        public string File { get; }
        public Assembly Assembly { get; set; }
        public ModAssembly(string file)
        {
            File = file;
        }
    }
}
