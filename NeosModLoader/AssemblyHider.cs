using BaseX;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
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

				// TypeHelper.FindType explicitly does a type search
				MethodInfo findTypeTarget = AccessTools.DeclaredMethod(typeof(TypeHelper), nameof(TypeHelper.FindType));
				MethodInfo findTypePatch = AccessTools.DeclaredMethod(typeof(AssemblyHider), nameof(FindTypePostfix));
				harmony.Patch(findTypeTarget, postfix: new HarmonyMethod(findTypePatch));

				// WorkerManager.IsValidGenericType checks a type for validity, and if it returns `true` it reveals that the type exists
				MethodInfo isValidGenericTypeTarget = AccessTools.DeclaredMethod(typeof(WorkerManager), nameof(WorkerManager.IsValidGenericType));
				MethodInfo isValidGenericTypePatch = AccessTools.DeclaredMethod(typeof(AssemblyHider), nameof(IsValidTypePostfix));
				harmony.Patch(isValidGenericTypeTarget, postfix: new HarmonyMethod(isValidGenericTypePatch));

				// WorkerManager.GetType uses FindType, but upon failure fails back to doing a (strangely) exhausitive reflection-based search for the type
				MethodInfo getTypeTarget = AccessTools.DeclaredMethod(typeof(WorkerManager), nameof(WorkerManager.GetType));
				MethodInfo getTypePatch = AccessTools.DeclaredMethod(typeof(AssemblyHider), nameof(FindTypePostfix));
				harmony.Patch(getTypeTarget, postfix: new HarmonyMethod(getTypePatch));

				// FrooxEngine likes to enumerate all types in all assemblies, which is prone to issues (such as crashing FrooxCode if a type isn't loadable)
				MethodInfo getAssembliesTarget = AccessTools.DeclaredMethod(typeof(AppDomain), nameof(AppDomain.GetAssemblies));
				MethodInfo getAssembliesPatch = AccessTools.DeclaredMethod(typeof(AssemblyHider), nameof(GetAssembliesPostfix));
				harmony.Patch(getAssembliesTarget, postfix: new HarmonyMethod(getAssembliesPatch));
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

		/// <summary>
		/// Check if an assembly belongs to a mod or not
		/// </summary>
		/// <param name="assembly">The assembly to check</param>
		/// <param name="typeOrAssembly">Type of root check being performed. Should be "type" or  "assembly". Used in logging.</param>
		/// <param name="name">Name of the root check being performed. Used in logging.</param>
		/// <param name="log">If `true`, this will emit logs. If `false`, this function will not log.</param>
		/// <param name="forceShowLate">If `true`, then this function will always return `false` for late-loaded types</param>
		/// <returns>`true` if this assembly belongs to a mod.</returns>
		private static bool IsModAssembly(Assembly assembly, string typeOrAssembly, string name, bool log, bool forceShowLate)
		{
			if (neosAssemblies!.Contains(assembly))
			{
				// the type belongs to a Neos assembly
				return false; // don't hide the thing
			}
			else
			{
				if (modAssemblies!.Contains(assembly))
				{
					// known type from a mod assembly
					if (log)
					{
						Logger.DebugInternal($"Hid {typeOrAssembly} \"{name}\" from Neos");
					}
					return true; // hide the thing
				}
				else
				{
					// an assembly was in neither neosAssemblies nor modAssemblies
					// this implies someone late-loaded an assembly after NML, and it was later used in-game
					// this is super weird, and probably shouldn't ever happen... but if it does, I want to know about it.
					// since this is an edge case users may want to handle in different ways, the HideLateTypes nml config option allows them to choose.
					bool hideLate = ModLoaderConfiguration.Get().HideLateTypes;
					if (log)
					{
						Logger.WarnInternal($"The \"{name}\" {typeOrAssembly} does not appear to part of Neos or a mod. It is unclear whether it should be hidden or not. Due to the HideLateTypes config option being {hideLate} it will be {(hideLate ? "Hidden" : "Shown")}");
					}
					// if forceShowLate == true, then this function will always return `false` for late-loaded types
					// if forceShowLate == false, then this function will return `true` when hideLate == true
					return hideLate && !forceShowLate;
				}
			}
		}

		/// <summary>
		/// Check if an assembly belongs to a mod or not
		/// </summary>
		/// <param name="assembly">The assembly to check</param>
		/// <param name="forceShowLate">If `true`, then this function will always return `false` for late-loaded types</param>
		/// <returns>`true` if this assembly belongs to a mod.</returns>
		private static bool IsModAssembly(Assembly assembly, bool forceShowLate = false)
		{
			// this generates a lot of logspam, as a single call to AppDomain.GetAssemblies() calls this many times
			return IsModAssembly(assembly, "assembly", assembly.ToString(), log: false, forceShowLate);
		}

		/// <summary>
		/// Check if a type belongs to a mod or not
		/// </summary>
		/// <param name="type">The type to check</param>
		/// <returns>true` if this type belongs to a mod.</returns>
		private static bool IsModType(Type type)
		{
			return IsModAssembly(type.Assembly, "type", type.ToString(), log: true, forceShowLate: false);
		}

		// postfix for a method that searches for a type, and returns a reference to it if found (TypeHelper.FindType and WorkerManager.GetType)
		private static void FindTypePostfix(ref Type? __result)
		{
			if (__result != null)
			{
				// we only need to think about types if the method actually returned a non-null result
				if (IsModType(__result))
				{
					__result = null;
				}
			}
		}

		// postfix for a method that validates a type (WorkerManager.IsValidGenericType)
		private static void IsValidTypePostfix(ref bool __result, Type type)
		{
			if (__result == true)
			{
				// we only need to think about types if the method actually returned a true result
				if (IsModType(type))
				{
					__result = false;
				}
			}
		}

		private static void GetAssembliesPostfix(ref Assembly[] __result)
		{
			Assembly? callingAssembly = Util.GetCallingAssembly();
			if (callingAssembly != null && neosAssemblies!.Contains(callingAssembly))
			{
				// if we're being called by Neos, then hide mod assemblies
				Logger.DebugFuncInternal(() => $"Intercepting call to AppDomain.GetAssemblies() from {callingAssembly}");
				__result = __result
					.Where(assembly => !IsModAssembly(assembly, forceShowLate: true)) // it turns out Neos itself late-loads a bunch of stuff, so we force-show late-loaded assemblies here
					.ToArray();
			}
		}
	}
}
