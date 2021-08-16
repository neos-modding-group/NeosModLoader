using System;
using System.IO;
using System.Reflection;

namespace NeosModLoader
{
    internal class Configuration
    {
        private static readonly string CONFIG_FILENAME = "NeosModLoader.config";

        private static Configuration _configuration;

        internal static Configuration get()
        {
            if (_configuration == null)
            {
                // the config file can just sit next to the dll. Simple.
                string path = $"{GetAssemblyDirectory()}\\{CONFIG_FILENAME}";
                _configuration = new Configuration();

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
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is FileNotFoundException)
                    {
                        Logger.MsgInternal($"{CONFIG_FILENAME} is missing! This is probably fine.");
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
    }
}
