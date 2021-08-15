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

            string[] files = null;
            try
            {
                files = Directory.GetFiles(modDirectory)
                    .Where(f => ".dll".Equals(Path.GetExtension(f).ToLowerInvariant()))
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


            foreach (string file in files)
            {
                try
                {
                    LoadMod(file);
                }
                catch (Exception e)
                {
                    Logger.ErrorInternal(string.Format("Unexpected exception loading mod from {0}:\n{1}\n{2}", file, e.ToString(), e.StackTrace.ToString()));
                }
            }
        }

        private static void LoadMod(string file)
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFile(file);
            }
            catch (Exception e)
            {
                Logger.ErrorInternal(string.Format("error loading assembly from {0}: {1}", file, e.ToString()));
                return;
            }
            if (assembly == null)
            {
                Logger.ErrorInternal(string.Format("unexpected null loading assembly from {0}", file));
                return;
            }
            HookMod(file, assembly);
        }

        private static void HookMod(string file, Assembly assembly)
        {
            Type[] modClasses = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && NEOS_MOD_TYPE.IsAssignableFrom(t)).ToArray();
            if (modClasses.Length == 0)
            {
                Logger.ErrorInternal(string.Format("no mods found in {0}", file));
            } else if (modClasses.Length != 1)
            {
                Logger.ErrorInternal(string.Format("more than one mod found in {0}. no mods will be loaded.", file));
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
                    Logger.ErrorInternal(string.Format("error instantiating mod {0} from {1}:\n{2}\n{3}", modClass.FullName, file, e.ToString(), e.StackTrace.ToString()));
                    return;
                }
                if (neosMod == null)
                {
                    Logger.ErrorInternal(string.Format("unexpected null instantiating mod {0} from {1}", modClass.FullName, file));
                    return;
                }
                LoadedMods.Add(assembly, neosMod);
                Logger.MsgInternal(string.Format("loaded mod {0} {1} from {2}", neosMod.Name, neosMod.Version, file));
                try
                {
                    neosMod.OnEngineInit();
                }
                catch (Exception e)
                {
                    Logger.ErrorInternal(string.Format("mod {0} from {1} threw error from OnEngineInit():\n{2}\n{3}", modClass.FullName, file, e.ToString(), e.StackTrace.ToString()));
                }
            }
        }
    }
}
