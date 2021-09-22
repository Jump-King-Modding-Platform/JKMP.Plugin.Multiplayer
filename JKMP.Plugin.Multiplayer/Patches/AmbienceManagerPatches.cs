using HarmonyLib;
using JumpKing;

namespace JKMP.Plugin.Multiplayer.Patches
{
    /// Prevent ambience from being paused when the pause menu is opened
    [HarmonyPatch(typeof(AmbienceManager.AmbienceScreen), nameof(AmbienceManager.AmbienceScreen.Pause))]
    internal static class AmbienceOnPausePatch
    {
        private static bool Prefix()
        {
            return false;
        }
    }
}