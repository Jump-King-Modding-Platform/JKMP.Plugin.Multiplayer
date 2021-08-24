using System;
using System.Reflection;
using HarmonyLib;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Events;
using JKMP.Plugin.Multiplayer.Steam;
using JKMP.Plugin.Multiplayer.Steam.Events;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer
{
    // ReSharper disable once UnusedType.Global
    public class MultiplayerPlugin : Core.Plugins.Plugin
    {
        private readonly Harmony harmony = new("com.jkmp.plugin.multiplayer");

        private static readonly ILogger Logger = LogManager.CreateLogger<MultiplayerPlugin>();

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
                Logger.Verbose("Run started");
            };

            GameEvents.SceneChanged += args =>
            {
                Logger.Verbose("Scene changed to {sceneType}", args.SceneType);
            };
        }
    }
}