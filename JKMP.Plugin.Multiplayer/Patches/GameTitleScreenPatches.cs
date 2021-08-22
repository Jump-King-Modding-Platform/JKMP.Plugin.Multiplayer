using System;
using HarmonyLib;
using JKMP.Plugin.Multiplayer.Game.Events;
using JumpKing.GameManager.TitleScreen;

namespace JKMP.Plugin.Multiplayer.Patches
{
    [HarmonyPatch(typeof(GameTitleScreen), "OnNewRun")]
    internal static class GameTitleScreenOnNewRunPatch
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(GameTitleScreen __instance)
        {
            Console.WriteLine("GameTitleScreen.OnNewRun called");
            GameEvents.OnSceneChanged(new GameEvents.SceneChangedEventArgs(SceneType.Game));
        }
    }
}