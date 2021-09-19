using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JKMP.Core.Logging;
using Matchmaking.Client;
using Microsoft.Xna.Framework;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Matchmaking
{
    internal static class MatchmakingManager
    {
        public static MatchmakingClient Instance => Client;
        
        public static string? Password
        {
            get => Client.Password;
            set => Client.SetPassword(value);
        }

        private static readonly MatchmakingClient Client = new();
        private static CancellationTokenSource? matchmakingCancellationSource;
        private static AuthTicket? currentSessionTicket;

        private static readonly ILogger Logger = LogManager.CreateLogger(typeof(MatchmakingManager));

        public static void Start(Vector2 position)
        {
            var _ = StartMatchmaking(position);
        }

        public static void Stop()
        {
            Client.Disconnect();
            matchmakingCancellationSource?.Cancel();
        }
        
        private static async Task StartMatchmaking(Vector2 position)
        {
            try
            {
                matchmakingCancellationSource = new();

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
                            await Client.Connect("192.168.1.200", 16000, currentSessionTicket.Data, SteamClient.Name, GetLevelName(), Password, position, matchmakingCancellationSource.Token);
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

        private static string GetLevelName()
        {
            return "default"; // todo: get unique identifier for current level name
        }
    }
}