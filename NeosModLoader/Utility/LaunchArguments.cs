using FrooxEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FrooxEngine.FinalIK.IKSolverVR;

namespace NeosModLoader.Utility
{
	/// <summary>
	/// Contains methods to access the command line arguments Neos was launched with.
	/// </summary>
	/// Use analyze on Environment.GetCommandLineArgs() to find all possible arguments.
	public static class LaunchArguments
	{
		/// <summary>
		/// Prefix symbol that indicates an argument name: <c>-</c>
		/// </summary>
		public const char ArgumentIndicator = '-';

		/// <summary>
		/// Prefix string after the argument indicator, that indicated an argument targeted at NML itself or mods loaded by it: <c>NML.</c><br/>
		/// This is removed from argument names to get their proper name.
		/// </summary>
		public const string NMLArgumentPrefix = "NML.";

		private static readonly Dictionary<string, Argument> arguments = new(StringComparer.InvariantCultureIgnoreCase);

		private static readonly string[] possibleNeosExactFlagArguments =
		{
			"ctaa", "ctaatemporaledgepower", "ctaasharpnessenabled", "ctaaadaptivesharpness", "ForceSRAnipal",
			"StereoDisplay", "MixedReality", "DirectComposition", "ExternalComposition", "create_mrc_config",
			"load_mrc_config"
		};

		private static readonly string[] possibleNeosExactParameterArguments =
		{
			"TextureSizeRatio", "DataPath", "CachePath"
		};

		private static readonly string[] possibleNeosInfixFlagArguments =
		{
			"CameraBiggestGroup", "CameraTimelapse", "CameraStayBehind", "CameraStayInFront", "AnnounceHomeOnLAN"
		};

		private static readonly string[] possibleNeosSuffixFlagArguments =
		{
			"GeneratePrecache", "Verbose", "pro", "UseLocalCloud", "UseStagingCloud", "ForceRelay", "Invisible",
			"ForceReticleAboveHorizon", "ForceNoVoice", "ResetDash", "Kiosk", "DontAutoOpenCloudHome", "ResetUserspace",
			"ForceLANOnly", "DeleteUnsyncedCloudRecords", "ForceSyncConflictingCloudRecords", "ForceIntroTutorial",
			"SkipIntroTutorial", "RepairDatabase", "UseNeosCamera", "LegacySteamVRInput", "Etee", "HideScreenReticle",
			"Viveport", "DisableNativeTextureUpload"
		};

		private static readonly string[] possibleNeosSuffixParameterArguments =
		{
			"Config", "LoadAssembly", "BackgroundWorkers", "PriorityWorkers", "Bench", "Watchdog", "Bootstrap",
			"OnlyHost", "Join", "Open", "OpenUnsafe", "EnableOWO"
		};

		/// <summary>
		/// Gets all arguments Neos was launched with.
		/// </summary>
		public static IEnumerable<Argument> Arguments
		{
			get
			{
				foreach (var argument in arguments.Values)
					yield return argument;
			}
		}

		/// <summary>
		/// Gets the path to the launched Neos executable.
		/// </summary>
		public static string ExecutablePath { get; }

		/// <summary>
		/// Gets the names of all arguments recognized by Neos.<br/>
		/// These have different rules relating to the names of other arguments.
		/// </summary>
		public static IEnumerable<string> PossibleNeosArguments
			=> PossibleNeosParameterArguments.Concat(PossibleNeosFlagArguments);

		/// <summary>
		/// Gets the names of all exact flag arguments recognized by Neos.<br/>
		/// These are forbidden as the exact names of any other arguments.
		/// </summary>
		public static IEnumerable<string> PossibleNeosExactFlagArguments
		{
			get
			{
				foreach (var argument in possibleNeosExactFlagArguments)
					yield return argument;
			}
		}

		/// <summary>
		/// Gets the names of all exact parameter arguments recognized by Neos.<br/>
		/// These are forbidden as the exact names of any other arguments.
		/// </summary>
		public static IEnumerable<string> PossibleNeosExactParameterArguments
		{
			get
			{
				foreach (var argument in possibleNeosExactParameterArguments)
					yield return argument;
			}
		}

		/// <summary>
		/// Gets the names of all flag arguments recognized by Neos.<br/>
		/// These have different rules relating to the names of other arguments.
		/// </summary>
		public static IEnumerable<string> PossibleNeosFlagArguments
			=> possibleNeosExactFlagArguments.Concat(possibleNeosSuffixFlagArguments).Concat(possibleNeosInfixFlagArguments);

