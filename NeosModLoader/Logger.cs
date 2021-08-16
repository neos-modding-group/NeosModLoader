using BaseX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace NeosModLoader
{
    internal class Logger
    {
        internal static void DebugExternal(string message)
        {
            if (Configuration.get().Debug)
            {
                LogInternal(LogType.DEBUG, message, SourceFromStackTrace());
            }
        }

        internal static void DebugInternal(string message)
        {
            if (Configuration.get().Debug)
            {
                LogInternal(LogType.DEBUG, message);
            }
        }

        internal static void MsgExternal(string message)
        {
            LogInternal(LogType.INFO, message, SourceFromStackTrace());
        }

        internal static void MsgInternal(string message)
        {
            LogInternal(LogType.INFO, message);
        }

        internal static void WarnExternal(string message)
        {
            LogInternal(LogType.WARN, message, SourceFromStackTrace());
        }

        internal static void WarnInternal(string message)
        {
            LogInternal(LogType.WARN, message);
        }

        internal static void ErrorExternal(string message)
        {
            LogInternal(LogType.ERROR, message, SourceFromStackTrace());
        }

        internal static void ErrorInternal(string message)
        {
            LogInternal(LogType.ERROR, message);
        }

        private static void LogInternal(LogType logType, string message, string source = null)
        {
            if (source == null)
            {
                UniLog.Log($"{GetTagFromLogType(logType)}[NeosModLoader] {message}");
            }
            else
            {
                UniLog.Log($"{GetTagFromLogType(logType)}[NeosModLoader/{source}] {message}");
            }
        }

        private static string SourceFromStackTrace()
        {
            Dictionary<Assembly, NeosMod> loadedMods = ModLoader.LoadedMods;
            // skip three frames: SourceFromStackTrace(), MsgExternal(), Msg()
            StackTrace stackTrace = new StackTrace(3);
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                Assembly assembly = stackTrace.GetFrame(i).GetMethod().DeclaringType.Assembly;
                NeosMod mod;
                if (loadedMods.TryGetValue(assembly, out mod))
                {
                    return mod.Name;
                }
            }
            return null;
        }

        private enum LogType
        {
            DEBUG,
            INFO,
            WARN,
            ERROR,
        }

        private static string GetTagFromLogType(LogType logType)
        {
            switch (logType)
            {
                case LogType.DEBUG: return "[DEBUG]";
                case LogType.INFO: return "[INFO] ";
                case LogType.WARN: return "[WARN] ";
                case LogType.ERROR: return "[ERROR]";
                default: return $"[{Enum.GetName(typeof(LogType), logType)}]"; // should never happen, but just in case...
            }
        }
    }
}
