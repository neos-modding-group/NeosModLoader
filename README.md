# NeosModLoader

A mod loader for [Neos VR](https://neos.com/). Consider joining our community on [Discord][Neos Modding Discord] for support, updates, and more.

## Installation

If you are using the Steam version of Neos you are in the right place. If you are using the standalone version, read the [Neos Standalone Setup](doc/neos_standalone_setup.md) instructions. If you are on Linux, read the [Linux Notes](doc/linux.md).

1. Download [NeosModLoader.dll](https://github.com/neos-modding-group/NeosModLoader/releases/latest/download/NeosModLoader.dll) to Neos's `Libraries` folder (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Libraries`).
2. Place [0Harmony.dll](https://github.com/neos-modding-group/NeosModLoader/releases/latest/download/0Harmony.dll) into a `nml_libs` folder under your Neos install directory (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_libs`). You will need to create this folder.
3. Add mod DLL files to a `nml_mods` folder under your Neos install directory (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods`). You can create the folder if it's missing, or simply launch Neos once with NeosModLoader installed and it will be created automatically.
4. Add the following to Neos's [launch options](https://wiki.neos.com/Command_Line_Arguments): `-LoadAssembly Libraries\NeosModLoader.dll`, substituting the path for wherever you put `NeosModLoader.dll`.
5. Start the game. If you want to verify that NeosModLoader is working you can check the Neos logs. (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs`). The modloader adds some very obvious logs on startup, and if they're missing something has gone wrong. Here is an [example log file](doc/example_log.log) where everything worked correctly.

If NeosModLoader isn't working after following those steps, take a look at our [troubleshooting page](doc/troubleshooting.md).

### Example Directory Structure

Your Neos directory should now look similar to the following. Files not related to modding are not shown.

```
<Neos Install Directory>
│   Neos.exe
│   NeosLauncher.exe
│
├───Logs
│       <Log files will generate here>
│
├───nml_mods
│       InspectorScroll.dll
│       MotionBlurDisable.dll
│       NeosContactsSort.dll
|       <More mods go here>
├───nml_libs
│       0Harmony.dll
|       <More libs go here>
│
└───Libraries
        NeosModLoader.dll
```

Note that the libraries can also be in the root of the Neos install directory if you prefer, but the loading of those happens outside of NML itself.

## Finding Mods

A list of known mods is available in the [Neos Mod List](https://www.neosmodloader.com/mods). New mods and updates are also announced in [our Discord][Neos Modding Discord].

## Frequently Asked Questions

Many questions about what NML is and how it works are answered on our [frequently asked questions page](doc/faq.md).

## Making a Mod

Check out the [Mod Creation Guide](doc/making_mods.md).

## Configuration

NeosModLoader aims to have a reasonable default configuration, but certain things can be adjusted via an [optional config file](doc/modloader_config.md).

## Contributing

Issues and PRs are welcome. Please read our [Contributing Guidelines](.github/CONTRIBUTING.md)!

## Licensing and Credits

NeosModLoader is licensed under the GNU Lesser General Public License (LGPL). See [LICENSE.txt](LICENSE.txt) for the full license.

Third-party libraries distributed alongside NeosModLoader:

- [LibHarmony] ([MIT License](https://github.com/pardeike/Harmony/blob/v2.2.1.0/LICENSE))

Third-party libraries used in source:

- [.NET](https://github.com/dotnet) (Various licenses)
- [Neos VR](https://neos.com/) ([EULA](https://store.steampowered.com/eula/740250_eula_0))
- [Json.NET](https://github.com/JamesNK/Newtonsoft.Json) ([MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md))

<!--- Link References -->
[LibHarmony]: https://github.com/pardeike/Harmony
[Neos Modding Discord]: https://discord.gg/vCDJK9xyvm
