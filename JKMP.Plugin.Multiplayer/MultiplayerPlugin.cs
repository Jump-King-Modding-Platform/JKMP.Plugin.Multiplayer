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
using JKMP.Plugin.Multiplayer.Game.Input;
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
        private AudioCaptureContext? audioCapture;
        private OpusContext? opus;

        private DynamicSoundEffectInstance? micPlayback;

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

                audioCapture = new AudioCaptureContext();
                var devices = audioCapture.GetOutputDevices();

                foreach (var device in devices)
                {
                    Logger.Debug("Audio device: {deviceName}", device.Name);
                }

                audioCapture.SetActiveDeviceToDefault();

                opus = new(48000);
                micPlayback = new(48000, AudioChannels.Mono);
                
                bool startedCapture = audioCapture.StartCapture(uncompressedData =>
                {
                    // Compress audio
                    Span<byte> compressedBuffer = new(new byte[uncompressedData.Length]);
                    int numBytes = opus.Compress(uncompressedData, compressedBuffer);
                    
                    if (numBytes < 0)
                    {
                        Logger.Error("Failed to compress audio data: {error}", numBytes);
                        return;
                    }
                    
                    Span<byte> compressedData = compressedBuffer.Slice(0, numBytes);

                    // Decompress audio
                    Span<short> decompressedBuffer = new(new short[uncompressedData.Length]);
                    numBytes = opus.Decompress(compressedData, decompressedBuffer);
                    
                    if (numBytes < 0)
                    {
                        Logger.Error("Failed to decompress audio data: {error}", numBytes);
                        return;
                    }

                    Span<short> decompressedData = decompressedBuffer.Slice(0, numBytes);
                    
                    unsafe
                    {
                        fixed (short* ptr = &decompressedData.GetPinnableReference())
                        {
                            Span<byte> bytes = new(ptr, decompressedData.Length * sizeof(short));
                            micPlayback.SubmitBuffer(bytes.ToArray());
                            micPlayback.Play();
                        }
                    }
                }, error =>
                {
                    Logger.Warning("An error occurred while capturing audio: {error}", error);
                });

                Logger.Debug("Started audio capture: {startedCapture}", startedCapture);
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

                InputManager.Update(gameTime);
            };

            GameEvents.GameDraw += gameTime =>
            {
                UIManager.Draw(gameTime);
            };
        }
    }
}