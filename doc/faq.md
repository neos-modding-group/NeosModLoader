# Frequently Asked Questions

## Something is broken! Where can I get help?

Please take a look at our [troubleshooting page](troubleshooting.md).

## Do you have a Discord server?

Yes. [Here it is.](https://discord.gg/vCDJK9xyvm)

## What is a mod?

Mods are .dll files loaded by NeosModLoader that change the behavior of your Neos client in some way. Unlike plugins, mods are specifically designed to work in multiplayer.

## What does NeosModLoader do?

NeosModLoader is simply a Neos [plugin](https://wiki.neos.com/Plugins) that does a lot of the boilerplate necessary to get mods working in a reasonable way. In summary, it:

1. Initializes earlier than a normal plugin
2. Ensures that Neos's compatibility check doesn't prevent you from joining other players. For safety reasons this will only work if NeosModLoader is the only plugin.
3. Loads mod .dll files and calls their `OnEngineInit()` function so the mods can begin executing

## Is using NeosModLoader allowed?

Yes, so long as Neos's [Mod & Plugin Policy] is followed.

## Will people know I'm using mods?

- By default, NeosModLoader does not do anything identifiable over the network. You will appear to be running the vanilla Neos version to any component that shows your version strings or compatibility hash.
- If you are running other plugins, they will alter your version strings and compatibility hash.
- NeosModLoader logs to the same log file Neos uses. If you send your logs to anyone it will be obvious that you are using a plugin. This is intended.
- NeosModLoader mods may have effects visible to other users, depending on the mod.
- If you wish to opt in to using your real version string you can set `advertiseversion=true` in the NeosModLoader.config file.
- If NeosModLoader breaks due to a bad install or a Neos update, it will be unable to hide its own existence.

## Are mods safe?

Mods are not sandboxed in any way. In other words, they run with the same level of privilege as Neos itself. A poorly written mod could cause performance or stability issues. A maliciously designed mod could give a malicious actor a dangerous level of control over your computer. **Make sure you only use mods from sources you trust.**

The modding community maintains [a list of mods](https://www.neosmodloader.com/mods) that have been manually audited to ensure they aren't evil. While this process isn't 100% foolproof, the mods on this list are significantly more trustworthy than an unvetted DLL.

If you aren't sure if you can trust a mod and you have some level of ability to read code, you can look at its source code. If the source code is unavailable or you suspect it may differ from the contents of the .dll file, you can inspect the mod with a [C# decompiler](https://www.google.com/search?q=c%23+decompiler). Things to be particularly wary of include:

- Obfuscated code
- Sending or receiving data over the internet
- Interacting with the file system (reading, writing, or executing files from disk)

## Where does NeosModLoader log to?

The regular Neos logs: `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs`

## Is NeosModLoader compatible with other mod loaders?

Yes, **however** other mod loaders are likely to come with LibHarmony, and you need to ensure you only have one. Therefore you may need to remove 0Harmony.dll from your Neos install directory. If the foreign mod loader's LibHarmony version is significantly different from the standard Harmony 2 library, then it will not be compatible with NeosModLoader at all.

## Why did you build a custom mod loader for Neos?

1. ~~Neos Plugins are given extra protections in the [Neos Guidelines](https://docs.google.com/document/d/1mqdbIvbj1b2LeFhNzfAASeTpRZk6vmbXISYLdTXTVR4/edit), and those same protections are not extended to a generic Unity mod loader.~~ This is no longer true, as modding Neos is now specifically allowed within Neos's [Mod & Plugin Policy].
2. As Neos Plugins are officially supported we can expect them to continue working even through major engine changes, for example if Neos ever switches to a non-Unity engine.

## As a content creator, when is a mod the right solution?

Check out this document for more detail: [Problem Solving Techniques](problem_solving_techniques.md).

## As a mod developer, why should I use NeosModLoader over a Neos Plugin?

If you are just trying to make a new component or logix node, you should use a plugin. The plugin system is specifically designed for that.

If you are trying to modify Neos's existing behavior without adding any new components, NeosModLoader offers the following:

- [LibHarmony] is a dependency of NeosModLoader, so as a mod developer you don't need to worry about making sure it's installed
- Neos Plugins normally break multiplayer compatibility. The NeosModLoader plugin has been specifically designed to remain compatible. This feature will only work if NeosModLoader.dll is the *only* plugin you are using.
- Neos Plugins can normally execute when Local Home loads at the earliest. NeosModLoader begins executing significantly earlier, giving you more room to alter Neos's behavior before it finishes initializing.
- Steam has a relatively small character limit on launch options, and every Neos plugin you install pushes you closer to that limit. Having more than a handful plugins will therefore prevent you from using Steam to launch the game, and NeosModLoader is unaffected by this issue.

## Can mods depend on other mods?

Yes. All mod assemblies are loaded before any mod hooks are called, so no special setup is needed if your mod provides public methods.

Mod hooks are called alphabetically by the mod filename, so you can purposefully alter your filename (`0_mod.dll`) to make sure your hooks run first.

## Can NeosModLoader load Neos plugins?

No. You need to use `-LoadAssembly <path>` to load plugins. There is important plugin handling code that does not run for NeosModLoader mods.

## Are NeosModLoader mods plugins?

No. NeosModLoader mods will not work if used as a Neos plugin.

<!--- Link References -->
[Mod & Plugin Policy]: https://wiki.neos.com/Mod_%26_Plugin_Policy
