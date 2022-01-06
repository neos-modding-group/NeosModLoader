using System;
using System.Collections.Generic;

namespace NeosModLoader
{
    public class ModConfigurationDefinition
    {
        internal NeosMod Owner { get; set; }
        public Version Version { get; internal set; }
        //TODO: clone the list because I don't trust giving API users shallow copies one bit
        public List<ModConfigurationKey> ConfigurationItemDefinitions { get; internal set; }
        internal ModConfigurationDefinition Load()
        {
            //TODO
            throw new NotImplementedException();
        }
    }

    public class ModConfiguration : ModConfigurationDefinition
    {
        Dictionary<ModConfigurationKey, object> Values;

        internal void Save()
        {
            //TODO
            throw new NotImplementedException();
        }
    }
}
