using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeosModLoader
{
    internal class DebugInfo
    {
        internal static void Log()
        {
            Logger.MsgInternal($"NeosModLoader v{ModLoader.VERSION} starting up!{(Configuration.get().Debug ? " Debug logs will be shown." : "")}");
            Logger.MsgInternal($"Using Harmony v{GetHarmonyVersion()}");
        }

        private static string GetHarmonyVersion()
        {
            return typeof(Harmony).Assembly.GetName()?.Version?.ToString();
        }
    }
}
