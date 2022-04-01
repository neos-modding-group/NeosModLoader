using System;

namespace NeosModLoader
{
    internal class DebugInfo
    {
        internal static void Log()
        {
            Logger.MsgInternal($"NeosModLoader v{ModLoader.VERSION} starting up!{(ModLoaderConfiguration.Get().Debug ? " Debug logs will be shown." : "")}");
            Logger.MsgInternal($"Using Harmony v{GetAssemblyVersion(typeof(HarmonyLib.Harmony))}");
            Logger.MsgInternal($"Using BaseX v{GetAssemblyVersion(typeof(BaseX.floatQ))}");
            Logger.MsgInternal($"Using FrooxEngine v{GetAssemblyVersion(typeof(FrooxEngine.IComponent))}");
            Logger.MsgInternal($"Using System.Text.Json v{GetAssemblyVersion(typeof(System.Text.Json.JsonElement))}");
        }

        private static string GetAssemblyVersion(Type typeFromAssembly)
        {
            return typeFromAssembly.Assembly.GetName()?.Version?.ToString();
        }
    }
}
