using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using JKMP.Plugin.Multiplayer.Networking.Messages.Handlers;
using JKMP.Plugin.Multiplayer.Threading;
using Matchmaking.Client.Messages.Processing;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public class P2PManager : IDisposable
    {
        public Mutex<Dictionary<ulong, RemotePlayer>> ConnectedPlayersMtx { get; } = new(new Dictionary<ulong, RemotePlayer>());
        
        private readonly Framed<GameMessagesCodec, GameMessage> messages;
        private readonly GameMessageProcessor processor;

        private static readonly ILogger Logger = LogManager.CreateLogger<P2PManager>();

        private readonly Queue<Action> pendingGameThreadActions = new();

        public P2PManager()
        {
            SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
            SteamNetworking.OnP2PConnectionFailed += OnP2PConnectionFailed;

            messages = new(new GameMessagesCodec());
            processor = new();
            Task.Run(ProcessMessages);
        }

        public void Dispose()
        {
            SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequest;
            SteamNetworking.OnP2PConnectionFailed -= OnP2PConnectionFailed;
            messages.Dispose();

            using var connectedPlayers = ConnectedPlayersMtx.Lock();
            foreach (var player in connectedPlayers.Value)
            {
                player.Value.Destroy();
                SteamNetworking.CloseP2PSessionWithUser(player.Key);
            }

            connectedPlayers.Value.Clear();
        }

        private void OnP2PConnectionFailed(SteamId steamId, P2PSessionError error)
        {
            Logger.Warning("Connection to {steamId} failed: {sessionError}", steamId, error);
            Disconnect(steamId);
        }

        private void OnP2PSessionRequest(SteamId steamId)
        {
            Logger.Verbose("P2P session request incoming from {steamId}", steamId);
            SteamNetworking.AcceptP2PSessionWithUser(steamId);
            ConnectTo(steamId);
        }

        public void ConnectTo(ICollection<SteamId> steamIds)
        {
            foreach (var steamId in steamIds)
            {
                ConnectTo(steamId);
            }
        }

        public void ConnectTo(SteamId steamId)
        {
            using (var connectedPlayers = ConnectedPlayersMtx.Lock())
            {
                if (connectedPlayers.Value.ContainsKey(steamId))
                    return;

                Logger.Verbose("Connecting to {steamId}...", steamId);
                connectedPlayers.Value.Add(steamId, new RemotePlayer(steamId));
            }

            Task.Run(async () =>
            {
                try
                {
                    AuthTicket? authTicket = await SteamUser.GetAuthSessionTicketAsync();

                    if (authTicket == null)
                    {
                        Logger.Error("Failed to get auth session ticket, aborting connection to {steamId}", steamId);
                        Disconnect(steamId);
                        return;
                    }

                    messages.Send(steamId, new HandshakeRequest
                    {
                        AuthSessionTicket = authTicket.Data
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "An unhandled exception was raised when connecting to {steamId}", steamId);
                    throw;
                }
            });
        }
        
        public void Disconnect(RemotePlayer player) => Disconnect(player.SteamId);
        public void Disconnect(SteamId steamId)
        {
            using var connectedPlayers = ConnectedPlayersMtx.Lock();
            
            if (connectedPlayers.Value.TryGetValue(steamId, out var player))
            {
                Logger.Verbose("Disconnecting from {steamId}", player.SteamId);
                player.Destroy();
                connectedPlayers.Value.Remove(steamId);
                
                SteamNetworking.CloseP2PSessionWithUser(steamId);
                player.AuthTicket!.Dispose();
                SteamUser.EndAuthSession(player.SteamId);
            }
        }

        private async Task ProcessMessages()
        {
            try
            {
                while (await messages.Next() is {} message)
                {
                    using var connectedPlayers = await ConnectedPlayersMtx.LockAsync();
                    if (connectedPlayers.Value.TryGetValue(message.Sender, out var player))
                    {
                        var context = new Context(player, messages, this);
                        await processor.HandleMessage(message, context);
                    }
                }

                Logger.Verbose("Finished message processing");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unhandled exception was raised in the message handler thread");
                throw;
            }
        }

        /// <summary>
        /// Executes the action on the game thread on the next update.
        /// </summary>
        /// <param name="action">The parameter is the delta time in the current update</param>
        public void ExecuteOnGameThread(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            pendingGameThreadActions.Enqueue(action);
        }

        public void Update(float delta)
        {
            while (pendingGameThreadActions.Count > 0)
            {
                pendingGameThreadActions.Dequeue()();
            }
        }
    }
}