using System;
using System.Diagnostics;
using System.Linq;
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
using JKMP.Plugin.Multiplayer.Game.UI;
using JKMP.Plugin.Multiplayer.Matchmaking;
using JKMP.Plugin.Multiplayer.Native.Audio;
using JKMP.Plugin.Multiplayer.Networking;
using JKMP.Plugin.Multiplayer.Steam;
using JKMP.Plugin.Multiplayer.Steam.Events;
using JumpKing;
using JumpKing.Player;
using Matchmaking.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer
{
    // ReSharper disable once UnusedType.Global
    public class MultiplayerPlugin : Core.Plugins.Plugin
    {
        internal static MultiplayerPlugin Instance { get; private set; } = null!;
        
        private readonly Harmony harmony = new("com.jkmp.plugin.multiplayer");

        private static readonly ILogger Logger = LogManager.CreateLogger<MultiplayerPlugin>();

        private GameEntity? mpEntity;
        private TitleScreenEntity? titleScreenEntity;
        private MatchmakingConfig? matchmakingConfig;
        private UiConfig? uiConfig;
        private VoiceConfig? voiceConfig;

        public MultiplayerPlugin()
        {
            Instance = this;
        }

        public override void OnLoaded()
        {
            var matchmakingConfigMenu = Configs.CreateConfigMenu<MatchmakingConfig>("Matchmaking", "Matchmaking");
            matchmakingConfig = matchmakingConfigMenu.Values;

            var uiConfigMenu = Configs.CreateConfigMenu<UiConfig>("UI", "UI");
            uiConfig = uiConfigMenu.Values;

            var voiceConfigMenu = Configs.CreateConfigMenu<VoiceConfig>("Voice", "Voice");
            voiceConfig = voiceConfigMenu.Values;
        }

        public override void CreateInputActions()
        {
            InputKeys.Ptt = Input.RegisterActionWithName("PTT", "Push to talk", onlyGameInput: true, "v");
            InputKeys.OpenChat = Input.RegisterActionWithName("OpenChat", "Chat", onlyGameInput: false, "enter");
            InputKeys.NextChatChannel = Input.RegisterActionWithName("NextChatChannel", "Next chat channel", onlyGameInput: false, "tab");
            InputKeys.PrevChatChannel = Input.RegisterActionWithName("PrevChatChannel", "Previous chat channel", onlyGameInput: false, "leftcontrol + tab");
            InputKeys.SelectGlobalChat = Input.RegisterActionWithName("SelGlobalChat", "Select global chat", onlyGameInput: false, "leftcontrol + 1");
            InputKeys.SelectGroupChat = Input.RegisterActionWithName("SelGroupChat", "Select group chat", onlyGameInput: false, "leftcontrol + 2");
            InputKeys.SelectLocalChat = Input.RegisterActionWithName("SelLocalChat", "Select local chat", onlyGameInput: false, "leftcontrol + 3");
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
            GameEvents.GameInitialize += () =>
            {
                UIManager.Initialize();
                UIManager.SetScale(uiConfig!.Scale);

                SoundEffect.DistanceScale = 75;
                SoundEffect.DopplerScale = 10f;
                SoundEffect.SpeedOfSound = PlayerValues.MAX_FALL;
            };

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
            };

            GameEvents.GameDraw += gameTime =>
            {
                UIManager.Draw(gameTime);
            };
        }
    }
}