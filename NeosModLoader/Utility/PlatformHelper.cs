using System.IO;

namespace NeosModLoader.Utility
{
	// Provides helper functions for platform-specific operations.
	// Used for cases such as file handling which can vary between platforms.
	internal class PlatformHelper
	{
		public static readonly string AndroidNeosPath = "/sdcard/ModData/com.Solirax.Neos";

		// Android does not support Directory.GetCurrentDirectory(), so will fall back to the root '/' directory.
		private static bool UseFallbackPath() => Directory.GetCurrentDirectory().Replace('\\', '/') == "/" && !Directory.Exists("/Neos_Data");

		public static string MainDirectory
		{
			get { return UseFallbackPath() ? AndroidNeosPath : Directory.GetCurrentDirectory(); }
		}
	}
}
