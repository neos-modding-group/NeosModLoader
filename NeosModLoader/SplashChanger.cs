using FrooxEngine;
using HarmonyLib;
using System;

namespace NeosModLoader
{
    // Custom splash screen logic failing shouldn't fail the rest of the modloader.
    // Keep that in mind when editing later on.
    internal static class SplashChanger
    {
        private static bool failed = false;
        // Returned true means success, false means something went wrong.
        internal static bool SetCustom(string text) {
            if (ModLoaderConfiguration.Get().HideVisuals) return true;
            try {
                // VerboseInit does extra logging, so turning it if off while we change the phase.
                bool ogVerboseInit = Engine.Current.VerboseInit;
                Engine.Current.VerboseInit = false;
                Traverse.Create(Engine.Current)
                    .Method("UpdateInitPhase", new Type[] {typeof(string), typeof(bool)})
                    .GetValue("~ NeosModLoader ~", false);
                Traverse.Create(Engine.Current)
                    .Method("UpdateInitSubphase", new Type[] {typeof(string), typeof(bool)})
                    .GetValue(text, false);
                Engine.Current.VerboseInit = ogVerboseInit;
                return true;
            } catch (Exception ex) {
                if (!failed) {
                    Logger.WarnInternal("Splash change failed: " + ex.ToString());
                    failed = true;
                }
                return false;
            }
        }
    }
}
