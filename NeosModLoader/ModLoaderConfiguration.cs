using System;
using System.IO;
using System.Reflection;

namespace NeosModLoader
{
    internal class ModLoaderConfiguration
    {
        private static readonly string CONFIG_FILENAME = "NeosModLoader.config";

        private static ModLoaderConfiguration _configuration;

        internal static ModLoaderConfiguration get()
        {
            if (_configuration == null)
            {
                // the config file can just sit next to the dll. Simple.
                string path = Path.Combine(GetAssemblyDirectory(), CONFIG_FILENAME);
                _configuration = new ModLoaderConfiguration();

                // .NET's ConfigurationManager is some hot trash to the point where I'm just done with it.
                // Time to reinvent the wheel. This parses simple key=value style properties from a text file.
                try
                {
                    var lines = File.ReadAllLines(path);
                    foreach (var line in lines)
                    {
                        int splitIdx = line.IndexOf('=');
                        if (splitIdx != -1)
                        {
                            string key = line.Substring(0, splitIdx);
                            string value = line.Substring(splitIdx + 1);

                            if ("unsafe".Equals(key) && "true".Equals(value))
                            {
                                _configuration.Unsafe = true;
                            }
                            else if ("debug".Equals(key) && "true".Equals(value))
                            {
                                _configuration.Debug = true;
                            }
                            else if ("nomods".Equals(key) && "true".Equals(value))
                            {
                                _configuration.NoMods = true;
                            }
                            else if ("advertiseversion".Equals(key) && "true".Equals(value))
                            {
                                _configuration.AdvertiseVersion = true;
                            }
                            else if ("logconflicts".Equals(key) && "false".Equals(value))
                            {
                                _configuration.LogConflicts = false;
                            }
                        }
                    }
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
            return _configuration;
        }

        private static string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public bool Unsafe { get; private set; } = false;
        public bool Debug { get; private set; } = false;
        public bool NoMods { get; private set; } = false;
        public bool AdvertiseVersion { get; private set; } = false;
        public bool LogConflicts { get; private set; } = true;
    }
}
