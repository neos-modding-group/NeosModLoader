# Neos Directories

If you've installed to a non-default location then finding the path is up to you.


| Directory | Description |
| --------- |------------ |
| Neos Install Directory | Contains the game install itself, the log directory, and the libraries directory | 
| Log Directory | A `Log` folder within the Neos Install Directory. Contains the main game logs. |
| Libraries Directory | A `Libraries` folder within the  Neos Install Directory. Plugins dlls go here.
| Data Directory | Contains the local db, Unity's player.log, and local assets folder. Location can be changed with `-DataPath <path>` argument. |
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
| Data Directory | *unknown* |
| Temporary Directory | *unknown* |
| Cache Directory |*unknown* |

## Linux Proton/WINE
| Description | Typical Path |
| ----------- | ------------ |
| Neos Install Directory (Steam) | *unknown* |
| Neos Install Directory (Standalone) | *unknown* |
| Data Directory | *unknown* |
| Temporary Directory | *unknown* |
| Cache Directory |*unknown* |
