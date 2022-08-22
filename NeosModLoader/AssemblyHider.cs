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
			initialAssemblies.Remove(Assembly.GetExecutingAssembly());
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
					Logger.WarnInternal($"The \"{__result}\" type does not appear to part of Neos or a mod. It is unclear whether it should be hidden or not.");
				}
				else
				{
					Type type = __result;
					Logger.DebugFuncInternal(() => $"Hid type \"{type}\" from Neos");
				}

				// Pretend the type doesn't exist
				__result = null;
			}
		}
	}
}
