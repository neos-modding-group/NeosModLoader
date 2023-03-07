using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace NeosModLoader
{
	internal static class Util
	{
		/// <summary>
		/// Get the executing mod by stack trace analysis. Always skips the first two frames, being this method and you, the caller.
		/// You may skip extra frames if you know your callers are guaranteed to be NML code.
		/// </summary>
		/// <param name="nmlCalleeDepth">The number NML method calls above you in the stack</param>
		/// <returns>The executing mod, or null if none found</returns>
		internal static NeosMod? ExecutingMod(int nmlCalleeDepth = 0)
		{
			// example: ExecutingMod(), SourceFromStackTrace(), MsgExternal(), Msg(), ACTUAL MOD CODE
			// you'd skip 4 frames
			// we always skip ExecutingMod() and whoever called us (as this is an internal method), which is where the 2 comes from
			StackTrace stackTrace = new(2 + nmlCalleeDepth);
			for (int i = 0; i < stackTrace.FrameCount; i++)
			{
				Assembly? assembly = stackTrace.GetFrame(i)?.GetMethod()?.DeclaringType?.Assembly;
				if (assembly != null && ModLoader.AssemblyLookupMap.TryGetValue(assembly, out NeosMod mod))
				{
					return mod;
				}
			}
			return null;
		}

		/// <summary>
		/// Get the calling assembly by stack trace analysis. Always skips the first one frame, being this method and you, the caller.
		/// </summary>
		/// <param name="skipFrames">The number of extra frame skip in the stack</param>
		/// <returns>The executing mod, or null if none found</returns>
		internal static Assembly? GetCallingAssembly(int skipFrames = 0)
		{
			// same logic as ExecutingMod(), but simpler case
			StackTrace stackTrace = new(2 + skipFrames);
			for (int i = 0; i < stackTrace.FrameCount; i++)
			{
				Assembly? assembly = stackTrace.GetFrame(i)?.GetMethod()?.DeclaringType?.Assembly;
				if (assembly != null)
				{
					return assembly;
				}
			}
			return null;
		}

		/// <summary>
		/// Used to debounce a method call. The underlying method will be called after there have been no additional calls
		/// for n milliseconds.
		/// 
		/// The Action<T> returned by this function has internal state used for the debouncing, so you will need to store and reuse the Action
		/// for each call.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func">underlying function call</param>
		/// <param name="milliseconds">debounce delay</param>
		/// <returns>a debounced wrapper to a method call</returns>
		// credit: https://stackoverflow.com/questions/28472205/c-sharp-event-debounce
		internal static Action<T> Debounce<T>(this Action<T> func, int milliseconds)
		{
			// this variable gets embedded in the returned Action via the magic of closures
			CancellationTokenSource? cancelTokenSource = null;

			return arg =>
			{
				// if there's already a scheduled call, then cancel it
				cancelTokenSource?.Cancel();
				cancelTokenSource = new CancellationTokenSource();

				// schedule a new call
				Task.Delay(milliseconds, cancelTokenSource.Token)
			  .ContinueWith(t =>
			  {
				  if (t.IsCompletedSuccessfully())
				  {
					  Task.Run(() => func(arg));
				  }
			  }, TaskScheduler.Default);
			};
		}

		// shim because this doesn't exist in .NET 4.6
		private static bool IsCompletedSuccessfully(this Task t)
		{
			return t.IsCompleted && !t.IsFaulted && !t.IsCanceled;
		}

		//credit to delta for this method https://github.com/XDelta/
		internal static string GenerateSHA256(string filepath)
		{
			using var hasher = SHA256.Create();
			using var stream = File.OpenRead(filepath);
			var hash = hasher.ComputeHash(stream);
			return BitConverter.ToString(hash).Replace("-", "");
		}

		internal static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null)
		{
			return new HashSet<T>(source, comparer);
		}

		// check if a type cannot possibly have null assigned
		internal static bool CannotBeNull(Type t)
		{
			return t.IsValueType && Nullable.GetUnderlyingType(t) == null;
		}

		// check if a type is allowed to have null assigned
		internal static bool CanBeNull(Type t)
		{
			return !CannotBeNull(t);
		}

		internal static IEnumerable<Type> GetLoadableTypes(this Assembly assembly, Predicate<Type> predicate)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(type => CheckType(type, predicate));
			}
		}

		// check a potentially unloadable type to see if it is (A) loadable and (B) satsifies a predicate without throwing an exception
		// this does a series of increasingly aggressive checks to see if the type is unsafe to touch
		private static bool CheckType(Type type, Predicate<Type> predicate)
		{
			if (type == null)
			{
				return false;
			}
			try
			{
				string _name = type.Name;
			}
			catch (Exception e)
			{
				Logger.DebugInternal("Could not read the name for a type.");
				return false;
			}
			try
			{
				if (type.TypeInitializer == null)
				{
					return false;
				}
			}
			catch (Exception e)
			{
				Logger.DebugFuncInternal(() => $"Could not read TypeInitializer for type \"{type.Name}\": {e}");
				return false;
			}

			try
			{
				return predicate(type);
			}
			catch (Exception e)
			{
				Logger.DebugFuncInternal(() => $"Could not load type \"{type.Name}\": {e}");
				return false;
			}
		}
	}
}
