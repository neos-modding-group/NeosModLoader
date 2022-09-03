using BaseX;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeosModLoader
{
	internal static class AssemblyHider
	{
		private static HashSet<Assembly>? neosAssemblies;
		private static HashSet<Assembly>? modAssemblies;

		/// <summary>
		/// Patch Neos's type lookup code to not see mod-related types. This is needed, because users can pass
		/// arbitrary strings to TypeHelper.FindType(), which can be used to detect if someone is running mods.
		/// </summary>
		/// <param name="harmony">Our NML harmony instance</param>
		/// <param name="initialAssemblies">Assemblies that were loaded when NML first started</param>
		internal static void PatchNeos(Harmony harmony, HashSet<Assembly> initialAssemblies)
		{
			if (ModLoaderConfiguration.Get().HideModTypes)
			{
				neosAssemblies = GetNeosAssemblies(initialAssemblies);
				modAssemblies = GetModAssemblies();
				MethodInfo target = AccessTools.DeclaredMethod(typeof(TypeHelper), nameof(TypeHelper.FindType));
				MethodInfo patch = AccessTools.DeclaredMethod(typeof(AssemblyHider), nameof(FindTypePostfix));
				harmony.Patch(target, postfix: new HarmonyMethod(patch));
			}
		}

		private static HashSet<Assembly> GetNeosAssemblies(HashSet<Assembly> initialAssemblies)
		{
			// Remove NML itself, as its types should be hidden but it's guaranteed to be loaded.
			initialAssemblies.Remove(Assembly.GetExecutingAssembly());

			// Remove Harmony, as users who aren't using nml_libs will already have it loaded.
			initialAssemblies.Remove(typeof(Harmony).Assembly);

			return initialAssemblies;
		}

		private static HashSet<Assembly> GetModAssemblies()
		{
			// start with ALL assemblies
			HashSet<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToHashSet();

			// remove assemblies that already existed before NML loaded
			assemblies.ExceptWith(neosAssemblies);

			return assemblies;
		}

		private static void FindTypePostfix(ref Type? __result)
		{
			if (__result != null && !neosAssemblies!.Contains(__result.Assembly))
			{
				if (!modAssemblies!.Contains(__result.Assembly))
				{
					// an assembly was in neither neosAssemblies nor modAssemblies
					// this implies someone late-loaded an assembly after NML, and it was later used in-game
					// this is super weird, and probably shouldn't ever happen... but if it does, I want to know about it.
					// since this is an edge case users may want to handle in different ways, the HideLateTypes nml config option allows them to choose.
					bool hideLate = ModLoaderConfiguration.Get().HideLateTypes;
					Logger.WarnInternal($"The \"{__result}\" type does not appear to part of Neos or a mod. It is unclear whether it should be hidden or not. due to the HideLateTypes config option being {hideLate} it will be {(hideLate ? "Hidden" : "Shown")}");
					if (hideLate) __result = null;
				}
				else
				{
					Logger.DebugInternal($"Hid type \"{__result}\" from Neos");
					__result = null; // Pretend the type doesn't exist
				}
			}
		}
	}
}
