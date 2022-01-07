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
        HashSet<ModConfigurationKey> ConfigurationItemDefinitions { get; }
    }

    /// <summary>
    /// Defines a mod configuration. This should be defined by a NeosMod using the NeosMod.DefineConfiguration() method.
    /// </summary>
    public class ModConfigurationDefinition : IModConfiguration
    {
        /// <summary>
        /// Mod that owns this configuration definition
        /// </summary>
        public NeosModBase Owner { get; private set; }

        /// <summary>
        /// Semantic version for this configuration definition. This is used to check if the defined and saved configs are compatible
        /// </summary>
        public Version Version { get; private set; }

        private HashSet<ModConfigurationKey> configurationItemDefinitions;

        /// <summary>
        /// The set of coniguration keys defined in this configuration definition
        /// </summary>
        public HashSet<ModConfigurationKey> ConfigurationItemDefinitions
        {
            // clone the collection because I don't trust giving public API users shallow copies one bit
            get { return new HashSet<ModConfigurationKey>(configurationItemDefinitions); }
            private set { configurationItemDefinitions = value; }
        }

        internal ModConfigurationDefinition(NeosModBase owner, Version version, HashSet<ModConfigurationKey> configurationItemDefinitions)
        {
            Owner = owner;
            Version = version;
            ConfigurationItemDefinitions = configurationItemDefinitions;
        }
    }

    /// <summary>
    /// The configuration for a mod. Each mod has zero or one configuration. The configuration object will never be reassigned once initialized.
    /// </summary>
    public class ModConfiguration : IModConfiguration
    {
        private ModConfigurationDefinition Definition;
        internal Dictionary<ModConfigurationKey, object> Values { get; private set; }
        internal LoadedNeosMod LoadedNeosMod { get; private set; }

        private static string ConfigDirectory = Path.Combine(Directory.GetCurrentDirectory(), "nml_config");
        private static readonly string VERSION_JSON_KEY = "version";
        private static readonly string VALUES_JSON_KEY = "values";

        /// <summary>
        /// Mod that owns this configuration definition
        /// </summary>
        public NeosModBase Owner => Definition.Owner;

        /// <summary>
        /// Semantic version for this configuration definition. This is used to check if the defined and saved configs are compatible
        /// </summary>
        public Version Version => Definition.Version;

        /// <summary>
        /// The set of coniguration keys defined in this configuration definition
        /// </summary>
        public HashSet<ModConfigurationKey> ConfigurationItemDefinitions => Definition.ConfigurationItemDefinitions;

        /// <summary>
        /// The delegate that is called for configuration change events.
        /// </summary>
        /// <param name="configurationChangedEvent">The event containing details about the configuration change</param>
        public delegate void ConfigurationChangedEventHandler(ConfigurationChangedEvent configurationChangedEvent);

        /// <summary>
        /// Called if any config value for any mod changed.
        /// </summary>
        public static event ConfigurationChangedEventHandler OnAnyConfigurationChanged;

        /// <summary>
        /// Called if one of the values in this mod's config changed.
        /// </summary>
        public event ConfigurationChangedEventHandler OnThisConfigurationChanged;

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

        /// <summary>
        /// Check if a key is defined in this config
        /// </summary>
        /// <param name="key">the key to check</param>
        /// <returns>true if the key is defined</returns>
        public bool IsKeyDefined(ModConfigurationKey key)
        {
            return ConfigurationItemDefinitions.Contains(key);
        }

        /// <summary>
        /// Get a value, throwing an exception if the key is not found
        /// </summary>
        /// <param name="key">The key to find</param>
        /// <returns>The found value</returns>
        /// <exception cref="KeyNotFoundException">key does not exist in the collection</exception>
        public object GetValue(ModConfigurationKey key)
        {
            if (IsKeyDefined(key) && TryGetValue(key, out object value))
            {
                return value;
            }
            else
            {
                throw new KeyNotFoundException($"{key.Name} not found in {LoadedNeosMod.NeosMod.Name} configuration");
            }
        }

        /// <summary>
        /// Get a value, throwing an exception if the key is not found
        /// </summary>
        /// <typeparam name="T">The value's type</typeparam>
        /// <param name="key">The key to find</param>
        /// <returns>The found value</returns>
        /// <exception cref="KeyNotFoundException">key does not exist in the collection</exception>
        public T GetValue<T>(ModConfigurationKey<T> key)
        {
            if (IsKeyDefined(key) && TryGetValue(key, out T value))
            {
                return value;
            }
            else
            {
                throw new KeyNotFoundException($"{key.Name} not found in {LoadedNeosMod.NeosMod.Name} configuration");
            }
        }

        /// <summary>
        /// Try to read a configuration value
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value if we succeeded, or null if we failed.</param>
        /// <returns>true if the value was read successfully</returns>
        public bool TryGetValue(ModConfigurationKey key, out object value)
        {
            if (!IsKeyDefined(key))
            {
                value = null;
                return false;
            }

            if (Values.TryGetValue(key, out object valueObject))
            {
                value = valueObject;
                return true;
            }
            else if (key.TryComputeDefault(out value))
            {
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Try to read a typed configuration value
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="key">The key</param>
        /// <param name="value">The value if we succeeded, or default(T) if we failed.</param>
        /// <returns>true if the value was read successfully</returns>
        public bool TryGetValue<T>(ModConfigurationKey<T> key, out T value)
        {
            if (!IsKeyDefined(key))
            {
                value = default(T);
                return false;
            }

            if (Values.TryGetValue(key, out object valueObject))
            {
                value = (T)valueObject;
                return true;
            }
            else if (key.TryComputeDefaultTyped(out value))
            {
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        /// <summary>
        /// Set a configuration value
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The new value</param>
        /// <param name="eventLabel">A custom label you may assign to this event</param>
        public void Set(ModConfigurationKey key, object value, string eventLabel = null)
        {
            if (!IsKeyDefined(key))
            {
                throw new KeyNotFoundException($"{key.Name} is not defined in the config definition for {LoadedNeosMod.NeosMod.Name}");
            }

            if (!key.Validate(value))
            {
                throw new ArgumentException($"\"{value}\" is not a valid value for \"{Owner.Name}{key.Name}\"");
            }

            if (key.ValueType().IsAssignableFrom(value.GetType()))
            {
                Values[key] = value;
                FireConfigurationChangedEvent(key, eventLabel);
            }
            else
            {
                throw new ArgumentException($"{value.GetType()} cannot be assigned to {key.ValueType()}");
            }
        }


        /// <summary>
        /// Set a typed configuration value
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="key">The key</param>
        /// <param name="value">The new value</param>
        /// <param name="eventLabel">A custom label you may assign to this event</param>
        public void Set<T>(ModConfigurationKey<T> key, T value, string eventLabel = null)
        {
            if (!IsKeyDefined(key))
            {
                throw new KeyNotFoundException($"{key.Name} is not defined in the config definition for {LoadedNeosMod.NeosMod.Name}");
            }

            if (!key.ValidateTyped(value))
            {
                throw new ArgumentException($"\"{value}\" is not a valid value for \"{Owner.Name}{key.Name}\"");
            }

            Values[key] = value;
            FireConfigurationChangedEvent(key, eventLabel);
        }

        /// <summary>
        /// Removes a configuration value, if set
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if a value was successfully found and removed, false if there was no value to remove</returns>
        public bool Unset(ModConfigurationKey key)
        {
            if (!IsKeyDefined(key))
            {
                throw new KeyNotFoundException($"{key.Name} is not defined in the config definition for {LoadedNeosMod.NeosMod.Name}");
            }
            else
            {
                return Values.Remove(key);
            }
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
                        Version version = new Version(json[VERSION_JSON_KEY].ToObject<string>());
                        if (!AreVersionsCompatible(version, definition.Version))
                        {
                            var handlingMode = mod.NeosMod.HandleIncompatibleConfigurationVersions(definition.Version, version);
                            switch (handlingMode)
                            {
                                case IncompatibleConfigurationHandlingOption.CLOBBER:
                                    Logger.WarnInternal($"{mod.NeosMod.Name} saved config version is {version} which is incompatible with mod's definition version {definition.Version}. Clobbering old config and starting fresh.");
                                    return new ModConfiguration(mod, definition, values);
                                case IncompatibleConfigurationHandlingOption.FORCE_LOAD:
                                    // continue processing
                                    break;
                                case IncompatibleConfigurationHandlingOption.ERROR:
                                    // fall through to default
                                default:
                                    mod.AllowSavingConfiguration = false;
                                    throw new ModConfigurationException($"{mod.NeosMod.Name} saved config version is {version} which is incompatible with mod's definition version {definition.Version}");
                            }
                        }
                        foreach (ModConfigurationKey key in definition.ConfigurationItemDefinitions)
                        {
                            JToken token = json[VALUES_JSON_KEY][key.Name];
                            if (token != null)
                            {
                                object value = token.ToObject(key.ValueType());
                                values.Add(key, value);
                            }
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
                mod.AllowSavingConfiguration = false;
                throw new ModConfigurationException($"Error loading config for {mod.NeosMod.Name}", e);
            }

            return new ModConfiguration(mod, definition, values);
        }

        /// <summary>
        /// Persist this configuration to disk. This method is not called automatically.
        /// </summary>
        public void Save()
        {
            // prevent saving if we've determined something is amiss with the configuration
            if (!LoadedNeosMod.AllowSavingConfiguration)
            {
                Logger.WarnInternal($"Config for {LoadedNeosMod.NeosMod.Name} will NOT be saved due to a safety check failing. This is probably due to you downgrading a mod.");
                return;
            }
            ModConfigurationDefinition definition = LoadedNeosMod.NeosMod.GetConfigurationDefinition();

            JObject json = new JObject();
            json[VERSION_JSON_KEY] = JToken.FromObject(definition.Version.ToString());

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

                        // I actually cannot believe I have to truncate the file myself
                        file.SetLength(file.Position);
                    }
                }
            }
        }

        private void FireConfigurationChangedEvent(ModConfigurationKey key, string label)
        {
            try
            {
                OnAnyConfigurationChanged?.SafeInvoke(new ConfigurationChangedEvent(this, key, label));
            }
            catch (Exception e)
            {
                Logger.ErrorInternal($"An OnAnyConfigurationChanged event subscriber threw an exception:\n{e}");
            }

            try
            {
                OnThisConfigurationChanged?.SafeInvoke(new ConfigurationChangedEvent(this, key, label));
            }
            catch (Exception e)
            {
                Logger.ErrorInternal($"An OnThisConfigurationChanged event subscriber threw an exception:\n{e}");
            }
        }
    }

    internal class ModConfigurationException : Exception
    {
        internal ModConfigurationException(string message) : base(message)
        {
        }

        internal ModConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Defines handling of incompatible configuration versions
    /// </summary>
    public enum IncompatibleConfigurationHandlingOption
    {
        /// <summary>
        /// Fail to read the config, and block saving over the config on disk.
        /// </summary>
        ERROR,

        /// <summary>
        /// Destroy the saved config and start over from scratch.
        /// </summary>
        CLOBBER,

        /// <summary>
        /// Ignore the version number and attempt to load the config from disk
        /// </summary>
        FORCE_LOAD,
    }
}
