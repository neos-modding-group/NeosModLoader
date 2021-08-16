using FrooxEngine;
using System;

namespace NeosModLoader
{
    [ImplementableClass(true)]
    class ExecutionHook
    {
#pragma warning disable CS0169
        // field must exist due to reflective access
        private static Type __connectorType;
#pragma warning restore CS0169

        static ExecutionHook()
        {
            try
            {
                Logger.MsgInternal($"NeosModLoader v{ModLoader.VERSION} starting up!{(Configuration.get().Debug ? " Debug logs will be shown." : "")}");
                NeosVersionReset.Initialize();
                ModLoader.LoadMods();
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
            public IImplementable Owner { get; private set; }
            public void ApplyChanges() { }
            public void AssignOwner(IImplementable owner) => Owner = owner;
            public void Destroy(bool destroyingWorld) { }
            public void Initialize() { }
            public void RemoveOwner() => Owner = null;
        }
    }
}
