using FrooxEngine;
using System;
using System.Reflection;

namespace NeosModLoader
{
    // Custom splash screen logic failing shouldn't fail the rest of the modloader.
    // Keep that in mind when editing later on.
    internal static class SplashChanger
    {
        private static bool failed = false;
        private static readonly MethodInfo UpdatePhase = typeof(Engine)
            .GetMethod("UpdateInitPhase", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo UpdateSubPhase = typeof(Engine)
            .GetMethod("UpdateInitSubphase", BindingFlags.NonPublic | BindingFlags.Instance);
        // Returned true means success, false means something went wrong.
        internal static bool SetCustom(string text)
        {
            if (ModLoaderConfiguration.Get().HideVisuals) return true;
            try
            {
                // VerboseInit does extra logging, so turning it if off while we change the phase.
                bool ogVerboseInit = Engine.Current.VerboseInit;
                Engine.Current.VerboseInit = false;
                UpdatePhase.Invoke(Engine.Current, new object[] { "~ NeosModLoader ~", false });
                UpdateSubPhase.Invoke(Engine.Current, new object[] { text, false });
                Engine.Current.VerboseInit = ogVerboseInit;
                return true;
            }
            catch (Exception ex)
            {
                if (!failed)
                {
                    Logger.WarnInternal("Splash change failed: " + ex.ToString());
                    failed = true;
                }
                return false;
            }
        }
    }
}
