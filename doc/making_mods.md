# Mod Creation Guide

If you have some level of familiarity with C#, getting started making mods should not be too difficult.

## Basic Visual Studio setup

1. Make a new .NET library against .NET version 4.6.2. You can use 4.7.2 if you absolutely need it in order to compile, but some features may not work.
2. Add NeosModLoader.dll as a reference.
3. Add references to Neos libraries as needed (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Neos_Data\Managed`)
4. Remove the reference to `System.Net.Http` as it will make the compiler angry

## Hooks

### `OnEngineInit()`

Called once during FrooxEngine initialization.

Happens **before** `OnEngineInit()`

- Head device setup
- Plugin initialization

Happens **after** `OnEngineInit()`

- Local DB initialization
- Networking initialization
- Audio initialization
- Worlds loading, including Local home and Userspace

## Mod Configuration

NeosModLoader provides a built-in configuration system that can be used to persist configuration values for mods. More information is available in the [configuration system documentation](config.md).

## Example Mod

```csharp
using HarmonyLib; // HarmonyLib comes included with a NeosModLoader install
using NeosModLoader;
using System;
using System.Reflection;

namespace MyMod
{
    public class MyMod : NeosMod
    {
        public override string Name => "MyMod";
        public override string Author => "your name here";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/myName/myRepo"; // this line is optional and can be omitted

        private static bool _first_trigger = false;

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("some string unique to MyMod");
            // do whatever LibHarmony patching you need

            Debug("a debug log");
            Msg("a regular log");
            Warn("a warn log");
            Error("an error log");
        }
    }
}
```

A [Template repo](https://github.com/EIA485/NeosTemplate/) is available.

## Full Example

A working example mod is available here: <https://github.com/zkxs/MotionBlurDisable>

It showcases the following:

- A valid Visual Studio project setup
- Using LibHarmony to patch a Neos method
- Using Unity to alter all existing GameObjects of a certain type

## Additional Resources

- [Quick C# Refresher](https://learnxinyminutes.com/docs/csharp/)
- [LibHarmony Documentation](https://harmony.pardeike.net/)
- [Unity API Documentation](https://docs.unity3d.com/ScriptReference/index.html)
- [Neos Plugin Wiki Page](https://wiki.neos.com/Plugins)
