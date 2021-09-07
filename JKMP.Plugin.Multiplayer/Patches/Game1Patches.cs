using HarmonyLib;
using JKMP.Plugin.Multiplayer.Game.Events;
using JumpKing;
using Microsoft.Xna.Framework;

namespace JKMP.Plugin.Multiplayer.Patches
{
    [HarmonyPatch(typeof(Game1), "Update")]
    internal static class HookGameUpdate
    {
        public static void Prefix(GameTime gameTime)
        {
            GameEvents.OnGameUpdate(gameTime);
        }
    }
}