		/// <summary>
		/// Gets the names of all infix flag arguments recognized by Neos.<br/>
		/// These are forbidden as infixes of any other arguments.
		/// </summary>
		public static IEnumerable<string> PossibleNeosInfixFlagArguments
		{
			get
			{
				foreach (var argument in possibleNeosInfixFlagArguments)
					yield return argument;
			}
		}

		/// <summary>
		/// Gets the names of all parameter arguments recognized by Neos.<br/>
		/// These have different rules relating to the names of other arguments.
		/// </summary>
		public static IEnumerable<string> PossibleNeosParameterArguments
			=> possibleNeosExactParameterArguments.Concat(possibleNeosSuffixParameterArguments);

		/// <summary>
		/// Gets the names of all suffix flag arguments recognized by Neos.<br/>
		/// These are forbidden as suffixes of any other arguments.
		/// </summary>
		public static IEnumerable<string> PossibleNeosSuffixFlagArguments
		{
			get
			{
				foreach (var flagArgument in possibleNeosSuffixFlagArguments)
					yield return flagArgument;
			}
		}

		/// <summary>
		/// Gets the names of all suffix parameter arguments recognized by Neos.<br/>
		/// These are forbidden as suffixes of any other arguments.
		/// </summary>
		public static IEnumerable<string> PossibleNeosSuffixParameterArguments
		{
			get
			{
				foreach (var argument in possibleNeosSuffixParameterArguments)
					yield return argument;
			}
		}

		static LaunchArguments()
		{
			possibleNeosExactFlagArguments = possibleNeosExactFlagArguments
				.Concat(
					Enum.GetValues(typeof(HeadOutputDevice))
					.Cast<HeadOutputDevice>()
					.Select(v => v.ToString()))
				.ToArray();

			// First argument is the path of the executable
			var args = Environment.GetCommandLineArgs();
			ExecutablePath = args[0];

			var i = 1;
			while (i < args.Length)
			{
				var arg = args[i++].TrimStart(ArgumentIndicator);
				var hasParameter = i < args.Length && args[i].FirstOrDefault() != ArgumentIndicator && MatchNeosArgument(args[i]) == null;

				var matchedNeosArgument = MatchNeosFlagArgument(arg);
				if (matchedNeosArgument != null)
				{
					arguments.Add(matchedNeosArgument, new Argument(Target.Neos, i, arg, matchedNeosArgument));

					if (hasParameter)
						Logger.WarnInternal($"Possible misplaced parameter value after flag argument: {matchedNeosArgument}");

					continue;
				}

				matchedNeosArgument = MatchNeosParameterArgument(arg);
				if (matchedNeosArgument != null)
				{
					if (hasParameter)
						arguments.Add(matchedNeosArgument, new Argument(Target.Neos, i, arg, matchedNeosArgument, args[i++]));
					else
						Logger.WarnInternal($"Expected parameter for argument: {matchedNeosArgument}");

					continue;
				}

				if (!arg.StartsWith(NMLArgumentPrefix, StringComparison.InvariantCultureIgnoreCase))
				{
					// The value of an unknown argument is not skipped, but added as its own argument in the next iteration as well
					arguments.Add(arg, new Argument(Target.Unknown, i, arg, arg, hasParameter ? args[i] : null));
					continue;
				}

				var name = arg.Substring(NMLArgumentPrefix.Length);
				arguments.Add(name, new Argument(Target.NML, i, arg, name, hasParameter ? args[i++] : null));
			}

			foreach (var argument in arguments)
				Logger.MsgInternal($"Parsed {argument.Value}");
		}

		/// <summary>
		/// Gets the <see cref="Argument"/> with the given proper name.
		/// </summary>
		/// <param name="name">The proper name of the argument.</param>
		/// <returns>The <see cref="Argument"/> with the given proper name.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="KeyNotFoundException"/>
		public static Argument GetArgument(string name)
			=> arguments[name];

		/// <summary>
		/// Checks whether an argument with the given proper name is present.
		/// </summary>
		/// <param name="name">The proper name of the argument.</param>
		/// <returns><c>true</c> if such an argument exists, otherwise <c>false</c>.</returns>
		public static bool IsPresent(string name)
			=> arguments.ContainsKey(name);

