# Neos Standalone Setup
How to have steam hour logging, mods, and NCR simultaneously

## Setup
1. Grab and run [NeosPublicSetup.exe](https://assets.neos.com/install/NeosPublicSetup.exe) 
2. Install it wherever you want to. The default `C:\Neos` location is fine if you don't have multiple drives to worry about. If you do, check the [drive notes](#drive-notes) below. **Do not merge your standalone install into your Steam install!** While merging installs can technically work it can easily create more problems than it solves.  
   ![NeosPublicSetup.exe screenshot](img/NeosPublicSetup.png)
3. Go to the directory you installed to and run `NeosProLauncher.exe`. Wait for it to finish patching. **Do not use either of the launch buttons!**  
   ![NeosProLauncher.exe screenshot](img/NeosProLauncher.png)
4. Once the pro launcher says it's ready, simply exit it without launching.
5. Observe that an `app` directory has been created next to `NeosProLauncher.exe`. So, for example, it may be in `C:\Neos\app`. This `app` directory contains the standalone neos install, and contains the `NeosLauncher.exe` and `Neos.exe` that you are familiar with from the Steam install.
6. Go to steam and add a non-steam game.  
   ![add non-steam game screenshot](img/add_non_steam_game.png)
7. Hit the "browse" button, and go to `C:\Neos\app\Neos.exe` (or wherever is applicable given your install directory)
8. Hit the "add selected programs" button.
9.  Right click the newly added game in your library, and go to "properties"  
   ![right click properties screenshot](img/non_steam_game_properties_1.png)
11. Configure the non-steam game with the same launch options you used on the Steam game. Optionally, give it a more descriptive name and check the "Include in VR Library" checkbox.  
    ![non steam game properties screenshot](img/non_steam_game_properties_2.png)  
12. Install NeosModLoader into the standalone version [as you normally would for the steam version](../README.md#installation), but using your `C:\Neos\app` directory instead.
13. Launch Neos using your new non-steam game shortcut. Steam will track playtime on the Steam version of the game even though you are running the standalone version.

## Notes
- You will need to run `NeosProLauncher.exe` every time you want to update. You never need actually use its launch buttons, as it does not support launch options which are required for plugins/mods.
- The logs you're used to finding in `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs` will go to `C:\Neos\app\Logs` now.

## Drive Notes
- The actual Neos install should be less than 1GB, but the log files can in certain cases be very large.
- The cache goes in `%temp%\Solirax\NeosVR` by default. This can be changed with the `-CachePath <file path>` launch option. This can get very large, upwards of 30GB so make sure the drive you save cache to has plenty of space. Neos will benefit by having this on a faster drive (read: SSD). The cache folder can be deleted whenever you need without breaking Neos.
- The data goes in `%userprofile%\AppData\LocalLow\Solirax\NeosVR` by default. This can be changed with the `-DataPath <file path>` launch option. This contains your localDB as well as locally saved assets. This can get to be around 10GB, or more if you store a lot in your local home. Deleting this will:
  - Reset any non-cloud-synced Neos settings. This will, for example, send you back to the tutorial (unless you use `-SkipIntroTutorial`)
  - Reset your cloud home and nuke anything that was stored in it.
  - Regenerate your machine ID
