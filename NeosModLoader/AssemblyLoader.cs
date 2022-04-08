using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NeosModLoader
{
    internal static class AssemblyLoader
    {
        private static AssemblyFile[] GetAssembliesFromDir(string dirName)
        {
            string assembliesDirectory = Path.Combine(Directory.GetCurrentDirectory(), dirName);

            Logger.MsgInternal($"loading assemblies from {assembliesDirectory}");

            AssemblyFile[] assembliesToLoad = null;
            try
            {
                assembliesToLoad = Directory.GetFiles(assembliesDirectory, "*.dll")
                    .Select(file => new AssemblyFile(file))
                    .ToArray();

                Array.Sort(assembliesToLoad, (a, b) => string.CompareOrdinal(a.File, b.File));
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

        private static void LoadAssembly(AssemblyFile assemblyFile)
        {
            SplashChanger.SetCustom($"Loading file: {assemblyFile.File}");
            Assembly assembly;
            try
            {
                Logger.DebugInternal($"Loading assembly [{Path.GetFileName(assemblyFile.File)}]");
                assembly = Assembly.LoadFile(assemblyFile.File);
            }
            catch (Exception e)
            {
                Logger.ErrorInternal($"error loading assembly from {assemblyFile.File}: {e}");
                return;
            }
            if (assembly == null)
            {
                Logger.ErrorInternal($"unexpected null loading assembly from {assemblyFile.File}");
                return;
            }
            assemblyFile.Assembly = assembly;
        }

        internal static AssemblyFile[] LoadAssembliesFromDir(string dirName) {
            var assembliesOrNull = GetAssembliesFromDir(dirName);
            if (assembliesOrNull is AssemblyFile[] assembliesToLoad) {
                foreach (AssemblyFile assemblyFile in assembliesToLoad)
                {
                    try
                    {
                        LoadAssembly(assemblyFile);
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorInternal($"Unexpected exception loading assembly from {assemblyFile.File}:\n{e}");
                    }
                }
            }

            return assembliesOrNull;
        }
    }
}
