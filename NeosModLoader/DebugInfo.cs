using HarmonyLib;

namespace NeosModLoader
{
    internal class DebugInfo
    {
        internal static void Log()
        {
            Logger.MsgInternal($"NeosModLoader v{ModLoader.VERSION} starting up!{(ModLoaderConfiguration.get().Debug ? " Debug logs will be shown." : "")}");
            Logger.MsgInternal($"Using Harmony v{GetHarmonyVersion()}");
        }

        private static string GetHarmonyVersion()
        {
            return typeof(Harmony).Assembly.GetName()?.Version?.ToString();
        }
    }
}
