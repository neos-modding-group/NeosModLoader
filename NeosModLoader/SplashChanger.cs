using FrooxEngine;
using System;
using System.Reflection;

namespace NeosModLoader
{
	// Custom splash screen logic failing shouldn't fail the rest of the modloader.
	// Keep that in mind when editing later on.
	internal static class SplashChanger
	{
		private static bool failed = false;

		private static MethodInfo? _updatePhase = null;
		private static MethodInfo? UpdatePhase
		{
			get
			{
				if (_updatePhase is null)
				{
					try
					{
						_updatePhase = typeof(Engine)
							.GetMethod("UpdateInitPhase", BindingFlags.NonPublic | BindingFlags.Instance);
					}
					catch (Exception ex)
					{
						if (!failed)
						{
							Logger.WarnInternal("UpdatePhase not found: " + ex.ToString());
						}
						failed = true;
					}
				}
				return _updatePhase;
			}
		}
		private static MethodInfo? _updateSubPhase = null;
		private static MethodInfo? UpdateSubPhase
		{
			get
			{
				if (_updateSubPhase is null)
				{
					try
					{
						_updateSubPhase = typeof(Engine)
							.GetMethod("UpdateInitSubphase", BindingFlags.NonPublic | BindingFlags.Instance);
					}
					catch (Exception ex)
					{
						if (!failed)
						{
							Logger.WarnInternal("UpdateSubPhase not found: " + ex.ToString());
						}
						failed = true;
					}
				}
				return _updateSubPhase;
			}
		}

		// Returned true means success, false means something went wrong.
		internal static bool SetCustom(string text)
		{
			if (ModLoaderConfiguration.Get().HideVisuals) return true;
			try
			{
				// VerboseInit does extra logging, so turning it if off while we change the phase.
				bool ogVerboseInit = Engine.Current.VerboseInit;
				Engine.Current.VerboseInit = false;
				UpdatePhase?.Invoke(Engine.Current, new object[] { "~ NeosModLoader ~", false });
				UpdateSubPhase?.Invoke(Engine.Current, new object[] { text, false });
				Engine.Current.VerboseInit = ogVerboseInit;
				return true;
			}
			catch (Exception ex)
			{
				if (!failed)
				{
					Logger.WarnInternal("Splash change failed: " + ex.ToString());
					failed = true;
				}
				return false;
			}
		}
	}
}
