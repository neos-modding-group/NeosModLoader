# NeosModLoader

A mod loader for [Neos VR](https://neos.com/). 

## Installation
1. Download [NeosModLoader.dll](https://github.com/zkxs/NeosModLoader/releases/latest/download/NeosModLoader.dll) to a location of your choosing.
2. Place [0Harmony.dll](https://github.com/pardeike/Harmony/releases) in your Neos install directory (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR`).
3. Add mod DLL files to a `nml_mods` folder under your Neos install directory (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods`). You can create the folder if it's missing, or simply launch Neos once with this mod loader installed and it will be created automatically.
4. Add the following to Neos's launch options: `-LoadAssembly "C:\full\path\to\NeosModLoader.dll"`, substituting the path for wherever you put `NeosModLoader.dll`.
5. Start the game. If you want to verify that the mod loader is working you can check the Neos logs. (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs`).

## FAQ
### As a mod developer, why should I use this over a Neos Plugin?
If you are just trying to make a new component or logix node, you should use a plugin. The plugin system is specifically designed for that.

If you are trying to modify Neos's existing behavior without adding any new components, this mod loader offers the following:
- [LibHarmony](https://github.com/pardeike/Harmony) is a dependency of the mod loader, so as a mod developer you don't need to worry about making sure it's installed
- Neos Plugins normally break multiplayer compatibility. This plugin has been specifically designed to remain compatible. This feature will only work if NeosModLoader.dll is the *only* plugin you are using.
- Neos Plugins can normally execute when Local Home loads at the earliest. This Mod Loader uses a special hooking technique to begin executing significantly earlier, giving you more room to alter Neos's behavior before it finishes initializing.
- Steam has a relatively small character limit on launch options, and every Neos plugin you install pushes you closer to that limit. Having more than a few plugins is impossible for this reason, but there's no limit on how many mods this mod loader can load.

### What does this do?
This is simply a Neos [plugin](https://wiki.neos.com/Plugins) that does a lot of the boilerplate necessary to get mods working in a reasonable way. In summary, it:
1. Forces Neos to initialize it earlier than a normal plugin with a sneaky hook
1. Ensures that Neos's compatibility check doesn't prevent you from joining other players
1. Loads mod .dll files and calls their `OnEngineInit()` function so the mods can begin executing

### Does this violate the Neos Guidelines?
No. As per the [guidelines](https://docs.google.com/document/d/1mqdbIvbj1b2LeFhNzfAASeTpRZk6vmbXISYLdTXTVR4/edit#) plugins are specifically allowed.

### Why build a custom mod loader?
1. Neos Plugins do not break the Neos Guidelines. While using a generic Unity mod loader probably won't get you into trouble, it's technically against the guidelines.
1. As Neos Plugins are officially supported we can expect them to continue working even through major engine changes, for example if Neos ever switches to a non-Unity engine.

## Example Mod

1. Make a new .NET library against .NET version 4.6.2. You can use 4.7.2 if you absolutely need it in order to compile, but some features may not work.
1. Add NeosModLoader.dll as a reference.

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

A working example mod is available here: https://github.com/zkxs/MotionBlurDisable
