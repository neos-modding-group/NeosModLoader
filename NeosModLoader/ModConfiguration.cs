using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        private List<ModConfigurationKey> configurationItemDefinitions;
        public List<ModConfigurationKey> ConfigurationItemDefinitions
        {
            // clone the list because I don't trust giving public API users shallow copies one bit
            get { return new List<ModConfigurationKey>(configurationItemDefinitions); }
            private set { configurationItemDefinitions = value; }
        }

        internal ModConfigurationDefinition(NeosModBase owner, Version version, List<ModConfigurationKey> configurationItemDefinitions)
        {
            Owner = owner;
            Version = version;
            ConfigurationItemDefinitions = configurationItemDefinitions;
        }
    }

    public class ModConfiguration : IModConfiguration
    {
        private ModConfigurationDefinition Definition;
        public Dictionary<ModConfigurationKey, object> Values { get; private set; }
        internal LoadedNeosMod LoadedNeosMod { get; private set; }

        private static string ConfigDirectory = Path.Combine(Directory.GetCurrentDirectory(), "nml_config");
        private static readonly string VERSION_JSON_KEY = "version";
        private static readonly string VALUES_JSON_KEY = "values";

        public NeosModBase Owner => Definition.Owner;
        public Version Version => Definition.Version;
        public List<ModConfigurationKey> ConfigurationItemDefinitions => Definition.ConfigurationItemDefinitions;

        private ModConfiguration(LoadedNeosMod loadedNeosMod, ModConfigurationDefinition definition, Dictionary<ModConfigurationKey, object> values)
        {
            LoadedNeosMod = loadedNeosMod;
            Definition = definition;
            Values = values;
        }

        internal static void EnsureDirectoryExists()
        {
            Directory.CreateDirectory(ConfigDirectory);
        }

        internal static string GetModConfigPath(LoadedNeosMod mod)
        {

            string filename = Path.ChangeExtension(Path.GetFileName(mod.ModAssembly.File), ".json");
            return Path.Combine(ConfigDirectory, filename);
        }

        private static bool AreVersionsCompatible(Version serializedVersion, Version currentVersion)
        {
            if (serializedVersion.Major != currentVersion.Major)
            {
                // major version differences are hard incompatible
                return false;
            }

            if (serializedVersion.Minor > currentVersion.Minor)
            {
                // if serialized config has a newer minor version than us
                // in other words, someone downgraded the mod but not the config
                // then we cannot load the config
                return false;
            }

            return true;
        }

        internal static ModConfiguration LoadConfigForMod(LoadedNeosMod mod)
        {
            ModConfigurationDefinition definition = mod.NeosMod.GetConfigurationDefinition();
            if (definition == null)
            {
                // if there's no definition, then there's nothing for us to do here
                return null;
            }

            Dictionary<ModConfigurationKey, object> values = new Dictionary<ModConfigurationKey, object>();
            string configFile = GetModConfigPath(mod);

            try
            {
                using (StreamReader file = File.OpenText(configFile))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject json = JObject.Load(reader);
                        Version version = json[VERSION_JSON_KEY].ToObject<Version>();
                        if (!AreVersionsCompatible(version, definition.Version))
                        {
                            mod.AllowSavingConfiguration = false;
                            throw new ModConfigurationException($"{mod.NeosMod.Name} saved config version is {version} which is incompatible with mod's definition version {definition.Version}");
                        }
                        foreach (ModConfigurationKey key in definition.ConfigurationItemDefinitions)
                        {
                            object value = json[VALUES_JSON_KEY][key.Name].ToObject(key.ValueType());
                            values.Add(key, value);
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // return early
                return new ModConfiguration(mod, definition, values);
            }
            catch (Exception e)
            {
                // I know not what exceptions the JSON library will throw, but they must be contained
                throw new ModConfigurationException($"Error loading config for {mod.NeosMod.Name}", e);
            }

            return new ModConfiguration(mod, definition, values);
        }

        internal void Save()
        {
            // prevent saving if we've determined something is amiss with the configuration
            if (!LoadedNeosMod.AllowSavingConfiguration)
            {
                Logger.WarnInternal($"Config for {LoadedNeosMod.NeosMod.Name} will NOT be saved due to a safety check failing. This is probably due to you downgrading a mod.");
                return;
            }
            ModConfigurationDefinition definition = LoadedNeosMod.NeosMod.GetConfigurationDefinition();

            JObject json = new JObject();
            json[VERSION_JSON_KEY] = JToken.FromObject(definition.Version);

            JObject valueMap = new JObject();
            foreach (KeyValuePair<ModConfigurationKey, object> entry in Values)
            {
                // make sure no one snuck an incorrect type into our value object
                if (!entry.Key.ValueType().IsAssignableFrom(entry.Value.GetType()))
                {
                    throw new ModConfigurationException($"{LoadedNeosMod.NeosMod.Name} config {entry.Key.Name} has type {entry.Key.ValueType()}, but a {entry.Value.GetType()} cannot be assigned to that.");
                }
                valueMap[entry.Key.Name] = JToken.FromObject(entry.Value);
            }

            json[VALUES_JSON_KEY] = valueMap;

            string configFile = GetModConfigPath(LoadedNeosMod);
            using (FileStream file = File.OpenWrite(configFile))
            {
                using (StreamWriter streamWriter = new StreamWriter(file))
                {
                    using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
                    {
                        json.WriteTo(jsonTextWriter);
                    }
                }
                // I actually cannot believe I have to truncate the file myself
                file.SetLength(file.Position);
            }
        }
    }

    internal class ModConfigurationException : Exception
    {
        public ModConfigurationException(string message) : base(message)
        {
        }

        public ModConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
