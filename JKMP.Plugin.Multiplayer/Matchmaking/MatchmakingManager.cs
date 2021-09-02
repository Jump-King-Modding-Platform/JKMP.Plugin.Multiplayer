using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JKMP.Core.Logging;
using Matchmaking.Client;
using Serilog;
using Serilog.Core;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Matchmaking
{
    internal static class MatchmakingManager
    {
        private static readonly MatchmakingClient Client = new();
        private static CancellationTokenSource? matchmakingCancellationSource;
        private static AuthTicket? currentSessionTicket;

        private static readonly ILogger Logger = LogManager.CreateLogger(typeof(MatchmakingManager));

        public static void Start()
        {
            var _ = StartMatchmaking();
        }

        public static void Stop()
        {
            Client.Disconnect();
            matchmakingCancellationSource?.Cancel();
        }
        
        private static async Task StartMatchmaking()
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
                            await Client.Connect("127.0.0.1", 16000, currentSessionTicket.Data, matchmakingCancellationSource.Token);
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