using HarmonyLib;
using JKMP.Plugin.Multiplayer.Game.Input;
using JKMP.Plugin.Multiplayer.Game.UI;
using JumpKing.Controller;
using JumpKing.PauseMenu;
using JumpKing.Player;

namespace JKMP.Plugin.Multiplayer.Patches
{
    [HarmonyPatch(typeof(InputComponent), nameof(InputComponent.GetState))]
    internal static class InputComponentGetStatePatch
    {
        // ReSharper disable once InconsistentNaming
        private static bool Prefix(ref InputComponent.State __result)
        {
            if (PauseManager.instance.IsPaused || !InputManager.GameInputEnabled)
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(InputComponent), nameof(InputComponent.GetPressedState))]
    internal static class InputComponentGetPressedStatePatch
    {
        // ReSharper disable once InconsistentNaming
        private static bool Prefix(ref InputComponent.State __result)
        {
            if (PauseManager.instance.IsPaused || !InputManager.GameInputEnabled)
            {
                return false;
            }

            return true;
        }
    }
}