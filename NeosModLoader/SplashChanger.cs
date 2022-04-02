using FrooxEngine;
using HarmonyLib;
using System;

namespace NeosModLoader
{
    // Custom splash screen logic failing shouldn't fail the rest of the modloader.
    // Keep that in mind when editing later on.
    public static class SplashChanger
    {
        // Returned true means success, false means something went wrong.
        public static bool SetCustom(string text) {
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
            } catch {
                return false;
            }
        }
    }
}
