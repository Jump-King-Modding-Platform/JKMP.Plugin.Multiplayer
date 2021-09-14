using HarmonyLib;
using JKMP.Plugin.Multiplayer.Game.Events;
using JumpKing;
using Microsoft.Xna.Framework;

namespace JKMP.Plugin.Multiplayer.Patches
{
    [HarmonyPatch(typeof(Game1), "Update")]
    internal static class HookGameUpdatePatch
    {
        public static void Prefix(GameTime gameTime)
        {
            GameEvents.OnGameUpdate(gameTime);
        }
    }
    
    [HarmonyPatch(typeof(Game1), "LoadContent")]
    internal class HookLoadContentPatch
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(Game1 __instance)
        {
            GameEvents.OnLoadContent(__instance.Content);
        }
    }
}