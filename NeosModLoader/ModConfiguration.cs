using FrooxEngine;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NeosModLoader
{
    public interface IModConfigurationDefinition
    {
        /// <summary>
        /// Mod that owns this configuration definition
        /// </summary>
        NeosModBase Owner { get; }

        /// <summary>
        /// Semantic version for this configuration definition. This is used to check if the defined and saved configs are compatible
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// The set of coniguration keys defined in this configuration definition
        /// </summary>
        ISet<ModConfigurationKey> ConfigurationItemDefinitions { get; }
    }

    /// <summary>
    /// Defines a mod configuration. This should be defined by a NeosMod using the NeosMod.DefineConfiguration() method.
    /// </summary>
    public class ModConfigurationDefinition : IModConfigurationDefinition
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

        internal bool AutoSave;

        // this is a ridiculous hack because HashSet.TryGetValue doesn't exist in .NET 4.6.2
        private Dictionary<ModConfigurationKey, ModConfigurationKey> configurationItemDefinitionsSelfMap;

        /// <summary>
        /// The set of coniguration keys defined in this configuration definition
        /// </summary>
        public ISet<ModConfigurationKey> ConfigurationItemDefinitions
        {
            // clone the collection because I don't trust giving public API users shallow copies one bit
            get => new HashSet<ModConfigurationKey>(configurationItemDefinitions);
            private set
            {
                if (value is HashSet<ModConfigurationKey> hashSet)
                {
                    configurationItemDefinitions = hashSet;
                }
                else
                {
                    configurationItemDefinitions = new HashSet<ModConfigurationKey>(value);
                }

                configurationItemDefinitionsSelfMap = new Dictionary<ModConfigurationKey, ModConfigurationKey>(configurationItemDefinitions.Count);
                foreach (ModConfigurationKey key in configurationItemDefinitions)
                {
                    key.DefiningKey = key; // early init this property for the defining key itself
                    configurationItemDefinitionsSelfMap.Add(key, key);
                }
            }
        }

        internal bool TryGetDefiningKey(ModConfigurationKey key, out ModConfigurationKey definingKey)
        {
            if (key.DefiningKey != null)
            {
                // we've already cached the defining key
                definingKey = key.DefiningKey;
                return true;
            }

            // first time we've seen this key instance: we need to hit the map
            if (configurationItemDefinitionsSelfMap.TryGetValue(key, out definingKey))
            {
                // initialize the cache for this key
                key.DefiningKey = definingKey;
                return true;
            }
            else
            {
                // not a real key
                definingKey = null;
                return false;
            }

        }

        internal ModConfigurationDefinition(NeosModBase owner, Version version, HashSet<ModConfigurationKey> configurationItemDefinitions, bool autoSave)
        {
            Owner = owner;
            Version = version;
            ConfigurationItemDefinitions = configurationItemDefinitions;
            AutoSave = autoSave;
        }
    }

    /// <summary>
    /// The configuration for a mod. Each mod has zero or one configuration. The configuration object will never be reassigned once initialized.
    /// </summary>
    public class ModConfiguration : IModConfigurationDefinition
    {
        private ModConfigurationDefinition Definition;
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
        public ISet<ModConfigurationKey> ConfigurationItemDefinitions => Definition.ConfigurationItemDefinitions;

        private bool AutoSave => Definition.AutoSave;

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

        private ModConfiguration(LoadedNeosMod loadedNeosMod, ModConfigurationDefinition definition)
        {
            LoadedNeosMod = loadedNeosMod;
            Definition = definition;
        }

        internal static void EnsureDirectoryExists()
        {
            Directory.CreateDirectory(ConfigDirectory);
        }

        private static string GetModConfigPath(LoadedNeosMod mod)
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

            // none of the checks failed!
            return true;
        }

        /// <summary>
        /// Check if a key is defined in this config
        /// </summary>
        /// <param name="key">the key to check</param>
        /// <returns>true if the key is defined</returns>
        public bool IsKeyDefined(ModConfigurationKey key)
        {
            // if a key has a non-null defining key it's guaranteed a real key. Lets check for that.
            ModConfigurationKey definingKey = key.DefiningKey;
            if (definingKey != null)
            {
                return true;
            }

            // okay, the defining key was null, so lets try to get the defining key from the hashtable instead
            if (Definition.TryGetDefiningKey(key, out definingKey))
            {
                // we might as well set this now that we have the real defining key
                key.DefiningKey = definingKey;
                return true;
            }

            // there was no definition
            return false;
        }

        /// <summary>
        /// Check if a key is the defining key
        /// </summary>
        /// <param name="key">the key to check</param>
        /// <returns>true if the key is the defining key</returns>
        internal bool IsKeyDefiningKey(ModConfigurationKey key)
        {
            // a key is the defining key if and only if its DefiningKey property references itself
            return ReferenceEquals(key, key.DefiningKey); // this is safe because we'll throw a NRE if key is null
        }

        /// <summary>
        /// Get a value, throwing an exception if the key is not found
        /// </summary>
        /// <param name="key">The key to find</param>
        /// <returns>The found value</returns>
        /// <exception cref="KeyNotFoundException">key does not exist in the collection</exception>
        public object GetValue(ModConfigurationKey key)
        {
            if (TryGetValue(key, out object value))
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
            if (TryGetValue(key, out T value))
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
            if (!Definition.TryGetDefiningKey(key, out ModConfigurationKey definingKey))
            {
                // not in definition
                value = null;
                return false;
            }

            if (definingKey.TryGetValue(out object valueObject))
            {
                value = valueObject;
                return true;
            }
            else if (definingKey.TryComputeDefault(out value))
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
            if (TryGetValue(key, out object valueObject))
            {
                value = (T)valueObject;
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
            if (!Definition.TryGetDefiningKey(key, out ModConfigurationKey definingKey))
            {
                throw new KeyNotFoundException($"{definingKey.Name} is not defined in the config definition for {LoadedNeosMod.NeosMod.Name}");
            }

            if (!definingKey.ValueType().IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException($"{value.GetType()} cannot be assigned to {definingKey.ValueType()}");
            }

            if (!definingKey.Validate(value))
            {
                throw new ArgumentException($"\"{value}\" is not a valid value for \"{Owner.Name}{definingKey.Name}\"");
            }

            definingKey.Set(value);
            FireConfigurationChangedEvent(definingKey, eventLabel);
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
            // the reason we don't fall back to untyped Set() here is so we can skip the type check

            if (!Definition.TryGetDefiningKey(key, out ModConfigurationKey definingKey))
            {
                throw new KeyNotFoundException($"{definingKey.Name} is not defined in the config definition for {LoadedNeosMod.NeosMod.Name}");
            }

            if (!definingKey.Validate(value))
            {
                throw new ArgumentException($"\"{value}\" is not a valid value for \"{Owner.Name}{definingKey.Name}\"");
            }

            definingKey.Set(value);
            FireConfigurationChangedEvent(definingKey, eventLabel);
        }

        /// <summary>
        /// Removes a configuration value, if set
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if a value was successfully found and removed, false if there was no value to remove</returns>
        public bool Unset(ModConfigurationKey key)
        {
            if (Definition.TryGetDefiningKey(key, out ModConfigurationKey definingKey))
            {
                return definingKey.Unset();
            }
            else
            {
                throw new KeyNotFoundException($"{key.Name} is not defined in the config definition for {LoadedNeosMod.NeosMod.Name}");
            }
        }

        private bool AnyValuesSet()
        {
            return ConfigurationItemDefinitions
                .Where(key => key.HasValue)
                .Any();
        }

        internal static ModConfiguration LoadConfigForMod(LoadedNeosMod mod)
        {
            ModConfigurationDefinition definition = mod.NeosMod.GetConfigurationDefinition();
            if (definition == null)
            {
                // if there's no definition, then there's nothing for us to do here
                return null;
            }

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
                                    return new ModConfiguration(mod, definition);
                                case IncompatibleConfigurationHandlingOption.FORCE_LOAD:
                                    // continue processing
                                    break;
                                case IncompatibleConfigurationHandlingOption.ERROR: // fall through to default
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
                                key.Set(value);
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // return early
                return new ModConfiguration(mod, definition);
            }
            catch (Exception e)
            {
                // I know not what exceptions the JSON library will throw, but they must be contained
                mod.AllowSavingConfiguration = false;
                throw new ModConfigurationException($"Error loading config for {mod.NeosMod.Name}", e);
            }

            return new ModConfiguration(mod, definition);
        }

        /// <summary>
        /// Persist this configuration to disk. This method is not called automatically. Default values are not automatically saved.
        /// </summary>
        public void Save() // this overload is needed for binary compatibility (REMOVE IN NEXT MAJOR VERSION)
        {
            Save(false);
        }

        /// <summary>
        /// Persist this configuration to disk. This method is not called automatically.
        /// </summary>
        /// <param name="saveDefaultValues">If true, default values will also be persisted</param>
        public void Save(bool saveDefaultValues = false)
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
            foreach (ModConfigurationKey key in ConfigurationItemDefinitions)
            {
                if (key.TryGetValue(out object value))
                {
                    // I don't need to typecheck this as there's no way to sneak a bad type past my Set() API
                    valueMap[key.Name] = JToken.FromObject(value);
                }
                else if (saveDefaultValues && key.TryComputeDefault(out object defaultValue))
                {
                    // I don't need to typecheck this as there's no way to sneak a bad type past my computeDefault API
                    valueMap[key.Name] = JToken.FromObject(defaultValue);
                }
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

        internal static void RegisterShutdownHook(Harmony harmony)
        {
            try
            {
                MethodInfo shutdown = AccessTools.DeclaredMethod(typeof(Engine), nameof(Engine.Shutdown));
                if (shutdown == null)
                {
                    Logger.ErrorInternal("Could not find method Engine.Shutdown(). Will not be able to autosave configs on close!");
                    return;
                }
                MethodInfo patch = AccessTools.DeclaredMethod(typeof(ModConfiguration), nameof(ShutdownHook));
                if (patch == null)
                {
                    Logger.ErrorInternal("Could not find method ModConfiguration.ShutdownHook(). Will not be able to autosave configs on close!");
                    return;
                }
                harmony.Patch(shutdown, prefix: new HarmonyMethod(patch));
            }
            catch (Exception e)
            {
                Logger.ErrorInternal($"Unexpected exception applying shutdown hook!\n{e}");
            }
        }

        private static void ShutdownHook()
        {
            int count = 0;
            ModLoader.Mods()
                .Select(mod => mod.GetConfiguration())
                .Where(config => config != null)
                .Where(config => config.AutoSave)
                .Where(config => config.AnyValuesSet())
                .Do(config =>
                {
                    config.Save();
                    count += 1;
                });
            Logger.MsgInternal($"Configs saved for {count} mods.");
        }
    }

    public class ModConfigurationException : Exception
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
