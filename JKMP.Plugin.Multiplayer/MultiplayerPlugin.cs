using System;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EntityComponent;
using HarmonyLib;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Game.Events;
using JKMP.Plugin.Multiplayer.Matchmaking;
using JKMP.Plugin.Multiplayer.Steam;
using JKMP.Plugin.Multiplayer.Steam.Events;
using JumpKing;
using JumpKing.Player;
using Matchmaking.Client;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer
{
    // ReSharper disable once UnusedType.Global
    public class MultiplayerPlugin : Core.Plugins.Plugin
    {
        private readonly Harmony harmony = new("com.jkmp.plugin.multiplayer");

        private static readonly ILogger Logger = LogManager.CreateLogger<MultiplayerPlugin>();

        private GameEntity? mpEntity;
        private TitleScreenEntity? titleScreenEntity;

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

                var plr = EntityManager.instance.Find<PlayerEntity>();
                MatchmakingManager.Start(plr.GetComponent<BodyComp>().position);
                mpEntity = new();
            };

            GameEvents.SceneChanged += args =>
            {
                Logger.Verbose("Scene changed to {sceneType}", args.SceneType);

                if (args.SceneType == SceneType.TitleScreen)
                {
                    mpEntity = null;
                    MatchmakingManager.Stop();
                    titleScreenEntity = new();
                }
                else if (args.SceneType == SceneType.Game)
                {
                    titleScreenEntity = null;
                    // game entity is created in the RunStarted event above
                }
            };
        }
    }
}