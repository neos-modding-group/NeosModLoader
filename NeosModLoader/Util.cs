using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

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
                Assembly assembly = stackTrace.GetFrame(i).GetMethod().DeclaringType.Assembly;
                if (ModLoader.AssemblyLookupMap.TryGetValue(assembly, out NeosMod mod))
                {
                    return mod;
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

    }
}
