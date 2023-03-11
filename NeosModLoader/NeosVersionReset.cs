using BaseX;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace NeosModLoader
{
	internal class NeosVersionReset
	{
		// used when AdvertiseVersion == true
		private const string NEOS_MOD_LOADER = "NeosModLoader.dll";

		internal static void Initialize()
		{
			ModLoaderConfiguration config = ModLoaderConfiguration.Get();
			Engine engine = Engine.Current;

			// get the version string before we mess with it
			string originalVersionString = engine.VersionString;

			List<string> extraAssemblies = Engine.ExtraAssemblies;
			string assemblyFilename = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
			bool nmlPresent = extraAssemblies.Remove(assemblyFilename);

			if (!nmlPresent)
			{
				throw new Exception($"Assertion failed: Engine.ExtraAssemblies did not contain \"{assemblyFilename}\"");
			}

			// get all PostX'd assemblies. This is useful, as plugins can't NOT be PostX'd.
			Assembly[] postxedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
				.Where(IsPostXProcessed)
				.ToArray();

			Logger.DebugFuncInternal(() =>
			{
				string potentialPlugins = postxedAssemblies
					.Select(a => Path.GetFileName(a.Location))
					.Join(delimiter: ", ");
				return $"Found {postxedAssemblies.Length} potential plugins: {potentialPlugins}";
			});

			HashSet<Assembly> expectedPostXAssemblies = GetExpectedPostXAssemblies();

			// attempt to map the PostX'd assemblies to Neos's plugin list
			Dictionary<string, Assembly> plugins = new Dictionary<string, Assembly>(postxedAssemblies.Length);
			Assembly[] unmatchedAssemblies = postxedAssemblies
				.Where(assembly =>
				{
					string filename = Path.GetFileName(assembly.Location);
					if (extraAssemblies.Contains(filename))
					{
						// okay, the assembly's filename is in the plugin list. It's probably a plugin.
						plugins.Add(filename, assembly);
						return false;
					}
					else
					{
						// remove certain expected assemblies from the "unmatchedAssemblies" naughty list
						return !expectedPostXAssemblies.Contains(assembly);
					}
				})
				.ToArray();


			Logger.DebugFuncInternal(() =>
			{
				string actualPlugins = plugins.Keys.Join(delimiter: ", ");
				return $"Found {plugins.Count} actual plugins: {actualPlugins}";
			});

			// warn about the assemblies we couldn't map to plugins
			foreach (Assembly assembly in unmatchedAssemblies)
			{
				Logger.WarnInternal($"Unexpected PostX'd assembly: \"{assembly.Location}\". If this is a plugin, then my plugin-detection code is faulty.");
			}

			// warn about the plugins we couldn't map to assemblies
			HashSet<string> unmatchedPlugins = new(extraAssemblies);
			unmatchedPlugins.ExceptWith(plugins.Keys); // remove all matched plugins
			foreach (string plugin in unmatchedPlugins)
			{
				Logger.ErrorInternal($"Unmatched plugin: \"{plugin}\". NML could not find the assembly for this plugin, therefore NML cannot properly calculate the compatibility hash.");
			}

			// flags used later to determine how to spoof
			bool includePluginsInHash = true;

			// if unsafe is true, we should pretend there are no plugins and spoof everything
			if (config.Unsafe)
			{
				if (!config.AdvertiseVersion)
				{
					extraAssemblies.Clear();
				}
				includePluginsInHash = false;
				Logger.WarnInternal("Unsafe mode is enabled! Not that you had a warranty, but now it's DOUBLE void!");
			}
			// else if unmatched plugins are present, we should not spoof anything
			else if (unmatchedPlugins.Count != 0)
			{
				Logger.ErrorInternal("Version spoofing was not performed due to some plugins having missing assemblies.");
				return;
			}
			// else we should spoof normally


			// get plugin assemblies sorted in the same order Neos sorted them.
			List<Assembly> sortedPlugins = extraAssemblies
				.Select(path => plugins[path])
				.ToList();

			if (config.AdvertiseVersion)
			{
				// put NML back in the version string
				Logger.MsgInternal($"Adding {NEOS_MOD_LOADER} to version string because you have AdvertiseVersion set to true.");
				extraAssemblies.Insert(0, NEOS_MOD_LOADER);
			}

			// we intentionally attempt to set the version string first, so if it fails the compatibilty hash is left on the original value
			// this is to prevent the case where a player simply doesn't know their version string is wrong
			if (!SpoofVersionString(engine, originalVersionString))
			{
				Logger.WarnInternal("Version string spoofing failed");
				return;
			}

			if (!SpoofCompatibilityHash(engine, sortedPlugins, includePluginsInHash))
			{
				Logger.WarnInternal("Compatibility hash spoofing failed");
				return;
			}

			Logger.MsgInternal("Compatibility hash spoofing succeeded");
		}

		private static bool IsPostXProcessed(Assembly assembly)
		{
			return assembly.Modules // in practice there will only be one module, and it will have the dll's name
				.SelectMany(module => module.GetCustomAttributes<DescriptionAttribute>())
				.Where(IsPostXProcessedAttribute)
				.Any();
		}

		private static bool IsPostXProcessedAttribute(DescriptionAttribute descriptionAttribute)
		{
			return descriptionAttribute.Description == "POSTX_PROCESSED";
		}

		// get all the non-plugin PostX'd assemblies we expect to exist
		private static HashSet<Assembly> GetExpectedPostXAssemblies()
		{
			List<Assembly?> list = new()
			{
				Type.GetType("FrooxEngine.IComponent, FrooxEngine")?.Assembly,
				Type.GetType("BusinessX.NeosClassroom, BusinessX")?.Assembly,
				Assembly.GetExecutingAssembly(),
			};
			return list
				.Where(assembly => assembly != null)
				.ToHashSet()!;
		}

		private static bool SpoofCompatibilityHash(Engine engine, List<Assembly> plugins, bool includePluginsInHash)
		{
			string vanillaCompatibilityHash;
			int? vanillaProtocolVersionMaybe = GetVanillaProtocolVersion();
			if (vanillaProtocolVersionMaybe is int vanillaProtocolVersion)
			{
				Logger.DebugFuncInternal(() => $"Vanilla protocol version is {vanillaProtocolVersion}");
				vanillaCompatibilityHash = CalculateCompatibilityHash(vanillaProtocolVersion, plugins, includePluginsInHash);
				return SetCompatibilityHash(engine, vanillaCompatibilityHash);
			}
			else
			{
				Logger.ErrorInternal("Unable to determine vanilla protocol version");
				return false;
			}
		}

		private static string CalculateCompatibilityHash(int ProtocolVersion, List<Assembly> plugins, bool includePluginsInHash)
		{
			using MD5CryptoServiceProvider cryptoServiceProvider = new();
			ConcatenatedStream inputStream = new();
			inputStream.EnqueueStream(new MemoryStream(BitConverter.GetBytes(ProtocolVersion)));
			if (includePluginsInHash)
			{
				foreach (Assembly plugin in plugins)
				{
					FileStream fileStream = File.OpenRead(plugin.Location);
					fileStream.Seek(375L, SeekOrigin.Current);
					inputStream.EnqueueStream(fileStream);
				}
			}
			byte[] hash = cryptoServiceProvider.ComputeHash(inputStream);
			return Convert.ToBase64String(hash);
		}

		private static bool SetCompatibilityHash(Engine engine, string Target)
		{
			// This is super sketchy and liable to break with new compiler versions.
			// I have a good reason for doing it though... if I just called the setter it would recursively
			// end up calling itself, because I'm HOOKINGthe CompatibilityHash setter.
			FieldInfo field = AccessTools.DeclaredField(typeof(Engine), $"<{nameof(Engine.CompatibilityHash)}>k__BackingField");

			if (field == null)
			{
				Logger.WarnInternal("Unable to write Engine.CompatibilityHash");
				return false;
			}
			else
			{
				Logger.DebugFuncInternal(() => $"Changing compatibility hash from {engine.CompatibilityHash} to {Target}");
				field.SetValue(engine, Target);
				return true;
			}
		}

		private static bool SpoofVersionString(Engine engine, string originalVersionString)
		{
			FieldInfo field = AccessTools.DeclaredField(engine.GetType(), "_versionString");
			if (field == null)
			{
				Logger.WarnInternal("Unable to write Engine._versionString");
				return false;
			}
			// null the cached value
			field.SetValue(engine, null);

			Logger.DebugFuncInternal(() => $"Changing version string from {originalVersionString} to {engine.VersionString}");
			return true;
		}

		// perform incredible bullshit to rip the hardcoded protocol version out of the dang IL
		private static int? GetVanillaProtocolVersion()
		{
			// raw IL immediately surrounding the number we need to find, which in this example is 770
			// ldc.i4       770
			// call         unsigned int8[] [mscorlib]System.BitConverter::GetBytes(int32)

			// we're going to search for that method call, then grab the operand of the ldc.i4 that precedes it

			MethodInfo targetCallee = AccessTools.DeclaredMethod(typeof(BitConverter), nameof(BitConverter.GetBytes), new Type[] { typeof(int) });
			if (targetCallee == null)
			{
				Logger.ErrorInternal("Could not find System.BitConverter::GetBytes(System.Int32)");
				return null;
			}

			MethodInfo initializeShim = AccessTools.DeclaredMethod(typeof(Engine), nameof(Engine.Initialize));
			if (initializeShim == null)
			{
				Logger.ErrorInternal("Could not find Engine.Initialize(*)");
				return null;
			}

			AsyncStateMachineAttribute asyncAttribute = (AsyncStateMachineAttribute)initializeShim.GetCustomAttribute(typeof(AsyncStateMachineAttribute));
			if (asyncAttribute == null)
			{
				Logger.ErrorInternal("Could not find AsyncStateMachine for Engine.Initialize");
				return null;
			}

			// async methods are weird. Their body is just some setup code that passes execution... elsewhere.
			// The compiler generates a companion type for async methods. This companion type has some ridiculous nondeterministic name, but luckily
			// we can just ask this attribute what the type is. The companion type should have a MoveNext() method that contains the actual IL we need.
			Type asyncStateMachineType = asyncAttribute.StateMachineType;
			MethodInfo initializeImpl = AccessTools.DeclaredMethod(asyncStateMachineType, "MoveNext");
			if (initializeImpl == null)
			{
				Logger.ErrorInternal("Could not find MoveNext method for Engine.Initialize");
				return null;
			}

			List<CodeInstruction> instructions = PatchProcessor.GetOriginalInstructions(initializeImpl);
			for (int i = 1; i < instructions.Count; i++)
			{
				if (instructions[i].Calls(targetCallee))
				{
					// we're guaranteed to have a previous instruction because we began iteration from 1
					CodeInstruction previous = instructions[i - 1];
					if (OpCodes.Ldc_I4.Equals(previous.opcode))
					{
						return (int)previous.operand;
					}
				}
			}

			return null;
		}
	}
}
