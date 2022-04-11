# Linux Notes

NeosModLoader works on Linux, but in addition to the [normal install steps](../README.md#installation) there are some extra steps you need to take until Neos issue [#2638](https://github.com/Neos-Metaverse/NeosPublic/issues/2638) is fixed.

The log directory on Linux is `$HOME/.local/share/Steam/steamapps/common/NeosVR/Logs`

If your log contains the following, you need to set up a workaround for the issue.

```log
System.IO.DirectoryNotFoundException: Could not find a part of the path "/home/myusername/.local/share/Steam/steamapps/common/NeosVR/Neos_Data\Managed/FrooxEngine.dll".
```

To set up the workaround, run the following commands in your terminal:

```bash
cd "$HOME/.local/share/Steam/steamapps/common/NeosVR"
ln -s Neos_Data/Managed 'Neos_Data\Managed'
```
