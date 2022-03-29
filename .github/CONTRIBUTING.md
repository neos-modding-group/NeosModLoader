# NeosModLoader Contributing Guidelines

If you are interested in contributing to this project via issues or PRs, please follow these guidelines.

## Issue Guidelines

If you are reporting a bug, please include a log, or at the very least the relevant stack trace. NeosModLoader logs to the Neos log by default (`C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs`).

## PR Guidelines

If you want your PR to be approved and merged quickly, please read the following:

### Branches

PRs should be made onto the `dev` branch. PRs made on to other branches, including master, will be subject to delays as changing their base branch to `dev` may introduce merge conflicts.

### Code Style

If your PR does not follow the project's code style it **will not be approved**. I expect contributors to be capable of fixing code style issues on their own.

Code style should match Microsoft convention. If you aren't sure, run Analyze > Code Cleanup on the file using Visual Studio.

#### Whitespace

- *.cs files
  - Lines MUST be indented using four spaces. Tabs are not acceptable.
  - MUST use unix-style (LF) line endings. CR or CRLF are not acceptable.
- *.sln files: use the Visual Studio defaults
  - indent using tabs
  - CRLF line endings
- *.csproj files: use the Visual Studio defaults
  - indent using two spaces
  - CRLF line endings

### New Features

Please consider the NML [design goals](#design-goals) before adding a new feature. If you aren't sure if your new feature makes sense for NML, I'm happy to talk with you about a potential new feature in our [discussions area](https://github.com/zkxs/NeosModLoader/discussions).

## Design Goals

- NML should be kept as simple as possible. Its purpose is to load mods, not mod Neos itself. Additionally, NML is not intended to fix Neos bugs. Please do not attempt to add Neos bugfixes directly to NML. Instead, ensure there's an issue open on [the Neos issue tracker](https://github.com/Neos-Metaverse/NeosPublic/issues), and only consider making a mod if the Neos team is unable to provide a fix in a reasonably timeframe.
- NML should only create APIs where the added API complexity is paid for by added ease of development for mod creators. If a proposed API is only useful to a very small percentage of mods, NeosModLoader probably isn't the place for it.
- NML should try to prevent mod developers from shooting themselves in the foot. For example, NML only supports mods with a single NeosMod implementation. Instead of silently ignoring extra implementations in a mod, NML will instead throw an error message and abort loading the mod entirely.