		/// <summary>
		/// Tries to find one of the <see cref="PossibleNeosArguments"/> that matches the given name in the right way.
		/// </summary>
		/// <param name="name">The name to check.</param>
		/// <returns>The matched Neos launch argument or <c>null</c> if there's no match.</returns>
		public static string? MatchNeosArgument(string name)
			=> MatchNeosParameterArgument(name)
			?? MatchNeosFlagArgument(name);

		/// <summary>
		/// Tries to find one of the <see cref="PossibleNeosFlagArguments"/> that matches the given name in the right way.
		/// </summary>
		/// <param name="name">The name to check.</param>
		/// <returns>The matched Neos launch argument flags or <c>null</c> if there's no match.</returns>
		public static string? MatchNeosFlagArgument(string name)
			=> possibleNeosExactFlagArguments.FirstOrDefault(neosArg => name.Equals(neosArg, StringComparison.InvariantCultureIgnoreCase))
			?? possibleNeosSuffixFlagArguments.FirstOrDefault(neosArg => name.EndsWith(neosArg, StringComparison.InvariantCultureIgnoreCase))
			?? possibleNeosInfixFlagArguments.FirstOrDefault(neosArg => name.IndexOf(neosArg, StringComparison.InvariantCultureIgnoreCase) >= 0);

		/// <summary>
		/// Tries to find one of the <see cref="PossibleNeosParameterArguments"/> that matches the given name in the right way.
		/// </summary>
		/// <param name="name">The name to check.</param>
		/// <returns>The matched Neos launch argument or <c>null</c> if there's no match.</returns>
		public static string? MatchNeosParameterArgument(string name)
			=> possibleNeosExactParameterArguments.FirstOrDefault(neosArg => name.Equals(neosArg, StringComparison.InvariantCultureIgnoreCase))
			?? possibleNeosSuffixParameterArguments.FirstOrDefault(neosArg => name.EndsWith(neosArg, StringComparison.InvariantCultureIgnoreCase));

		/// <summary>
		/// Tries to get the <see cref="Argument"/> with the given proper name.
		/// </summary>
		/// <param name="name">The proper name of the argument.</param>
		/// <param name="argument">The <see cref="Argument"/> with the given proper name or <c>default(<see cref="Argument"/>)</c> if it's not present.</param>
		/// <returns><c>true</c> if such an argument exists, otherwise <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static bool TryGetArgument(string name, out Argument argument)
			=> arguments.TryGetValue(name, out argument);

		/// <summary>
		/// Data structure for launch arguments.
		/// </summary>
		public readonly struct Argument
		{
			/// <summary>
			/// Gets the index of this argument in the array returned by <see cref="Environment.GetCommandLineArgs"/>.
			/// </summary>
			public int Index { get; }

			/// <summary>
			/// Gets whether the argument is a flag, i.e. whether it doesn't have a value.
			/// </summary>
			public bool IsFlag => Value == null;

			/// <summary>
			/// Gets the proper name of the argument.<br/>
			/// For <see cref="Target.Neos"/> this is the name Neos looks for,
			/// while for <see cref="Target.NML"/> this is the name without the <see cref="NMLArgumentPrefix">NML Argument Prefix</see>.
			/// </summary>
			public string Name { get; }

			/// <summary>
			/// Gets the raw name of the argument, as it is on the command line.
			/// </summary>
			public string RawName { get; }

			/// <summary>
			/// Gets the target that the argument is for.
			/// </summary>
			public Target Target { get; }

			/// <summary>
			/// Gets the value associated with the argument. Is <c>null</c> for <see cref="IsFlag">flags</see>.
			/// </summary>
			public string? Value { get; }

			internal Argument(Target target, int index, string rawName, string name, string? value = null)
			{
				Target = target;
				Index = index - 1;
				RawName = rawName;
				Name = name;
				Value = value;
			}

			/// <summary>
			/// Gets a string representation of this parsed argument.
			/// </summary>
			/// <returns>A string representing this parsed argument.</returns>
			public override string ToString()
			{
				return $"{Target} Argument {(Name.Equals(RawName, StringComparison.InvariantCultureIgnoreCase) ? Name : $"{RawName} ({Name})")}: {(IsFlag ? "present" : $"\"{Value}\"")}";
			}
		}

		/// <summary>
		/// Different possible targets for command line arguments.
		/// </summary>
		public enum Target
		{
			/// <summary>
			/// Not a known target.
			/// </summary>
			Unknown,

			/// <summary>
			/// Targeted at Neos itself.
			/// </summary>
			Neos,

			/// <summary>
			/// Targeted at NML or a mod it loads.
			/// </summary>
			NML
		}
	}
}
