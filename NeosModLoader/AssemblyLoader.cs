using NeosModLoader.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NeosModLoader
{
	internal static class AssemblyLoader
	{
		private static string[]? GetAssemblyPathsFromDir(string dirName)
		{
			string assembliesDirectory = Path.Combine(PlatformHelper.MainDirectory, dirName);

			Logger.MsgInternal($"loading assemblies from {dirName}");

			string[]? assembliesToLoad = null;
			try
			{
				// Directory.GetFiles and Directory.EnumerateFiles have a fucked up API: https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles?view=netframework-4.6.2#system-io-directory-getfiles(system-string-system-string-system-io-searchoption)
				// long story short if I searched for "*.dll" it would unhelpfully use some incredibly inconsistent behavior and return results like "foo.dll_disabled"
				// So I have to filter shit after the fact... ugh
				assembliesToLoad = Directory.EnumerateFiles(assembliesDirectory, "*.dll", SearchOption.AllDirectories)
					.Where(file => file.EndsWith(".dll"))
					.ToArray();
				Array.Sort(assembliesToLoad, string.CompareOrdinal);
			}
			catch (Exception e)
			{
				if (e is DirectoryNotFoundException)
				{
					Logger.MsgInternal($"{dirName} directory not found, creating it now.");
					try
					{
						Directory.CreateDirectory(assembliesDirectory);
					}
					catch (Exception e2)
					{
						Logger.ErrorInternal($"Error creating ${dirName} directory:\n{e2}");
					}
				}
				else
				{
					Logger.ErrorInternal($"Error enumerating ${dirName} directory:\n{e}");
				}
			}
			return assembliesToLoad;
		}

		private static Assembly? LoadAssembly(string filepath)
		{
			string filename = Path.GetFileName(filepath);
			SplashChanger.SetCustom($"Loading file: {filename}");
			Assembly assembly;
			try
			{
				Logger.DebugFuncInternal(() => $"load assembly {filename}");
				assembly = Assembly.LoadFrom(filepath);
			}
			catch (Exception e)
			{
				Logger.ErrorInternal($"error loading assembly from {filepath}: {e}");
				return null;
			}
			if (assembly == null)
			{
				Logger.ErrorInternal($"unexpected null loading assembly from {filepath}");
				return null;
			}
			return assembly;
		}

		internal static AssemblyFile[] LoadAssembliesFromDir(string dirName)
		{
			List<AssemblyFile> assemblyFiles = new();
			if (GetAssemblyPathsFromDir(dirName) is string[] assemblyPaths)
			{
				foreach (string assemblyFilepath in assemblyPaths)
				{
					try
					{
						if (LoadAssembly(assemblyFilepath) is Assembly assembly)
						{
							assemblyFiles.Add(new AssemblyFile(assemblyFilepath, assembly));
						}
					}
					catch (Exception e)
					{
						Logger.ErrorInternal($"Unexpected exception loading assembly from {assemblyFilepath}:\n{e}");
					}
				}
			}
			return assemblyFiles.ToArray();
		}
	}
}
