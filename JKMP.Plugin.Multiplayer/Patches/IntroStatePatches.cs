using System;
using HarmonyLib;
using JKMP.Plugin.Multiplayer.Game.Events;
using JumpKing.GameManager;

namespace JKMP.Plugin.Multiplayer.Patches
{
    [HarmonyPatch(typeof(IntroState), "OnNewRun")]
    internal static class IntroStateOnNewRunPatch
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(IntroState __instance)
        {
            GameEvents.OnSceneChanged(new GameEvents.SceneChangedEventArgs(SceneType.Game));
        }
    }
}