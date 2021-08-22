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
            Console.WriteLine("GameLoop.OnNewRun called");
            GameEvents.OnGameStarted(new GameEvents.GameStartedEventArgs());
        }
    }
}