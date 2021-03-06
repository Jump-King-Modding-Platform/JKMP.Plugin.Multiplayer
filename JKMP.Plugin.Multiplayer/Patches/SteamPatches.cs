using System;
using System.Threading;
using HarmonyLib;
using JKMP.Plugin.Multiplayer.Game.Events;
using JKMP.Plugin.Multiplayer.Steam.Events;
using JumpKing;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Patches
{
    [HarmonyPatch(typeof(Program), "InitSteam")]
    internal static class InitSteamPatch
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(ref bool __result)
        {
            var eventArgs = new SteamEvents.SteamInitializedEventArgs(__result);
            SteamEvents.OnSteamInitialized(eventArgs);
            __result = eventArgs.Success;
        }
    }

    [HarmonyPatch(typeof(Program), "Run")]
    internal static class DestroySteamPatch
    {
        private static void Postfix()
        {
            if (SteamClient.IsValid)
                SteamClient.Shutdown();
        }
    }
}