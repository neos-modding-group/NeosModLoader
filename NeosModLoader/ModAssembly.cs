using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
