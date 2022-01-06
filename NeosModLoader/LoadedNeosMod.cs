using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeosModLoader
{
    internal class LoadedNeosMod
    {
        internal LoadedNeosMod(NeosMod neosMod, ModAssembly modAssembly)
        {
            NeosMod = neosMod;
            ModAssembly = modAssembly;
        }

        internal NeosMod NeosMod { get; private set; }
        internal ModAssembly ModAssembly { get; private set; }
        internal ModConfiguration ModConfiguration { get; set; }
    }
}
