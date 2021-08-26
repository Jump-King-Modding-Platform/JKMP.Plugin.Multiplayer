using HarmonyLib;
using JKMP.Plugin.Multiplayer.Game.Events;
using JumpKing.Player;

namespace JKMP.Plugin.Multiplayer.Patches
{
    [HarmonyPatch(typeof(JumpState), "Start")]
    internal class JumpStateStartPatch
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(JumpState __instance)
        {
            // Make sure this player has an InputComponent
            if (__instance.player.GetComponent<InputComponent>() == null)
                return;
            
            PlayerEvents.OnStartJump();
        }
    }
}