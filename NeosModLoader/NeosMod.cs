using System;
using System.Collections.Generic;

namespace NeosModLoader
{
	/// <summary>
	/// Contains members that only the <see cref="ModLoader"/> or the Mod itself are intended to access.
	/// </summary>
	public abstract class NeosMod : NeosModBase
	{
		/// <summary>
		/// Gets whether debug logging is enabled.
		/// </summary>
		/// <returns><c>true</c> if debug logging is enabled.</returns>
		public static bool IsDebugEnabled() => Logger.IsDebugEnabled();

		/// <summary>
		/// Logs an object as a line in the log based on the value produced by the given function if debug logging is enabled..
		/// <para/>
		/// This is more efficient than passing an <see cref="object"/> or a <see cref="string"/> directly,
		/// as it won't be generated if debug logging is disabled.
		/// </summary>
		/// <param name="messageProducer">The function generating the object to log.</param>
		public static void DebugFunc(Func<object> messageProducer) => Logger.DebugFuncExternal(messageProducer);

		/// <summary>
		/// Logs the given message as a line in the log if debug logging is enabled. Prefer <see cref="DebugFunc(Func{object})"/> or <see cref="Debug(object)"/>.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Debug(string message) => Logger.DebugExternal(message); // needed for binary compatibility (REMOVE IN NEXT MAJOR VERSION)

		/// <summary>
		/// Logs the given object as a line in the log if debug logging is enabled.
		/// </summary>
		/// <param name="message">The object to log.</param>
		public static void Debug(object message) => Logger.DebugExternal(message);

		/// <summary>
		/// Logs the given objects as lines in the log if debug logging is enabled.
		/// </summary>
		/// <param name="messages">The objects to log.</param>
		public static void Debug(params object[] messages) => Logger.DebugListExternal(messages);


		/// <summary>
		/// Logs the given message as a regular line in the log. Prefer <see cref="Msg(object)"/>.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Msg(string message) => Logger.MsgExternal(message); // needed for binary compatibility (REMOVE IN NEXT MAJOR VERSION)

		/// <summary>
		/// Logs the given object as a regular line in the log.
		/// </summary>
		/// <param name="message">The object to log.</param>
		public static void Msg(object message) => Logger.MsgExternal(message);

		/// <summary>
		/// Logs the given objects as regular lines in the log.
		/// </summary>
		/// <param name="messages">The objects to log.</param>
		public static void Msg(params object[] messages) => Logger.MsgListExternal(messages);


		/// <summary>
		/// Logs the given message as a warning line in the log. Prefer <see cref="Warn(object)"/>.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Warn(string message) => Logger.WarnExternal(message); // needed for binary compatibility (REMOVE IN NEXT MAJOR VERSION)

		/// <summary>
		/// Logs the given object as a warning line in the log.
		/// </summary>
		/// <param name="message">The object to log.</param>
		public static void Warn(object message) => Logger.WarnExternal(message);

		/// <summary>
		/// Logs the given objects as warning lines in the log.
		/// </summary>
		/// <param name="messages">The objects to log.</param>
		public static void Warn(params object[] messages) => Logger.WarnListExternal(messages);


		/// <summary>
		/// Logs the given message as an error line in the log. Prefer <see cref="Error(object)"/>.
		/// </summary>
		/// <param name="message">The message to log.</param>
		public static void Error(string message) => Logger.ErrorExternal(message); // needed for binary compatibility (REMOVE IN NEXT MAJOR VERSION)

		/// <summary>
		/// Logs the given object as an error line in the log.
		/// </summary>
		/// <param name="message">The object to log.</param>
		public static void Error(object message) => Logger.ErrorExternal(message);

		/// <summary>
		/// Logs the given objects as error lines in the log.
		/// </summary>
		/// <param name="messages">The objects to log.</param>
		public static void Error(params object[] messages) => Logger.ErrorListExternal(messages);

		/// <summary>
		/// Called once immediately after NeosModLoader begins execution
		/// </summary>
		public virtual void OnEngineInit() { }

		/// <summary>
		/// Build the defined configuration for this mod.
		/// </summary>
		/// <returns>This mod's configuration definition.</returns>
		internal ModConfigurationDefinition? BuildConfigurationDefinition()
		{
			ModConfigurationDefinitionBuilder builder = new(this);
			builder.ProcessAttributes();
			DefineConfiguration(builder);
			return builder.Build();
		}

		/// <summary>
		/// Get the defined configuration for this mod. This should be overridden by your mod if necessary.
		/// </summary>
		/// <returns>This mod's configuration definition. calls DefineConfiguration(ModConfigurationDefinitionBuilder) by default.</returns>
		[Obsolete("This method is obsolete. Use DefineConfiguration(ModConfigurationDefinitionBuilder) instead.")] // REMOVE IN NEXT MAJOR VERSION
		public virtual ModConfigurationDefinition? GetConfigurationDefinition()
		{
			return BuildConfigurationDefinition();
		}

		/// <summary>
		/// Create a configuration definition for this mod.
		/// </summary>
		/// <param name="version">The semantic version of the configuration definition</param>
		/// <param name="configurationItemDefinitions">A list of configuration items</param>
		/// <returns></returns>
		[Obsolete("This method is obsolete. Use DefineConfiguration(ModConfigurationDefinitionBuilder) instead.")] // REMOVE IN NEXT MAJOR VERSION
		public ModConfigurationDefinition DefineConfiguration(Version version, IEnumerable<ModConfigurationKey> configurationItemDefinitions) // needed for binary compatibility
		{
			return DefineConfiguration(version, configurationItemDefinitions, true);
		}

		/// <summary>
		/// Create a configuration definition for this mod.
		/// </summary>
		/// <param name="version">The semantic version of the configuration definition</param>
		/// <param name="configurationItemDefinitions">A list of configuration items</param>
		/// <param name="autoSave">If false, the config will not be autosaved on Neos close</param>
		/// <returns></returns>
		[Obsolete("This method is obsolete. Use DefineConfiguration(ModConfigurationDefinitionBuilder) instead.")] // REMOVE IN NEXT MAJOR VERSION
		public ModConfigurationDefinition DefineConfiguration(Version version, IEnumerable<ModConfigurationKey> configurationItemDefinitions, bool autoSave = true)
		{
			if (version == null)
			{
				throw new ArgumentNullException("version must be non-null");
			}

			if (configurationItemDefinitions == null)
			{
				throw new ArgumentNullException("configurationItemDefinitions must be non-null");
			}

			return new ModConfigurationDefinition(this, version, new HashSet<ModConfigurationKey>(configurationItemDefinitions), autoSave);
		}

		/// <summary>
		/// Define this mod's configuration via a builder
		/// </summary>
		/// <param name="builder">A builder you can use to define the mod's configuration</param>
		public virtual void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
		{
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
