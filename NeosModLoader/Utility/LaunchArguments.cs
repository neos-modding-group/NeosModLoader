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
	/// Refer to FrooxEngine.Engine.Initialize(...) to gather possible Arguments
	public static class LaunchArguments
	{
		/// <summary>
		/// Prefix symbol that indicates an argument name: <c>-</c>
		/// </summary>
		public const string ArgumentIndicator = "-";

		/// <summary>
		/// Prefix string that indicated an argument targeted at NML itself or mods loaded by it: <c>-NML.</c><br/>
		/// This is removed from argument names to get their proper name.
		/// </summary>
		public const string NMLArgumentPrefix = ArgumentIndicator + "NML.";

		private static readonly Dictionary<string, Argument> arguments = new();
		private static readonly string[] possibleNeosArguments = { "config", "LoadAssembly", "GeneratePrecache", "Verbose", "pro", "backgroundworkers", "priorityworkers" };
		private static readonly string[] possibleNeosFlagArguments = { "GeneratePrecache", "Verbose", "pro" };

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
		/// Gets the names of all flag arguments recognized by Neos.<br/>
		/// These are forbidden as suffixes of any other arguments.
		/// </summary>
		public static IEnumerable<string> PossibleNeosFlagArguments
		{
			get
			{
				foreach (var flagArgument in possibleNeosFlagArguments)
					yield return flagArgument;
			}
		}

		/// <summary>
		/// Gets the names of all arguments recognized by Neos.<br/>
		/// These are forbidden as suffixes of any other arguments.
		/// </summary>
		public static IEnumerable<string> PossibleNeosLaunchArguments
		{
			get
			{
				foreach (var argument in possibleNeosArguments)
					yield return argument;
			}
		}

		static LaunchArguments()
		{
			var args = Environment.GetCommandLineArgs();

			var i = 0;
			while (i < args.Length)
			{
				var arg = args[i++];

				var matchedNeosArgument = MatchNeosArgument(arg);
				if (matchedNeosArgument != null)
				{
					if (MatchNeosFlagArgument(matchedNeosArgument) != null)
						arguments.Add(matchedNeosArgument, new Argument(Target.Neos, arg, matchedNeosArgument));
					else if (i < args.Length)
						arguments.Add(matchedNeosArgument, new Argument(Target.Neos, arg, matchedNeosArgument, args[i++]));
					else
						Logger.WarnInternal($"Found Neos Launch Argument [{matchedNeosArgument}] without a value");

					continue;
				}

				string? value = null;
				// If it's the last argument or followed by another, treat it as a flag
				if (i < args.Length && !args[i].StartsWith(ArgumentIndicator) && MatchNeosArgument(args[i]) == null)
					value = args[i];

				if (!arg.StartsWith(NMLArgumentPrefix, StringComparison.InvariantCultureIgnoreCase))
				{
					// The value of an unknown argument is not skipped, but added as its own argument in the next iteration as well
					arguments.Add(arg, new Argument(Target.Unknown, arg, value));
					continue;
				}

				// The value of an NML argument gets skipped
				if (value != null)
					++i;

				var name = arg.Substring(NMLArgumentPrefix.Length);
				arguments.Add(name, new Argument(Target.NML, arg, name, value));
			}

			foreach (var argument in arguments)
				Logger.MsgInternal($"Parsed {argument}");
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
		/// Tries to find one of the <see cref="PossibleNeosLaunchArguments"/> that is a suffix of the given name.
		/// </summary>
		/// <param name="name">The name to check.</param>
		/// <returns>The matched Neos launch argument or <c>null</c> if there's no match.</returns>
		public static string? MatchNeosArgument(string name)
			=> possibleNeosArguments.FirstOrDefault(neosArg => name.EndsWith(neosArg, StringComparison.InvariantCultureIgnoreCase));

		/// <summary>
		/// Tries to find one of the <see cref="PossibleNeosFlagArguments"/> that is a suffix of the given name.
		/// </summary>
		/// <param name="name">The name to check.</param>
		/// <returns>The matched Neos launch argument flags or <c>null</c> if there's no match.</returns>
		public static string? MatchNeosFlagArgument(string name)
			=> possibleNeosFlagArguments.FirstOrDefault(neosArg => name.EndsWith(neosArg, StringComparison.InvariantCultureIgnoreCase));

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

			internal Argument(Target target, string rawName, string name, string? value = null)
			{
				Target = target;
				RawName = rawName;
				Name = name;
				Value = value;
			}

			internal Argument(Target target, string name, string? value = null)
				: this(target, name, name, value)
			{ }

			/// <summary>
			/// Gets a string representation of this parsed argument.
			/// </summary>
			/// <returns>A string representing this parsed argument.</returns>
			public override string ToString()
			{
				return $"{Target} Argument {RawName} ({Name}): {(IsFlag ? "present" : $"\"{Value}\"")}";
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
