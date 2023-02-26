using BaseX;
using FrooxEngine;
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

		private static bool IsModType(Type type)
		{
			if (neosAssemblies!.Contains(type.Assembly))
			{
				// the type belongs to a Neos assembly
				return false; // don't hide the type
			}
			else
			{
				if (modAssemblies!.Contains(type.Assembly))
				{
					// known type from a mod assembly
					Logger.DebugInternal($"Hid type \"{type}\" from Neos");
					return true; // hide the type
				}
				else
				{
					// an assembly was in neither neosAssemblies nor modAssemblies
					// this implies someone late-loaded an assembly after NML, and it was later used in-game
					// this is super weird, and probably shouldn't ever happen... but if it does, I want to know about it.
					// since this is an edge case users may want to handle in different ways, the HideLateTypes nml config option allows them to choose.
					bool hideLate = ModLoaderConfiguration.Get().HideLateTypes;
					Logger.WarnInternal($"The \"{type}\" type does not appear to part of Neos or a mod. It is unclear whether it should be hidden or not. Due to the HideLateTypes config option being {hideLate} it will be {(hideLate ? "Hidden" : "Shown")}");
					return hideLate; // hide the type only if hideLate == true
				}
			}
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
	}
}
