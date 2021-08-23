using System;
using System.Reflection;
using HarmonyLib;
using JKMP.Plugin.Multiplayer.Game.Events;
using JKMP.Plugin.Multiplayer.Steam;
using Steamworks;

namespace JKMP.Plugin.Multiplayer
{
    // ReSharper disable once UnusedType.Global
    public class MultiplayerPlugin : Core.Plugins.Plugin
    {
        private readonly Harmony harmony = new("com.jkmp.plugin.multiplayer");

        public override void Initialize()
        {
            harmony.PatchAll(typeof(MultiplayerPlugin).Assembly);

            SteamEvents.SteamInitialized += args =>
            {
                if (args.Success)
                {
                    args.Success = SteamManager.InitializeSteam();
                }
            };

            GameEvents.RunStarted += args =>
            {
                Console.WriteLine("Run started");
            };

            GameEvents.SceneChanged += args =>
            {
                Console.WriteLine($"Scene changed to {args.SceneType}");
            };
        }
    }
}