using System;
using System.Collections.Generic;
using System.IO;

namespace NeosModLoader
{
    internal interface IModConfiguration
    {
        NeosModBase Owner { get; }
        Version Version { get; }
        List<ModConfigurationKey> ConfigurationItemDefinitions { get; }
    }

    public class ModConfigurationDefinition : IModConfiguration
    {
        public NeosModBase Owner { get; private set; }
        public Version Version { get; private set; }
        //TODO: clone the list because I don't trust giving API users shallow copies one bit

        private List<ModConfigurationKey> configurationItemDefinitions;
        public List<ModConfigurationKey> ConfigurationItemDefinitions
        {
            get { return configurationItemDefinitions; }
            private set { configurationItemDefinitions = value; } 
        }

        internal ModConfigurationDefinition(NeosModBase owner, Version version, List<ModConfigurationKey> configurationItemDefinitions)
        {
            Owner = owner;
            Version = version;
            ConfigurationItemDefinitions = configurationItemDefinitions;
        }

        internal ModConfigurationDefinition Load()
        {
            //TODO
            throw new NotImplementedException();
        }
    }

    public class ModConfiguration : IModConfiguration
    {
        private ModConfigurationDefinition Definition;
        Dictionary<ModConfigurationKey, object> Values;

        internal void Save()
        {
            //TODO
            throw new NotImplementedException();
        }

        private static string ConfigDirectory = Path.Combine(Directory.GetCurrentDirectory(), "nml_config");

        public NeosModBase Owner => Definition.Owner;
        public Version Version => Definition.Version;
        public List<ModConfigurationKey> ConfigurationItemDefinitions => Definition.ConfigurationItemDefinitions;

        internal static void EnsureDirectoryExists()
        {
            Directory.CreateDirectory(ConfigDirectory);
        }

        internal static ModConfiguration LoadConfigForMod(LoadedNeosMod mod)
        {
            string configName = GetModConfigName(mod);
            string configFile = Path.Combine(ConfigDirectory, configName);
            Logger.DebugInternal($"About to load config from {configFile} for filename {configName}");

            //TODO
            return null;
        }

        internal static string GetModConfigName(LoadedNeosMod mod)
        {
            return Path.ChangeExtension(Path.GetFileName(mod.ModAssembly.File), ".json");
        }
    }
}
