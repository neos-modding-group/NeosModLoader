using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NeosModLoader
{
    internal class ModLoader
    {
        public static readonly string VERSION = "1.5.0";
        private static readonly Type NEOS_MOD_TYPE = typeof(NeosMod);
        internal static List<LoadedNeosMod> LoadedMods { get; } = new List<LoadedNeosMod>();
        internal static Dictionary<Assembly, NeosMod> AssemblyLookupMap = new Dictionary<Assembly, NeosMod>();
        internal static Dictionary<NeosModBase, LoadedNeosMod> ModBaseLookupMap = new Dictionary<NeosModBase, LoadedNeosMod>();
        internal static Dictionary<string, LoadedNeosMod> ModNameLookupMap = new Dictionary<string, LoadedNeosMod>();

        /// <summary>
        /// Allows reading metadata for all loaded mods
        /// </summary>
        /// <returns>A new list containing each loaded mod</returns>
        public static IEnumerable<NeosModBase> Mods()
        {
            return LoadedMods
                .Select(m => (NeosModBase)m.NeosMod)
                .ToList();
        }

        internal static void LoadMods()
        {
            ModLoaderConfiguration config = ModLoaderConfiguration.get();
            if (config.NoMods)
            {
                Logger.DebugInternal("mods will not be loaded due to configuration file");
                return;
            }

            string modDirectory = Path.Combine(Directory.GetCurrentDirectory(), "nml_mods");

            Logger.DebugInternal($"loading mods from {modDirectory}");

            // generate list of assemblies to load
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

            ModConfiguration.EnsureDirectoryExists();

            // mods assemblies are all loaded before hooking begins so mods can interconnect if needed
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

            // call Initialize() each mod
            foreach (ModAssembly mod in modsToLoad)
            {
                try
                {
                    LoadedNeosMod loaded = InitializeMod(mod);
                    if (loaded != null)
                    {
                        RegisterMod(loaded);
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorInternal($"Unexpected exception initializing mod from {mod.File}:\n{e}");
                }
            }

            foreach (LoadedNeosMod mod in LoadedMods)
            {
                try
                {
                    HookMod(mod);
                }
                catch (Exception e)
                {
                    Logger.ErrorInternal($"Unexpected exception initializing mod {mod.NeosMod.Name} from {mod.ModAssembly.File}:\n{e}");
                }
            }

            // log potential conflicts
            if (config.LogConflicts)
            {
                IEnumerable<MethodBase> patchedMethods = Harmony.GetAllPatchedMethods();
                foreach (MethodBase patchedMethod in patchedMethods)
                {
                    Patches patches = Harmony.GetPatchInfo(patchedMethod);
                    HashSet<string> owners = new HashSet<string>(patches.Owners);
                    if (owners.Count > 1)
                    {
                        Logger.WarnInternal($"method \"{patchedMethod.FullDescription()}\" has been patched by the following:");
                        foreach (string owner in owners)
                        {
                            Logger.WarnInternal($"    \"{owner}\" ({TypesForOwner(patches, owner)})");
                        }
                    }
                    else if (config.Debug)
                    {
                        string owner = owners.FirstOrDefault();
                        Logger.DebugInternal($"method \"{patchedMethod.FullDescription()}\" has been patched by \"{owner}\"");
                    }
                }
            }
        }

        private static void RegisterMod(LoadedNeosMod mod)
        {
            try
            {
                ModNameLookupMap.Add(mod.NeosMod.Name, mod);
            }
            catch (ArgumentException)
            {
                LoadedNeosMod existing = ModNameLookupMap[mod.NeosMod.Name];
                Logger.ErrorInternal($"{mod.ModAssembly.File} declares duplicate mod {mod.NeosMod.Name} already declared in {existing.ModAssembly.File}. The new mod will be ignored.");
                return;
            }

            LoadedMods.Add(mod);
            AssemblyLookupMap.Add(mod.ModAssembly.Assembly, mod.NeosMod);
            ModBaseLookupMap.Add(mod.NeosMod, mod);
        }

        private static string TypesForOwner(Patches patches, string owner)
        {
            Func<Patch, bool> ownerEquals = patch => Equals(patch.owner, owner);
            int prefixCount = patches.Prefixes.Where(ownerEquals).Count();
            int postfixCount = patches.Postfixes.Where(ownerEquals).Count();
            int transpilerCount = patches.Transpilers.Where(ownerEquals).Count();
            int finalizerCount = patches.Finalizers.Where(ownerEquals).Count();
            return $"prefix={prefixCount}; postfix={postfixCount}; transpiler={transpilerCount}; finalizer={finalizerCount}";
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

        // loads mod class and mod config
        private static LoadedNeosMod InitializeMod(ModAssembly mod)
        {
            if (mod.Assembly == null)
            {
                return null;
            }

            Type[] modClasses = mod.Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && NEOS_MOD_TYPE.IsAssignableFrom(t)).ToArray();
            if (modClasses.Length == 0)
            {
                Logger.ErrorInternal($"no mods found in {mod.File}");
                return null;
            }
            else if (modClasses.Length != 1)
            {
                Logger.ErrorInternal($"more than one mod found in {mod.File}. no mods will be loaded.");
                return null;
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
                    return null;
                }
                if (neosMod == null)
                {
                    Logger.ErrorInternal($"unexpected null instantiating mod {modClass.FullName} from {mod.File}");
                    return null;
                }

                LoadedNeosMod loadedMod = new LoadedNeosMod(neosMod, mod);
                loadedMod.ModConfiguration = ModConfiguration.LoadConfigForMod(loadedMod);
                return loadedMod;
            }
        }

        private static void HookMod(LoadedNeosMod mod)
        {
            Logger.MsgInternal($"loaded mod {mod.NeosMod.Name} {mod.NeosMod.Version} from {mod.ModAssembly.File}");
            try
            {
                mod.NeosMod.OnEngineInit();
            }
            catch (Exception e)
            {
                Logger.ErrorInternal($"mod {mod.NeosMod.Name} from {mod.ModAssembly.File} threw error from OnEngineInit():\n{e}");
            }
        }
    }
}
