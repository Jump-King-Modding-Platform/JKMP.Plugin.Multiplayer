using System;
using HarmonyLib;
using JKMP.Plugin.Multiplayer.Game.Events;
using JumpKing.GameManager;

namespace JKMP.Plugin.Multiplayer.Patches
{
    [HarmonyPatch(typeof(GameLoop), "OnNewRun")]
    internal static class GameLoopOnNewRunPatch
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(GameLoop __instance)
        {
            GameEvents.OnRunStarted(new GameEvents.RunStartedEventArgs());
        }
    }
}