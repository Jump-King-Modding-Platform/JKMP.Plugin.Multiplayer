using System;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EntityComponent;
using HarmonyLib;
using JKMP.Core.Configuration;
using JKMP.Core.Configuration.UI;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Configuration;
using JKMP.Plugin.Multiplayer.Game;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Game.Events;
using JKMP.Plugin.Multiplayer.Game.Input;
using JKMP.Plugin.Multiplayer.Game.UI;
using JKMP.Plugin.Multiplayer.Matchmaking;
using JKMP.Plugin.Multiplayer.Networking;
using JKMP.Plugin.Multiplayer.Steam;
using JKMP.Plugin.Multiplayer.Steam.Events;
using JumpKing;
using JumpKing.Player;
using Matchmaking.Client;
using Microsoft.Xna.Framework;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer
{
    // ReSharper disable once UnusedType.Global
    public class MultiplayerPlugin : Core.Plugins.Plugin
    {
        internal static MultiplayerPlugin Instance { get; private set; }
        
        private readonly Harmony harmony = new("com.jkmp.plugin.multiplayer");

        private static readonly ILogger Logger = LogManager.CreateLogger<MultiplayerPlugin>();

        private GameEntity? mpEntity;
        private TitleScreenEntity? titleScreenEntity;
        private MatchmakingConfig? matchmakingConfig;

        public MultiplayerPlugin()
        {
            Instance = this;
        }

        public override void OnLoaded()
        {
            var configMenu = Configs.CreateConfigMenu<MatchmakingConfig>("Matchmaking", "Matchmaking");
            matchmakingConfig = configMenu.Values;
            MatchmakingManager.Password = configMenu.Values.Password;
            
            configMenu.PropertyChanged += OnMatchmakingConfigChanged;
        }

        private void OnMatchmakingConfigChanged(object sender, IConfigMenu.PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(matchmakingConfig.Password))
            {
                matchmakingConfig!.Password = matchmakingConfig.Password;
            }
        }

        public bool SaveMatchmakingConfig()
        {
            // Set password to UI value
            matchmakingConfig!.Password = MatchmakingManager.Password ?? "";

            try
            {
                Configs.SaveConfig(matchmakingConfig, "Matchmaking");
                return true;
            }
            catch
            {
                // Ignore, errors are logged to console for eventual troubleshooting
                return false;
            }
        }

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

            GameEvents.LoadContent += Content.LoadContent;
            GameEvents.GameInitialize += UIManager.Initialize;

            GameEvents.RunStarted += args =>
            {
                Logger.Verbose("Run started");

                var plr = EntityManager.instance.Find<PlayerEntity>();
                MatchmakingManager.Start(plr.GetComponent<BodyComp>().position, matchmakingConfig!.Endpoint, matchmakingConfig.Port);
                mpEntity = new();
            };

            GameEvents.SceneChanged += args =>
            {
                Logger.Verbose("Scene changed to {sceneType}", args.SceneType);

                if (args.SceneType == SceneType.TitleScreen)
                {
                    mpEntity?.Destroy();
                    mpEntity = null;
                    MatchmakingManager.Stop();
                    titleScreenEntity = new();
                }
                else if (args.SceneType == SceneType.Game)
                {
                    titleScreenEntity?.Destroy();
                    titleScreenEntity = null;
                    // game entity is created in the RunStarted event above
                }
            };

            GameEvents.GameUpdate += gameTime =>
            {
                SteamClient.RunCallbacks();

                InputManager.Update(gameTime);
            };

            GameEvents.GameDraw += gameTime =>
            {
                UIManager.Draw(gameTime);
            };
        }
    }
}