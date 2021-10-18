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
        internal static void DebugList(object[] messages)
        {
            string Source = SourceFromStackTrace();
            foreach (object element in messages)
            {
                LogInternal(LogType.DEBUG, element.ToString(), Source);
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
        internal static void MsgList(object[] messages)
        {
            string Source = SourceFromStackTrace();
            foreach (object element in messages)
            {
                LogInternal(LogType.INFO, element.ToString(), Source);
            }
        }

        internal static void WarnExternal(string message)
        {
            LogInternal(LogType.WARN, message, SourceFromStackTrace());
        }

        internal static void WarnInternal(string message)
        {
            LogInternal(LogType.WARN, message);
        }
        internal static void WarnList(object[] messages)
        {
            string Source = SourceFromStackTrace();
            foreach (object element in messages)
            {
                LogInternal(LogType.WARN, element.ToString(), Source);
            }
        }

        internal static void ErrorExternal(string message)
        {
            LogInternal(LogType.ERROR, message, SourceFromStackTrace());
        }

        internal static void ErrorInternal(string message)
        {
            LogInternal(LogType.ERROR, message);
        }

        internal static void ErrorList(object[] messages)
        {
            string Source = SourceFromStackTrace();
            foreach (object element in messages)
            {
                LogInternal(LogType.ERROR, element.ToString(), Source);
            }
        }

        private static void LogInternal(string logTypePrefix, string message, string source = null)
        {
            if (source == null)
            {
                UniLog.Log($"{logTypePrefix}[NeosModLoader] {message}");
            }
            else
            {
                UniLog.Log($"{logTypePrefix}[NeosModLoader/{source}] {message}");
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

        private sealed class LogType
        {
            public readonly static string DEBUG = "[DEBUG]";
            public readonly static string INFO = "[INFO] ";
            public readonly static string WARN = "[WARN] ";
            public readonly static string ERROR = "[ERROR]";
        }
    }
}
