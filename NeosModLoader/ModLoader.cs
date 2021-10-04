using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NeosModLoader
{
    internal class ModLoader
    {
        public static readonly string VERSION = "1.3.1";
        private static readonly Type NEOS_MOD_TYPE = typeof(NeosMod);
        internal static Dictionary<Assembly, NeosMod> LoadedMods { get; } = new Dictionary<Assembly, NeosMod>();

        internal static void LoadMods()
        {
            if (Configuration.get().NoMods)
            {
                Logger.DebugInternal("mods will not be loaded due to configuration file");
                return;
            }

            string modDirectory = Path.Combine(Directory.GetCurrentDirectory(), "nml_mods");

            Logger.DebugInternal($"loading mods from {modDirectory}");

            ModAssembly[] modsToLoad = null;
            try
            {
                modsToLoad = Directory.GetFiles(modDirectory, "*.dll")
                    .Select(file => new ModAssembly(file))
                    .ToArray();

                Array.Sort(modsToLoad, (a, b) => string.CompareOrdinal(a.File, b.File));
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException)
                {
                    Logger.MsgInternal("mod directory not found, creating it now.");
                    try
                    {
                        Directory.CreateDirectory(modDirectory);
                    }
                    catch (Exception e2)
                    {
                        Logger.ErrorInternal($"Error creating mod directory:\n{e2}");
                    }
                }
                else
                {
                    Logger.ErrorInternal($"Error enumerating mod directory:\n{e}");
                }
                return;
            }

            foreach (ModAssembly mod in modsToLoad)
            {
                try
                {
                    LoadAssembly(mod);
                }
                catch (Exception e)
                {
                    Logger.ErrorInternal($"Unexpected exception loading mod assembly from {mod.File}:\n{e}");
                }
            }

            foreach (ModAssembly mod in modsToLoad)
            {
                try
                {
                    HookMod(mod);
                }
                catch (Exception e)
                {
                    Logger.ErrorInternal($"Unexpected exception loading mod from {mod.File}:\n{e}");
                }
            }
        }

        private static void LoadAssembly(ModAssembly mod)
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFile(mod.File);
            }
            catch (Exception e)
            {
                Logger.ErrorInternal($"error loading assembly from {mod.File}: {e}");
                return;
            }
            if (assembly == null)
            {
                Logger.ErrorInternal($"unexpected null loading assembly from {mod.File}");
                return;
            }
            mod.Assembly = assembly;
        }

        private static void HookMod(ModAssembly mod)
        {
            if (mod.Assembly == null)
            {
                return;
            }

            Type[] modClasses = mod.Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && NEOS_MOD_TYPE.IsAssignableFrom(t)).ToArray();
            if (modClasses.Length == 0)
            {
                Logger.ErrorInternal($"no mods found in {mod.File}");
            }
            else if (modClasses.Length != 1)
            {
                Logger.ErrorInternal($"more than one mod found in {mod.File}. no mods will be loaded.");
            }
            else
            {
                Type modClass = modClasses[0];
                NeosMod neosMod = null;
                try
                {
                    neosMod = (NeosMod)AccessTools.CreateInstance(modClass);
                }
                catch (Exception e)
                {
                    Logger.ErrorInternal($"error instantiating mod {modClass.FullName} from {mod.File}:\n{e}");
                    return;
                }
                if (neosMod == null)
                {
                    Logger.ErrorInternal($"unexpected null instantiating mod {modClass.FullName} from {mod.File}");
                    return;
                }
                LoadedMods.Add(mod.Assembly, neosMod);
                Logger.MsgInternal($"loaded mod {neosMod.Name} {neosMod.Version} from {mod.File}");
                try
                {
                    neosMod.OnEngineInit();
                }
                catch (Exception e)
                {
                    Logger.ErrorInternal($"mod {modClass.FullName} from {mod.File} threw error from OnEngineInit():\n{e}");
                }
            }
        }

        private class ModAssembly
        {
            public string File { get; }
            public Assembly Assembly { get; set; }
            public ModAssembly(string file)
            {
                File = file;
            }
        }
    }
}
