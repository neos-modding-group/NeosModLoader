using System;
using System.Collections.Generic;

namespace NeosModLoader
{
    // contains members that only the modloader or the mod itself are intended to access
    public abstract class NeosMod : NeosModBase
    {
        public static void Debug(string message) => Logger.DebugExternal(message);
        public static void Debug(object message) => Logger.DebugExternal(message.ToString());
        public static void Debug(params object[] messages) => Logger.DebugList(messages);
        public static void Msg(string message) => Logger.MsgExternal(message);
        public static void Msg(object message) => Logger.MsgExternal(message.ToString());
        public static void Msg(params object[] messages) => Logger.MsgList(messages);
        public static void Warn(string message) => Logger.WarnExternal(message);
        public static void Warn(object message) => Logger.WarnExternal(message.ToString());
        public static void Warn(params object[] messages) => Logger.WarnList(messages);
        public static void Error(string message) => Logger.ErrorExternal(message);
        public static void Error(object message) => Logger.ErrorExternal(message.ToString());
        public static void Error(params object[] messages) => Logger.ErrorList(messages);
        public virtual void OnEngineInit() { }
        public virtual ModConfigurationDefinition GetConfigurationDefinition()
        {
            return null;
        }
        public ModConfigurationDefinition DefineConfiguration(Version version, List<ModConfigurationKey> configurationItemDefinitions)
        {
            return new ModConfigurationDefinition(this, version, configurationItemDefinitions);
        }
    }
}
