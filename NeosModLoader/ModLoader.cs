using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeosModLoader
{
	/// <summary>
	/// Contains the actual mod loader.
	/// </summary>
	public class ModLoader
	{
		internal const string VERSION_CONSTANT = "1.12.6";
		/// <summary>
		/// NeosModLoader's version
		/// </summary>
		public static readonly string VERSION = VERSION_CONSTANT;
		private static readonly Type NEOS_MOD_TYPE = typeof(NeosMod);
		private static readonly List<LoadedNeosMod> LoadedMods = new(); // used for mod enumeration
		internal static readonly Dictionary<Assembly, NeosMod> AssemblyLookupMap = new(); // used for logging
		private static readonly Dictionary<string, LoadedNeosMod> ModNameLookupMap = new(); // used for duplicate mod checking

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

		internal static void LoadMods(Harmony harmony)
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
				try
				{
					LoadedNeosMod? loaded = InitializeMod(mod);
					if (loaded != null)
					{
						// if loading succeeded, then we need to register the mod
						RegisterMod(loaded);
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
			}

			SplashChanger.SetCustom("Hooking big fish");
			ModConfiguration.RegisterShutdownHook(harmony);

			foreach (LoadedNeosMod mod in LoadedMods)
			{
				try
				{
					HookMod(mod);
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

			Type[] modClasses = mod.Assembly.GetLoadableTypes(t => t.IsClass && !t.IsAbstract && NEOS_MOD_TYPE.IsAssignableFrom(t)).ToArray();
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
				Logger.MsgInternal($"loaded mod [{neosMod.Name}/{neosMod.Version}] ({Path.GetFileName(mod.File)}) by {neosMod.Author} with 256hash: {mod.Sha256}");
				loadedMod.ModConfiguration = ModConfiguration.LoadConfigForMod(loadedMod);
				return loadedMod;
			}
		}

		private static void HookMod(LoadedNeosMod mod)
		{
			SplashChanger.SetCustom($"Starting mod [{mod.NeosMod.Name}/{mod.NeosMod.Version}]");
			Logger.DebugFuncInternal(() => $"calling OnEngineInit() for [{mod.NeosMod.Name}]");
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
