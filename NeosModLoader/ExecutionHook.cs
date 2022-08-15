using FrooxEngine;
using System;
using System.Linq;

namespace NeosModLoader
{
    [ImplementableClass(true)]
    internal class ExecutionHook
    {
#pragma warning disable CS0169
        // fields must exist due to reflective access
        private static Type? __connectorType; // needed in all Neos versions
        private static Type? __connectorTypes; // needed in Neos 2021.10.17.1326 and later
#pragma warning restore CS0169

        static ExecutionHook()
        {
            try
            {
                SplashChanger.SetCustom("Loading libraries");
                AssemblyFile[] loadedAssemblies = AssemblyLoader.LoadAssembliesFromDir("nml_libs");
                if (loadedAssemblies.Length != 0)
                {
                    string loadedAssemblyList = string.Join("\n", loadedAssemblies.Select(a => a.Assembly.FullName));
                    Logger.MsgInternal($"Loaded libraries from nml_libs:\n{loadedAssemblyList}");
                }

                SplashChanger.SetCustom("Initializing");
                DebugInfo.Log();
                NeosVersionReset.Initialize();
                ModLoader.WatchModsDirectory();
                ModLoader.LoadMods();
                SplashChanger.SetCustom("Loaded");
            }
            catch (Exception e) // it's important that this doesn't send exceptions back to Neos
            {
                Logger.ErrorInternal($"Exception in execution hook!\n{e}");
            }
        }

        // implementation not strictly required, but method must exist due to reflective access
        private static DummyConnector InstantiateConnector()
        {
            return new DummyConnector();
        }

        // type must match return type of InstantiateConnector()
        private class DummyConnector : IConnector
        {
            public IImplementable? Owner { get; private set; }
            public void ApplyChanges() { }
            public void AssignOwner(IImplementable owner) => Owner = owner;
            public void Destroy(bool destroyingWorld) { }
            public void Initialize() { }
            public void RemoveOwner() => Owner = null;
        }
    }
}
