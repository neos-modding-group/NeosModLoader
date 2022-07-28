# Neos Directories

If you've installed to a non-default location then finding the path is up to you.

| Directory | Description |
| --------- |------------ |
| Neos Install Directory | Contains the game install itself, the log directory, and the `Libraries` directory |
| Log Directory | A `Logs` directory within the Neos Install Directory. Contains the main game logs. |
| Libraries Directory | A `Libraries` directory within the  Neos Install Directory. Plugins dlls go here.
| Data Directory | Contains the local db, Unity's player.log, and local assets directory. Location can be changed with `-DataPath <path>` argument. |
| Temporary Directory | Contains crash logs and the cache |
| Cache Directory | Contains cached remote assets. Located inside the Temporary Directory by default. Location can be changed with `-CachePath <path>` argument. |

## Windows

| Description | Typical Path |
| ----------- | ------------ |
| Neos Install Directory (Steam) | `C:\Program Files (x86)\Steam\steamapps\common\NeosVR` |
| Neos Install Directory (Standalone) | `C:\Neos\app` |
| Data Directory | `%userprofile%\AppData\LocalLow\Solirax\NeosVR` |
| Temporary Directory | `%temp%\Solirax\NeosVR` |
| Cache Directory | `%temp%\Solirax\NeosVR\Cache` |

## Linux Native

| Description | Typical Path |
| ----------- | ------------ |
| Neos Install Directory (Steam) | `$HOME/.local/share/Steam/steamapps/common/NeosVR` |
| Neos Install Directory (Standalone) | *unknown* |
| Data Directory | `$HOME/.config/unity3d/Solirax/NeosVR` |
| Temporary Directory | `/tmp/Solirax/NeosVR` |
| Cache Directory | `/tmp/Solirax/NeosVR/Cache` |

## Linux Proton/WINE

| Description | Typical Path |
| ----------- | ------------ |
| Neos Install Directory (Steam) | `$HOME/.local/share/Steam/steamapps/common/NeosVR` |
| Neos Install Directory (Standalone) | *unknown* |
| Data Directory | `$HOME/.local/share/Steam/steamapps/compatdata/740250/pfx/drive_c/users/steamuser/AppData/LocalLow/Solirax/NeosVR` |
| Temporary Directory | `$HOME/.local/share/Steam/steamapps/compatdata/740250/pfx/drive_c/users/steamuser/Temp/Solirax/NeosVR` |
| Cache Directory | `$HOME/.local/share/Steam/steamapps/compatdata/740250/pfx/drive_c/users/steamuser/Temp/Solirax/NeosVR/Cache` |

## Drive Notes

- The actual Neos install should be less than 1GB, but the log files can in certain cases be very large.
- The cache can get very large, upwards of 30GB so make sure the drive you save cache to has plenty of space. Neos will benefit by having this on a faster drive (read: SSD). The cache directory can be deleted whenever you need without breaking Neos. The cache directory can be changed with the `-CachePath <file path>` launch option.
- The data directory contains your localDB as well as locally saved assets. This can get to be around 10GB, or more if you store a lot in your local home. The data directory can be changed with the `-DataPath <file path>` launch option. Deleting this will:
  - Reset any non-cloud-synced Neos settings. This will, for example, send you back to the tutorial (unless you use `-SkipIntroTutorial`)
  - Reset your cloud home and nuke anything that was stored in it.
  - Regenerate your machine ID
