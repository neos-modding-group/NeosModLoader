# Modloader Configuration

NeosModLoader aims to have a reasonable default configuration, but certain things can be adjusted via an optional config file. The config file does not create itself automatically, but you can create it yourself by making a `NeosModLoader.config` file in the same directory as `NeosModLoader.dll`. `NeosModLoader.config` is a simple text file that supports keys and values in the following format:

```ini
debug=true
nomods=false
```

Not all keys are required to be present. Missing keys will use the defaults outlined below:

| Configuration      | Default | Description |
| ------------------ | ------- | ----------- |
| `debug`            | `false` | If `true`, NeosMod.Debug() logs will appear in your log file. Otherwise, they are hidden. |
| `hidevisuals`      | `false` | If `true`, NML won't show a loading indicator on the splash screen. |
| `nomods`           | `false` | If `true`, mods will not be loaded from `nml_mods`. |
| `nolibraries`      | `false` | If `true`, extra libraries from `nml_libs` will not be loaded. |
| `advertiseversion` | `false` | If `false`, your version will be spoofed and will resemble `2021.8.29.1240`. If `true`, your version will be left unaltered and will resemble `2021.8.29.1240+NeosModLoader.dll`. This version string is visible to other players under certain circumstances. |
| `unsafe`           | `false` | If `true`, the version spoofing safety check is disabled and it will still work even if you have other Neos plugins. DO NOT load plugin components in multiplayer sessions, as it will break things and cause crashes. Plugin components should only be used in your local home or user space. |
| `logconflicts`     | `true`  | If `false`, conflict logging will be disabled. If `true`, potential mod conflicts will be logged. If `debug` is also `true` this will be more verbose. |
| `hidemodtypes`     | `true`  | If `true`, mod-related types will be hidden in-game. If `false`, no types will be hidden, which makes NML detectable in-game. |
| `hidelatetypes`    | `true`  | If `true` and `hidemodtypes` is `true`, late loaded types will be hidden in-game. If `false`, late loaded types will be shown |