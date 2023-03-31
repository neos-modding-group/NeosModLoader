using FrooxEngine;
using HarmonyLib;
using NeosModLoader.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NeosModLoader
{
	internal class ModLoaderConfiguration
	{
		private static readonly Lazy<ModLoaderConfiguration> _configuration = new(LoadConfig);
		private static readonly string CONFIG_FILENAME = "NeosModLoader.config";

		public bool AdvertiseVersion { get; private set; } = false;

		public bool Debug { get; private set; } = false;

		public bool ExposeLateTypes
		{
			get => !HideLateTypes;
			set => HideLateTypes = !value;
		}

		public bool ExposeModTypes
		{
			get => !HideModTypes;
			set => HideModTypes = !value;
		}

		public bool HideConflicts
		{
			get => !LogConflicts;
			set => LogConflicts = !value;
		}

		public bool HideLateTypes { get; private set; } = true;

		public bool HideModTypes { get; private set; } = true;

		public bool HideVisuals { get; private set; } = false;

		public bool LogConflicts { get; private set; } = true;

		public bool NoLibraries { get; private set; } = false;

		public bool NoMods { get; private set; } = false;

		public bool Unsafe { get; private set; } = false;

		internal static ModLoaderConfiguration Get() => _configuration.Value;

		private static string GetAssemblyDirectory()
		{
			var codeBase = Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			var path = Uri.UnescapeDataString(uri.Path);

			return Path.GetDirectoryName(path);
		}

		private static string? GetConfigPath()
		{
			if (LaunchArguments.TryGetArgument("Config.Path", out var argument))
				return argument.Value;

			// the config file can just sit next to the dll. Simple.
			return Path.Combine(GetAssemblyDirectory(), CONFIG_FILENAME);
		}

		private static ModLoaderConfiguration LoadConfig()
		{
			var path = GetConfigPath();
			var config = new ModLoaderConfiguration();

			var configOptions = typeof(ModLoaderConfiguration).GetProperties(AccessTools.all).ToArray();

			if (!File.Exists(path))
			{
				Logger.MsgInternal($"Using default config - file doesn't exist: {path}");
			}
			else
			{
				// .NET's ConfigurationManager is some hot trash to the point where I'm just done with it.
				// Time to reinvent the wheel. This parses simple key=value style properties from a text file.
				try
				{
					var unknownKeys = new List<string>();
					var lines = File.ReadAllLines(path);

					foreach (var line in lines)
					{
						var splitIdx = line.IndexOf('=');
						if (splitIdx == -1)
							continue;

						string key = line.Substring(0, splitIdx).Trim();
						string value = line.Substring(splitIdx + 1).Trim();

						var possibleProperty = configOptions.FirstOrDefault(property => property.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));
						if (possibleProperty == null)
						{
							unknownKeys.Add(key);
							continue;
						}

						var parsedValue = TypeDescriptor.GetConverter(possibleProperty.PropertyType).ConvertFromInvariantString(value);
						possibleProperty.SetValue(config, parsedValue);

						Logger.MsgInternal($"Loaded value for {possibleProperty.Name} from file: {parsedValue}");
					}

					Logger.WarnInternal($"Unknown key found in config file: {string.Join(", ", unknownKeys)}");
					Logger.WarnInternal($"Supported keys: {string.Join(", ", configOptions.Select(property => $"{property.PropertyType} {property.Name}"))}");
				}
				catch (Exception e)
				{
					if (e is FileNotFoundException)
					{
						Logger.MsgInternal($"{path} is missing! This is probably fine.");
					}
					else if (e is DirectoryNotFoundException || e is IOException || e is UnauthorizedAccessException)
					{
						Logger.WarnInternal(e.ToString());
					}
					else
					{
						throw;
					}
				}
			}

			var boolType = typeof(bool);
			foreach (var option in configOptions)
			{
				if (LaunchArguments.TryGetArgument($"Config.{option.Name}", out var argument))
				{
					if (option.PropertyType == boolType)
					{
						option.SetValue(config, true);
						Logger.MsgInternal($"Enabling [{option.Name}] from launch flag");

						if (!argument.IsFlag)
							Logger.WarnInternal("Found possible misplaced parameter value after this flag argument");
					}
					else if (!argument.IsFlag)
					{
						config.SetProperty(option, argument.Value!);
						Logger.MsgInternal($"Setting [{option.Name}] from launch flag: {argument.Value}");
					}
				}
			}

			return config;
		}

		private void SetProperty(PropertyInfo property, string value)
		{
			var parsedValue = TypeDescriptor.GetConverter(property.PropertyType).ConvertFromInvariantString(value);
			property.SetValue(this, parsedValue);
		}
	}
}
