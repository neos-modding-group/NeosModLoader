# NeosModLoader

A mod loader for [Neos VR](https://neos.com/).

## Installation
1. Download [NeosModLoader.dll](https://github.com/zkxs/NeosModLoader/releases/latest/download/NeosModLoader.dll) to a location of your choosing.
2. Place [0Harmony.dll](https://github.com/zkxs/NeosModLoader/releases/download/1.0.0.0/0Harmony.dll) in your Neos install directory (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR`).
3. Add mod DLL files to a `nml_mods` folder under your Neos install directory (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods`). You can create the folder if it's missing, or simply launch Neos once with NeosModLoader installed and it will be created automatically.
4. Add the following to Neos's launch options: `-LoadAssembly "C:\full\path\to\NeosModLoader.dll"`, substituting the path for wherever you put `NeosModLoader.dll`.
5. Start the game. If you want to verify that NeosModLoader is working you can check the Neos logs. (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs`).

## Mods
A list of known mods is available [here](https://github.com/zkxs/neos-mod-list/blob/master/README.md)!

## FAQ
### What does NeosModLoader do?
NeosModLoader is simply a Neos [plugin](https://wiki.neos.com/Plugins) that does a lot of the boilerplate necessary to get mods working in a reasonable way. In summary, it:
1. Initializes earlier than a normal plugin
2. Ensures that Neos's compatibility check doesn't prevent you from joining other players. For safety reasons this will only work if NeosModLoader is the only plugin.
3. Loads mod .dll files and calls their `OnEngineInit()` function so the mods can begin executing

### I can't join sessions because my version doesn't match!
Make sure NeosModLoader is the only plugin being loaded. For safety reasons NeosModLoader will only bypass the plugin compatibility check for itself, not other plugins.

### Something is broken! Where can I get help?
1. Check the logs (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs`) for clues as to what is going wrong.
2. Disable NeosModLoader by removing the `-LoadAssembly <path>` launch option. If Neos is still having problems while unmodified, you can get support on the [Neos Discord](https://discordapp.com/invite/GQ92NUu5). You should not ask the Neos Discord for help with mods.
3. If you only experience the problem while modded, try uninstalling all of your mods and re-installing them one by one. Once you find the problematic mod reach out it its developers.
4. If you are having an issue with NeosModLoader itself, please open [an issue](https://github.com/zkxs/NeosModLoader/issues).

### Does NeosModLoader violate the Neos Guidelines?
Sort answer: maybe?  
Long answer: [see here](doc/neos_guidelines.md).

### Will people know I'm using mods?
- NeosModLoader itself does not do anything identifiable over the network. You will appear to be running the vanilla Neos version to any component that shows your version strings or compatibility hash.
- If you are running other plugins, they will alter your version strings and compatibility hash.
- NeosModLoader logs to the same log file Neos uses. If you send your logs to anyone it will be obvious that you are using a plugin. This is intended.
- NeosModLoader mods may have effects visible to other users, depending on the mod.

### Are mods safe? 
Mods are not sandboxed in any way. In other words, they run with the same level of privilege as Neos itself. A poorly written mod could cause performance or stability issues. A maliciously designed mod could give a malicious actor a dangerous level of control over your computer. **Make sure you only use mods from sources you trust.**

If you aren't sure if you can trust a mod and you have some level of ability to read code, you can look at its source code. If the source code is unavailable or you suspect it may differ from the contents of the .dll file, you can inspect the mod with a [C# decompiler](https://www.google.com/search?q=c%23+decompiler). Things to be particularly wary of include:
- Obfuscated code
- Sending or receiving data over the internet
- Interacting with the file system (reading, writing, or executing files from disk)

### Where does NeosModLoader log to?
The regular Neos logs: `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs`

### Is NeosModLoader compatible with other mod loaders?
Yes, **however** other mod loaders are likely to come with LibHarmony, and you need to ensure you only have one. Therefore you may need to remove 0Harmony.dll from your Neos install directory. If the foreign mod loader's LibHarmony version is significantly different from the standard Harmony 2 library, then it will not be compatible with NeosModLoader at all.

### Why did you build a custom mod loader for Neos?
1. Neos Plugins are given extra protections in the [Neos Guidelines](https://docs.google.com/document/d/1mqdbIvbj1b2LeFhNzfAASeTpRZk6vmbXISYLdTXTVR4/edit), and those same protections are not extended to a generic Unity mod loader.
2. As Neos Plugins are officially supported we can expect them to continue working even through major engine changes, for example if Neos ever switches to a non-Unity engine.

### As a mod developer, why should I use NeosModLoader over a Neos Plugin?
If you are just trying to make a new component or logix node, you should use a plugin. The plugin system is specifically designed for that.

If you are trying to modify Neos's existing behavior without adding any new components, NeosModLoader offers the following:
- [LibHarmony](https://github.com/pardeike/Harmony) is a dependency of NeosModLoader, so as a mod developer you don't need to worry about making sure it's installed
- Neos Plugins normally break multiplayer compatibility. The NeosModLoader plugin has been specifically designed to remain compatible. This feature will only work if NeosModLoader.dll is the *only* plugin you are using.
- Neos Plugins can normally execute when Local Home loads at the earliest. NeosModLoader begins executing significantly earlier, giving you more room to alter Neos's behavior before it finishes initializing.
- Steam has a relatively small character limit on launch options, and every Neos plugin you install pushes you closer to that limit. Having more than a handful plugins will therefore prevent you from using Steam to launch the game, and NeosModLoader is unaffected by this issue.

### Can mods depend on other mods?
Yes. All mod assemblies are loaded before any mod hooks are called, so no special setup is needed if your mod provides public methods.

Mod hooks are called alphabetically by the mod filename, so you can purposefully alter your filename (`0_mod.dll`) to make sure your hooks run first.

### Can NeosModLoader load Neos plugins?
No. You need to use `-LoadAssembly <path>` to load plugins. There is important plugin handling code that does not run for NeosModLoader mods.

### Are NeosModLoader mods plugins?
No. NeosModLoader mods will not work if used as a Neos plugin.

## Making a Mod
If you have some level of familiarity with C#, getting started making mods should not be too difficult.
### Basic Visual Studio setup
1. Make a new .NET library against .NET version 4.6.2. You can use 4.7.2 if you absolutely need it in order to compile, but some features may not work.
2. Add NeosModLoader.dll as a reference.
3. Add references to Neos libraries as needed (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Neos_Data\Managed`)
4. Remove the reference to `System.Net.Http` as it will make the compiler angry

### Hooks
#### `OnEngineInit()`
Called once during FrooxEngine initialization.

Happens **before** `OnEngineInit()`
- Head device setup
- Plugin initialization

Happens **after** `OnEngineInit()`
- Local DB initialization
- Networking initialization
- Audio initialization
- Worlds loading, including Local home and Userspace

### Example Mod

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

### Full Example
A working example mod is available here: https://github.com/zkxs/MotionBlurDisable

It showcases the following:
- A valid Visual Studio project setup
- Using LibHarmony to patch a Neos method
- Using Unity to alter all existing GameObjects of a certain type

### Additional Resources
- [Quick C# Refresher](https://learnxinyminutes.com/docs/csharp/)
- [LibHarmony Documentation](https://harmony.pardeike.net/)
- [Unity API Documentation](https://docs.unity3d.com/ScriptReference/index.html)
- [Neos Plugin Wiki Page](https://wiki.neos.com/Plugins)

## Configuration
NeosModLoader aims to have a reasonable default configuration, but certain things can be changed via a config file.
The `NeosModLoader.config` file should be placed in the same directory as `NeosModLoader.dll`, and contains keys and values in the following format:
```
debug=true
nomods=false
```

| Configuration | Default | Description |
| ------------- | ------- | ----------- |
| `debug`       | `false` | if `true`, NeosMod.Debug() logs will appear in your log file. Otherwise, they are hidden. |
| `nomods`      | `false` | if `true`, mods will not be loaded. |
