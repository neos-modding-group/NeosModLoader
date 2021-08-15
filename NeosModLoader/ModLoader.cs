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
        private static readonly Type NEOS_MOD_TYPE = typeof(NeosMod);
        internal static Dictionary<Assembly, NeosMod> LoadedMods { get; } = new Dictionary<Assembly, NeosMod>();

        internal static void LoadMods()
        {
            string modDirectory = Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).FullName + "\\nml_mods";
            Logger.DebugInternal(string.Format("loading mods from {0}", modDirectory));

            ModAssembly[] modsToLoad = null;
            try
            {
                modsToLoad = Directory.GetFiles(modDirectory, "*.dll")
                    .Select(file => new ModAssembly(file))
                    .ToArray();
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
                        Logger.ErrorInternal(string.Format("Error creating mod directory:\n{0}\n{1}", e2.ToString(), e2.StackTrace.ToString()));
                    }
                }
                else
                {
                    Logger.ErrorInternal(string.Format("Error enumerating mod directory:\n{0}\n{1}", e.ToString(), e.StackTrace.ToString()));
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
                    Logger.ErrorInternal(string.Format("Unexpected exception loading mod assembly from {0}:\n{1}\n{2}", mod.File, e.ToString(), e.StackTrace.ToString()));
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
                    Logger.ErrorInternal(string.Format("Unexpected exception loading mod from {0}:\n{1}\n{2}", mod.File, e.ToString(), e.StackTrace.ToString()));
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
                Logger.ErrorInternal(string.Format("error loading assembly from {0}: {1}", mod.File, e.ToString()));
                return;
            }
            if (assembly == null)
            {
                Logger.ErrorInternal(string.Format("unexpected null loading assembly from {0}", mod.File));
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
                Logger.ErrorInternal(string.Format("no mods found in {0}", mod.File));
            } else if (modClasses.Length != 1)
            {
                Logger.ErrorInternal(string.Format("more than one mod found in {0}. no mods will be loaded.", mod.File));
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
                    Logger.ErrorInternal(string.Format("error instantiating mod {0} from {1}:\n{2}\n{3}", modClass.FullName, mod.File, e.ToString(), e.StackTrace.ToString()));
                    return;
                }
                if (neosMod == null)
                {
                    Logger.ErrorInternal(string.Format("unexpected null instantiating mod {0} from {1}", modClass.FullName, mod.File));
                    return;
                }
                LoadedMods.Add(mod.Assembly, neosMod);
                Logger.MsgInternal(string.Format("loaded mod {0} {1} from {2}", neosMod.Name, neosMod.Version, mod.File));
                try
                {
                    neosMod.OnEngineInit();
                }
                catch (Exception e)
                {
                    Logger.ErrorInternal(string.Format("mod {0} from {1} threw error from OnEngineInit():\n{2}\n{3}", modClass.FullName, mod.File, e.ToString(), e.StackTrace.ToString()));
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
