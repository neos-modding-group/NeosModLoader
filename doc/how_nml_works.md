# Going Into Detail: How NeosModLoader Interfaces with Neos

NeosModLoader interfaces with Neos in two main places: the hook and the compatibility hash.

## The Hook

The hook is the point where Neos "hooks" into NeosModLoader, allowing it to begin execution.

Typically, [plugins] use [components] to execute custom code. This is limiting as they can only begin execution once a world loads. This usually involves putting a plugin's component into your local home.

The NeosModLoader plugin uses a different mechanism. Instead of using a component, it uses a connector. Neos loads connectors during its initial setup, at which point they can begin execution. As this system is independent of world loading it is more reliable. The connector implementation is in [ExecutionHook.cs](../NeosModLoader/ExecutionHook.cs).

This connector-based hook does not modify the Neos application, and only uses public Neos APIs.

## The Compatibility Hash

[Neos plugins][plugin], when loaded, alter your client's compatibility hash from what a vanilla client would have. You cannot join a session unless your compatibility hash is an exact match with the host.

Plugins are intended to add new features such as [components] and [LogiX nodes][logix] to the [data model], and altering the data model breaks multiplayer compatibility. This is why the compatibility hash exists, and why plugins alter it.

NeosModLoader does not change the data model, therefore it is 100% network compatible with vanilla Neos. Furthermore, mods cannot alter the [data model] because NeosModLoader does not perform important component post-processing.

In order to make a client using NeosModLoader compatible with vanilla Neos the compatibility hash must be restored to its default value. [NeosVersionReset.cs](../NeosModLoader/NeosVersionReset.cs) does the following:

1. Finds the original protocol version number by scanning FrooxEngine for a particular integer. This involves interpreting the IL bytes as instructions, which could be considered disassembling.
2. Calculates the default compatibility hash using the integer we just found.
3. Restores version and compatibility hash fields to their original values via [reflection](https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/reflection). This does not modify Neos code, but it does change the values of fields that are not intended to be changed.

[plugin]: https://wiki.neos.com/Plugins
[plugins]: https://wiki.neos.com/Plugins
[component]: https://wiki.neos.com/Component
[components]: https://wiki.neos.com/Component
[logix]: https://wiki.neos.com/LogiX
[data model]: https://wiki.neos.com/Core_Concepts#Data_Model

## Other Minor Ways NeosModLoader Interfaces with Neos

- Hooking into the splash screen to show mod loading progress
- Hooking into Neos shutdown to trigger one last mod configuration save
