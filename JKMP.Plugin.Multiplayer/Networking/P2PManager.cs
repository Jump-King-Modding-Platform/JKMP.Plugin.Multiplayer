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
        public static P2PManager? Instance { get; private set; }
        
        public Mutex<Dictionary<ulong, RemotePlayer>> ConnectedPlayersMtx { get; } = new(new Dictionary<ulong, RemotePlayer>());
        
        public P2PEvents Events { get; private set; }
        
        private readonly Framed<GameMessagesCodec, GameMessage> messages;
        private readonly GameMessageProcessor processor;

        private static readonly ILogger Logger = LogManager.CreateLogger<P2PManager>();

        private readonly Queue<Action> pendingGameThreadActions = new();

        private readonly HashSet<(DateTime, SteamId)> recentlyDisconnectedPeers = new();

        public P2PManager()
        {
            SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
            SteamNetworking.OnP2PConnectionFailed += OnP2PConnectionFailed;

            messages = new(new GameMessagesCodec());
            processor = new();
            Events = new();
            Task.Run(ProcessMessages);

            Instance = this;
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
            Instance = null;
        }

        private void OnP2PConnectionFailed(SteamId steamId, P2PSessionError error)
        {
            Logger.Warning("Connection to {steamId} failed: {sessionError}", steamId, error);
            Disconnect(steamId);
        }

        private void OnP2PSessionRequest(SteamId steamId)
        {
            if (recentlyDisconnectedPeers.Any(t => t.Item2 == steamId))
                return;

            Logger.Verbose("P2P session request incoming from {steamId}", steamId);
            SteamNetworking.AcceptP2PSessionWithUser(steamId);
            ConnectTo(steamId);
        }

        public void ConnectTo(IEnumerable<SteamId> steamIds)
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
                    using var guard = await ConnectedPlayersMtx.LockAsync();
                    var remotePlayer = guard.Value[steamId];
                    AuthTicket? authTicket = await SteamUser.GetAuthSessionTicketAsync();

                    if (authTicket == null)
                    {
                        guard.Dispose();
                        
                        Logger.Error("Failed to get auth session ticket, aborting connection to {steamId}", steamId);
                        Disconnect(steamId);
                        return;
                    }

                    remotePlayer.AuthTicket = authTicket;

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
                recentlyDisconnectedPeers.Add((DateTime.UtcNow, steamId));

                messages.Send(steamId, new Disconnected(), P2PSend.UnreliableNoDelay, 1);
                SteamNetworking.CloseP2PSessionWithUser(steamId);
                player.AuthTicket?.Dispose();
                SteamUser.EndAuthSession(player.SteamId);
            }
        }

        public void DisconnectAll()
        {
            Logger.Verbose("Disconnecting all P2P clients");
            
            using var connectedPlayers = ConnectedPlayersMtx.Lock();
            foreach (var kv in connectedPlayers.Value)
            {
                messages.Send(kv.Key, new Disconnected(), P2PSend.UnreliableNoDelay, 1);
                kv.Value.Destroy();
                SteamNetworking.CloseP2PSessionWithUser(kv.Key);
                kv.Value.AuthTicket!.Dispose();
                SteamUser.EndAuthSession(kv.Key);

                recentlyDisconnectedPeers.Add((DateTime.UtcNow, kv.Key));
            }

            connectedPlayers.Value.Clear();
        }

        private async Task ProcessMessages()
        {
            try
            {
                var context = new Context(messages, this);
                
                while (await messages.Next() is {} message)
                {
                    if (message is not PlayerStateChanged)
                        Logger.Verbose("Incoming message {message}", message.GetType().Name);

                    processor.PushMessage(message, context);
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
            
            if (recentlyDisconnectedPeers.Count > 0)
            {
                var now = DateTime.UtcNow;
                var toRemove = new List<(DateTime, SteamId)>();
                
                foreach ((DateTime, SteamId) item in recentlyDisconnectedPeers)
                {
                    if ((now - item.Item1).TotalSeconds > 1d)
                    {
                        toRemove.Add(item);
                    }
                }

                recentlyDisconnectedPeers.RemoveWhere(item => toRemove.Contains(item));
            }

            processor.HandlePendingMessages().Wait();
        }

        internal void Broadcast(GameMessage message, P2PSend sendType = P2PSend.Reliable) => BroadcastAsync(message, sendType).Wait();
        internal async Task BroadcastAsync(GameMessage message, P2PSend sendType = P2PSend.Reliable)
        {
            byte[] bytes = messages.Encode(message);
            
            using var guard = await ConnectedPlayersMtx.LockAsync();
            foreach (RemotePlayer client in guard.Value.Values)
            {
                if (client.State != PlayerNetworkState.Connected)
                    continue;
                
                messages.Send(client.SteamId, bytes, sendType);
            }
        }
    }
}