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
        private MatchmakingClient matchmakingClient = new();
        private CancellationTokenSource? matchmakingCancellationSource;
        private AuthTicket? currentSessionTicket;

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

                var _ = StartMatchmaking();
                mpEntity = new();
            };

            GameEvents.SceneChanged += args =>
            {
                Logger.Verbose("Scene changed to {sceneType}", args.SceneType);

                if (args.SceneType == SceneType.TitleScreen)
                {
                    mpEntity = null;
                    matchmakingClient.Disconnect();
                    matchmakingCancellationSource?.Cancel();
                    titleScreenEntity = new();
                }
                else if (args.SceneType == SceneType.Game)
                {
                    titleScreenEntity = null;
                    // game entity is created in the RunStarted event above
                }
            };
        }

        private async Task StartMatchmaking()
        {
            try
            {
                matchmakingCancellationSource = new();
                matchmakingClient = new();

                var jobTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            Logger.Debug("Acquiring steam auth session ticket...");
                            currentSessionTicket = await SteamUser.GetAuthSessionTicketAsync();

                            if (matchmakingCancellationSource.IsCancellationRequested)
                                break;

                            if (currentSessionTicket == null)
                            {
                                Logger.Error("Failed to retrieve steam auth session ticket, retrying...");
                                continue;
                            }
                            
                            Logger.Debug("Connecting to matchmaking server...");
                            await matchmakingClient.Connect("127.0.0.1", 16000, currentSessionTicket.Data, matchmakingCancellationSource.Token);
                        }
                        catch (SocketException ex)
                        {
                            Logger.Warning("Failed to connect to matchmaking server: {errorMessage}", ex.Message);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }

                        if (matchmakingCancellationSource.IsCancellationRequested)
                            break;

                        Logger.Warning("Reconnecting to matchmaking server in 5 seconds...");

                        try
                        {
                            await Task.Delay(millisecondsDelay: 5000, matchmakingCancellationSource.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                });

                await jobTask;
                currentSessionTicket?.Dispose();
                currentSessionTicket = null;
                Logger.Debug("Disconnected from matchmaking server");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Matchmaking task raised an unhandled exception");
            }
        }
    }
}