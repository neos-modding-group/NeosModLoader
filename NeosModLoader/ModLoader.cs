using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeosModLoader
{
    public class ModLoader
    {
        internal const string VERSION_CONSTANT = "1.11.3";
        /// <summary>
        /// NeosModLoader's version
        /// </summary>
        public static readonly string VERSION = VERSION_CONSTANT;
        private static readonly Type NEOS_MOD_TYPE = typeof(NeosMod);
        private static readonly List<LoadedNeosMod> LoadedMods = new(); // used for mod enumeration
        internal static readonly Dictionary<Assembly, NeosMod> AssemblyLookupMap = new(); // used for logging
        private static readonly Dictionary<string, LoadedNeosMod> ModNameLookupMap = new(); // used for duplicate mod checking
        private static FileSystemWatcher modDirWatcher = new(@"./nml_mods", "*.dll");

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
            ModLoaderConfiguration config = ModLoaderConfiguration.Get();
            if (config.NoMods)
            {
                Logger.DebugInternal("mods will not be loaded due to configuration file");
                return;
            }
            SplashChanger.SetCustom("Looking for mods");

            // generate list of assemblies to load
            AssemblyFile[] modsToLoad;
            if (AssemblyLoader.LoadAssembliesFromDir("nml_mods") is AssemblyFile[] arr)
            {
                modsToLoad = arr;
            }
            else
            {
                return;
            }

            ModConfiguration.EnsureDirectoryExists();

            // call Initialize() each mod
            foreach (AssemblyFile mod in modsToLoad)
            {
                TryLoadMod(mod, true);
            }

            SplashChanger.SetCustom("Hooking big fish");
            Harmony harmony = new("net.michaelripley.neosmodloader");
            ModConfiguration.RegisterShutdownHook(harmony);

            foreach (LoadedNeosMod mod in LoadedMods)
            {
                try
                {
                    HookMod(mod, true);
                }
                catch (Exception e)
                {
                    Logger.ErrorInternal($"Unexpected exception in OnEngineInit() for mod {mod.NeosMod.Name} from {mod.ModAssembly.File}:\n{e}");
                }
            }

            // log potential conflicts
            if (config.LogConflicts)
            {
                SplashChanger.SetCustom("Looking for conflicts");
                logPotentialConflicts(config);
            }
        }

        private static void logPotentialConflicts(ModLoaderConfiguration config)
        {
            IEnumerable<MethodBase> patchedMethods = Harmony.GetAllPatchedMethods();
            foreach (MethodBase patchedMethod in patchedMethods)
            {
                Patches patches = Harmony.GetPatchInfo(patchedMethod);
                HashSet<string> owners = new(patches.Owners);
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
                    Logger.DebugFuncInternal(() => $"method \"{patchedMethod.FullDescription()}\" has been patched by \"{owner}\"");
                }
            }
        }

        /// <summary>
        /// Tries to initialize a single mod and logs any errors it encounters if it fails.
        /// </summary>
        /// <returns>The loaded mod or null if it failed to load it.</returns>
        private static LoadedNeosMod? TryLoadMod(AssemblyFile mod, bool atStartup)
        {
            try
            {
                LoadedNeosMod? loaded = InitializeMod(mod);
                if (loaded != null)
                {
                    if (!atStartup && !loaded.NeosMod.SupportsHotloading())
                    {
                        // trying to hotload a mod that doesn't support it
                        ModLoaderConfiguration config = ModLoaderConfiguration.Get();
                        if (!config.HotloadUnsupported)
                        {
                            Logger.ErrorInternal($"Cannot hotload mod that does not support it: [{loaded.NeosMod.Name}/{loaded.NeosMod.Version}]");
                            return null;
                        }
                        Logger.ErrorInternal($"Hotloading mod that does not support hotloading: [{loaded.NeosMod.Name}/{loaded.NeosMod.Version}]");
                    }
                    // if loading succeeded, then we need to register the mod
                    RegisterMod(loaded);
                    return loaded.FinishedLoading? loaded : null;
                }
            }
            catch (ReflectionTypeLoadException reflectionTypeLoadException)
            {
                // this exception type has some inner exceptions we must also log to gain any insight into what went wrong
                StringBuilder sb = new();
                sb.AppendLine(reflectionTypeLoadException.ToString());
                foreach (Exception loaderException in reflectionTypeLoadException.LoaderExceptions)
                {
                    sb.AppendLine($"Loader Exception: {loaderException.Message}");
                    if (loaderException is FileNotFoundException fileNotFoundException)
                    {
                        if (!string.IsNullOrEmpty(fileNotFoundException.FusionLog))
                        {
                            sb.Append("    Fusion Log:\n    ");
                            sb.AppendLine(fileNotFoundException.FusionLog);
                        }
                    }
                }
                Logger.ErrorInternal($"ReflectionTypeLoadException initializing mod from {mod.File}:\n{sb}");
            }
            catch (Exception e)
            {
                Logger.ErrorInternal($"Unexpected exception initializing mod from {mod.File}:\n{e}");
            }
            return null;
        }

        /// <summary>
        /// We have a bunch of maps and things the mod needs to be registered in. This method does all that jazz.
        /// </summary>
        /// <param name="mod">The successfully loaded mod to register</param>
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
            mod.NeosMod.loadedNeosMod = mod; // complete the circular reference (used to look up config)
            mod.FinishedLoading = true; // used to signal that the mod is truly loaded
        }

        private static string TypesForOwner(Patches patches, string owner)
        {
            bool ownerEquals(Patch patch) => Equals(patch.owner, owner);
            int prefixCount = patches.Prefixes.Where(ownerEquals).Count();
            int postfixCount = patches.Postfixes.Where(ownerEquals).Count();
            int transpilerCount = patches.Transpilers.Where(ownerEquals).Count();
            int finalizerCount = patches.Finalizers.Where(ownerEquals).Count();
            return $"prefix={prefixCount}; postfix={postfixCount}; transpiler={transpilerCount}; finalizer={finalizerCount}";
        }

        // loads mod class and mod config
        private static LoadedNeosMod? InitializeMod(AssemblyFile mod)
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
                NeosMod? neosMod = null;
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
                SplashChanger.SetCustom($"Loading configuration for [{neosMod.Name}/{neosMod.Version}]");

                LoadedNeosMod loadedMod = new(neosMod, mod);
                Logger.MsgInternal($"loaded mod [{neosMod.Name}/{neosMod.Version}] ({Path.GetFileName(mod.File)}) by {neosMod.Author}");
                loadedMod.ModConfiguration = ModConfiguration.LoadConfigForMod(loadedMod);
                return loadedMod;
            }
        }

        private static void HookMod(LoadedNeosMod mod, bool atStartup)
        {
            SplashChanger.SetCustom($"Starting mod [{mod.NeosMod.Name}/{mod.NeosMod.Version}]");
            Logger.DebugFuncInternal(() => $"calling OnEngineInit() for [{mod.NeosMod.Name}]");
            try
            {
                // check if the mod supports OnInit()
                if (mod.NeosMod.SupportsHotloading())
                {
                    mod.NeosMod.OnInit(atStartup);
                }
                else
                {
                    mod.NeosMod.OnEngineInit();
                }
            }
            catch (Exception e)
            {
                Logger.ErrorInternal($"mod {mod.NeosMod.Name} from {mod.ModAssembly.File} threw error from OnEngineInit():\n{e}");
            }
        }

        /// <summary>
        /// Hotloads a single mod at runtime.
        /// Returns whether or not the mod was sucessfully loaded.
        /// </summary>
        /// <param name="path">The file path to the mod's .dll</param>
        public static bool LoadAndInitializeNewMod(string path)
        {
            ModLoaderConfiguration config = ModLoaderConfiguration.Get();
            if (config.NoMods)
            {
                Logger.DebugInternal("Mod was not hotloaded due to configuration file");
                return false;
            }

            AssemblyFile? mod = AssemblyLoader.LoadAssemblyFile(path);
            if (mod != null)
            {
                LoadedNeosMod? loadedMod = TryLoadMod(mod, false);
                if (loadedMod != null)
                {
                    HookMod(loadedMod, false);

                    // re-log potential conflicts
                    if (config.LogConflicts)
                    {
                        logPotentialConflicts(config);
                    }

                    // display load success to the user
                    if (!config.HideVisuals)
                    {
                        FrooxEngine.LoadingIndicator.ShowMessage("Loaded New Mod", $"The mod '{loadedMod.Name}' has been loaded");
                    }
                    return true;
                }
            }
            if (!config.HideVisuals)
            {
                FrooxEngine.LoadingIndicator.ShowMessage("<color=#f00>Failed to Load Mod</color>", "<color=#f00>Check log for more info</color>");
            }
            return false;
        }

        internal static void WatchModsDirectory()
        {
            ModLoaderConfiguration config = ModLoaderConfiguration.Get();
            if (config.Hotloading)
            {
                modDirWatcher.Created += new FileSystemEventHandler(NewModFound);
                modDirWatcher.IncludeSubdirectories = true;
                modDirWatcher.EnableRaisingEvents = true;
            }
        }

        private static void NewModFound(object sender, FileSystemEventArgs e)
        {
            LoadAndInitializeNewMod(e.FullPath);
        }
    }
}
