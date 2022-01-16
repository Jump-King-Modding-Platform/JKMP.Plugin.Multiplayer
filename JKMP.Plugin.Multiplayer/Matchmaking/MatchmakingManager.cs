using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using JKMP.Core.Logging;
using JumpKing.GameManager;
using JumpKing.Level;
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

        public static void Start(Vector2 position, string endpoint, ushort port)
        {
            var _ = StartMatchmaking(position, endpoint, port);
        }

        public static void Stop()
        {
            Client.Disconnect();
            matchmakingCancellationSource?.Cancel();
        }
        
        private static async Task StartMatchmaking(Vector2 position, string endpoint, ushort port)
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

                            Logger.Debug("Connecting to matchmaking server at {endpoint}:{port}...", endpoint, port);
                            await Client.Connect(endpoint,
                                port, currentSessionTicket.Data,
                                SteamClient.Name, CalculateLevelHash(false),
                                Password,
                                position,
                                matchmakingCancellationSource.Token
                            );
                        }
                        catch (SocketException ex)
                        {
                            Logger.Warning("Failed to connect to matchmaking server: {errorMessage}", ex.Message);
                        }
                        catch (MatchmakingConnectException)
                        {
                            break;
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Matchmaking thread raised an unhandled exception");
                            // Ignore for now to ensure we don't crash the game
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

        /// <param name="separateStoryLines">
        /// If true the hash will include the title screen result.
        /// This means that someone playing base game and someone playing new game+ will not matchmake.
        /// </param>
        private static string CalculateLevelHash(bool separateStoryLines)
        {
            // Calculate hash of current map data
            using MemoryStream memoryStream = new();
            using BinaryWriter writer = new(memoryStream);

            if (separateStoryLines)
                writer.Write((byte)TitleScreenResultUtil.GetTitleScreenResult());

            var screens = Traverse.Create(typeof(LevelManager)).Field<LevelScreen[]>("m_screens").Value;

            if (screens == null)
                throw new NotSupportedException("LevelManager.m_screens field not found");

            foreach (LevelScreen screen in screens)
            {
                IBlock[]? hitBoxes = Traverse.Create(screen).Field<IBlock[]>("m_hitboxes").Value;

                if (hitBoxes == null)
                    throw new NotSupportedException("LevelScreen.m_hitboxes field not found");

                foreach (IBlock hitBox in hitBoxes)
                {
                    switch (hitBox)
                    {
                        case IceBlock iceBlock:
                            writer.Write((byte)0);
                            break;
                        case NoWindBlock noWindBlock:
                            writer.Write((byte)1);
                            break;
                        case SandBlock sandBlock:
                            writer.Write((byte)2);
                            break;
                        case SnowBlock snowBlock:
                            writer.Write((byte)3);
                            break;
                        case WaterBlock waterBlock:
                            writer.Write((byte)4);
                            break;
                        case BoxBlock boxBlock:
                            writer.Write((byte)5);
                            break;
                        case QuarkBlock quarkBlock:
                            writer.Write((byte)6);
                            break;
                        case SlopeBlock slopeBlock:
                            writer.Write((byte)7);
                            break;
                        default:
                            writer.Write((byte)100);
                            break;
                    }
                    
                    Rectangle rect = hitBox.GetRect();
                    writer.Write(rect.GetHashCode());
                }
            }

            var bytes = memoryStream.ToArray();
            byte[] hashBytes = new SHA256Managed().ComputeHash(bytes);
            var builder = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; ++i)
            {
                builder.Append(hashBytes[i].ToString("X2"));
            }

            return builder.ToString();
        }
    }
}