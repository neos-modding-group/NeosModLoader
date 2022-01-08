using System;
using System.Collections.Generic;

namespace NeosModLoader
{
    // contains members that only the modloader or the mod itself are intended to access
    public abstract class NeosMod : NeosModBase
    {
        public static void Debug(string message) => Logger.DebugExternal(message); // needed for binary compatibility
        public static void Debug(object message) => Logger.DebugExternal(message);
        public static void Debug(params object[] messages) => Logger.DebugListExternal(messages);

        public static void Msg(string message) => Logger.MsgExternal(message); // needed for binary compatibility
        public static void Msg(object message) => Logger.MsgExternal(message);
        public static void Msg(params object[] messages) => Logger.MsgListExternal(messages);

        public static void Warn(string message) => Logger.WarnExternal(message); // needed for binary compatibility
        public static void Warn(object message) => Logger.WarnExternal(message);
        public static void Warn(params object[] messages) => Logger.WarnListExternal(messages);

        public static void Error(string message) => Logger.ErrorExternal(message); // needed for binary compatibility
        public static void Error(object message) => Logger.ErrorExternal(message);
        public static void Error(params object[] messages) => Logger.ErrorListExternal(messages);

        /// <summary>
        /// Called once immediately after NeosModLoader begins execution
        /// </summary>
        public virtual void OnEngineInit() { }

        /// <summary>
        /// Get the defined configuration for this mod. This should be overridden by your mod if necessary.
        /// </summary>
        /// <returns>This mod's configuration definition. null by default.</returns>
        public virtual ModConfigurationDefinition GetConfigurationDefinition()
        {
            return null;
        }

        /// <summary>
        /// Create a configuration definition for this mod.
        /// </summary>
        /// <param name="version">The semantic version of the configuration definition</param>
        /// <param name="configurationItemDefinitions">A list of configuration items</param>
        /// <returns></returns>
        public ModConfigurationDefinition DefineConfiguration(Version version, IEnumerable<ModConfigurationKey> configurationItemDefinitions)
        {
            return new ModConfigurationDefinition(this, version, new HashSet<ModConfigurationKey>(configurationItemDefinitions));
        }

        /// <summary>
        /// Defines handling of incompatible configuration versions
        /// </summary>
        /// <param name="serializedVersion">Configuration version read from the config file</param>
        /// <param name="definedVersion">Configuration version defined in the mod code</param>
        /// <returns></returns>
        public virtual IncompatibleConfigurationHandlingOption HandleIncompatibleConfigurationVersions(Version serializedVersion, Version definedVersion)
        {
            return IncompatibleConfigurationHandlingOption.ERROR;
        }
    }
}
