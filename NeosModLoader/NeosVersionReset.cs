using BaseX;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace NeosModLoader
{
    internal class NeosVersionReset
    {
        internal static void Initialize()
        {
            Engine engine = Engine.Current;
            List<string> extraAssemblies = Engine.ExtraAssemblies;

            if (!extraAssemblies.Remove("NeosModLoader.dll"))
            {
                Logger.ErrorInternal("assertion failed: Engine.ExtraAssemblies did not contain NeosModLoader.dll");
                return;
            }

            if (Configuration.get().UnsafeMode)
            {
                Logger.WarnInternal("Unsafe mode is on! Beware of using plugin components in multiplayer!");
                extraAssemblies.Clear();
            }
            else if (extraAssemblies.Count != 0)
            {
                Logger.ErrorInternal("NeosModLoader.dll must be your only plugin to keep multiplayer compatibility!");
                return;
            }

            string vanillaCompatibilityHash;
            int? vanillaProtocolVersionMaybe = GetVanillaProtocolVersion();
            if (vanillaProtocolVersionMaybe is int vanillaProtocolVersion)
            {
                Logger.DebugInternal(string.Format("vanilla protocol version is {0}", vanillaProtocolVersion));
                vanillaCompatibilityHash = CalculateCompatibilityHash(vanillaProtocolVersion);
            }
            else
            {
                Logger.ErrorInternal("unable to determine vanilla protocol version");
                return;
            }

            // we intentionally attempt to set the version string first, so if it fails the compatibilty hash is left on the original value
            // this is to prevent the case where a player simply doesn't know their version string is wrong
            bool success = SetVersionString(engine);
            if (success)
            {
                success = SetCompatibilityHash(engine, vanillaCompatibilityHash);
            }
            if (success)
            {
                Logger.MsgInternal("version spoofing succeeded");
            }
            else
            {
                Logger.WarnInternal("version spoofing failed");
            }
        }

        private static string CalculateCompatibilityHash(int ProtocolVersion)
        {
            using (MD5CryptoServiceProvider cryptoServiceProvider = new MD5CryptoServiceProvider())
            {
                ConcatenatedStream concatenatedStream = new ConcatenatedStream();
                concatenatedStream.EnqueueStream(new MemoryStream(BitConverter.GetBytes(ProtocolVersion)));
                byte[] hash = cryptoServiceProvider.ComputeHash(concatenatedStream);
                return Convert.ToBase64String(hash);
            }
        }

        private static bool SetCompatibilityHash(Engine engine, string Target)
        {
            // This is super sketchy and liable to break with new compiler versions.
            // I have a good reason for doing it though... if I just called the setter it would recursively
            // end up calling itself, because I'm HOOKINGthe CompatibilityHash setter.
            FieldInfo field = AccessTools.DeclaredField(typeof(Engine), string.Format("<{0}>k__BackingField", nameof(Engine.CompatibilityHash)));

            if (field == null)
            {
                Logger.WarnInternal("unable to write Engine.CompatibilityHash");
                return false;
            }
            else
            {
                Logger.DebugInternal(string.Format("changing compatibility hash from {0} to {1}", engine.CompatibilityHash, Target));
                field.SetValue(engine, Target);
                return true;
            }
        }
        private static bool SetVersionString(Engine engine)
        {
            // calculate correct version string
            string target = Engine.VersionNumber;

            if (!engine.VersionString.Equals(target))
            {
                FieldInfo field = AccessTools.DeclaredField(engine.GetType(), "_versionString");
                if (field == null)
                {
                    Logger.WarnInternal("unable to write Engine._versionString");
                    return false;
                }
                Logger.DebugInternal(string.Format("changing version string from {0} to {1}", engine.VersionString, target));
                field.SetValue(engine, target);
            }
            return true;
        }

        // perform incredible bullshit to rip the hardcoded protocol version out of the dang IL
        private static int? GetVanillaProtocolVersion()
        {
            // raw IL immediately surrounding the number we need to find, which in this example is 770
            // ldc.i4       770
            // call         unsigned int8[] [mscorlib]System.BitConverter::GetBytes(int32)

            // we're going to search for that method call, then grab the operand of the ldc.i4 that precedes it

            MethodInfo targetCallee = AccessTools.DeclaredMethod(typeof(BitConverter), nameof(BitConverter.GetBytes), new Type[] { typeof(int) });
            if (targetCallee == null)
            {
                Logger.ErrorInternal("could not find System.BitConverter::GetBytes(System.Int32)");
                return null;
            }

            MethodInfo initializeShim = AccessTools.DeclaredMethod(typeof(Engine), nameof(Engine.Initialize));
            if (initializeShim == null)
            {
                Logger.ErrorInternal("could not find Engine.Initialize(*)");
                return null;
            }

            AsyncStateMachineAttribute asyncAttribute = (AsyncStateMachineAttribute)initializeShim.GetCustomAttribute(typeof(AsyncStateMachineAttribute));
            if (asyncAttribute == null)
            {
                Logger.ErrorInternal("could not find AsyncStateMachine for Engine.Initialize");
                return null;
            }

            // async methods are weird. Their body is just some setup code that passes execution... elsewhere.
            // The compiler generates a companion type for async methods. This companion type has some ridiculous nondeterministic name, but luckily
            // we can just ask this attribute what the type is. The companion type should have a MoveNext() method that contains the actual IL we need.
            Type asyncStateMachineType = asyncAttribute.StateMachineType;
            MethodInfo initializeImpl = AccessTools.DeclaredMethod(asyncStateMachineType, "MoveNext");
            if (initializeImpl == null)
            {
                Logger.ErrorInternal("could not find MoveNext method for Engine.Initialize");
                return null;
            }

            List<CodeInstruction> instructions = PatchProcessor.GetOriginalInstructions(initializeImpl);
            for (int i = 1; i < instructions.Count; i++)
            {
                if (instructions[i].Calls(targetCallee))
                {
                    // we're guaranteed to have a previous instruction because we began iteration from 1
                    CodeInstruction previous = instructions[i - 1];
                    if (OpCodes.Ldc_I4.Equals(previous.opcode))
                    {
                        return (int)previous.operand;
                    }
                }
            }

            return null;
        }
    }
}
