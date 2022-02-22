using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Collections;
using JKMP.Plugin.Multiplayer.Memory;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using JKMP.Plugin.Multiplayer.Networking.Messages.Handlers;
using Serilog;
using Steamworks;
using Steamworks.Data;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public partial class P2PManager : IDisposable
    {
        public const ushort VirtualPort = 1;
        
        public static P2PManager? Instance { get; private set; }

        public Dictionary<NetIdentity, RemotePlayer> ConnectedPlayers { get; } = new();

        public P2PEvents Events { get; private set; }
        
        private readonly GameMessageProcessor processor;

        private static readonly ILogger Logger = LogManager.CreateLogger<P2PManager>();

        private readonly Queue<(Action action, TaskCompletionSource<bool> tcs)> pendingGameThreadActions = new();

        private readonly PeerManager peerManager;
        private readonly SocketManager p2pListener;
        private readonly GameMessagesCodec codec;
        private readonly Dictionary<NetIdentity, (Connection connection, Framed<GameMessagesCodec> messages)> playerConnections = new();
        private readonly Dictionary<NetIdentity, Steamworks.ConnectionManager> connectionManagers = new();
        private readonly Dictionary<NetIdentity, AuthTicket> authTickets = new();
        private readonly FixedQueue<(GameMessage, Framed<GameMessagesCodec>)> pendingMessages = new(maxCount: 300);
        private readonly CancellationTokenSource processIncomingMessagesCts = new();
        private readonly object connectLock = new();
        private readonly byte[] sendBuffer = new byte[8192];

        public P2PManager()
        {
            processor = new();
            Events = new();
            peerManager = new();
            codec = new();

            peerManager.Connecting += OnPeerConnecting;
            peerManager.Connected += OnPeerConnected;
            peerManager.Disconnected += OnPeerDisconnected;
            peerManager.IncomingMessage += OnPeerMessage;
            p2pListener = SteamNetworkingSockets.CreateRelaySocket(VirtualPort, peerManager, symmetricConnect: true);

            Instance = this;
            Task.Run(ProcessIncomingMessages);
        }

        private void OnPeerConnecting(Connection connection, ConnectionInfo info)
        {
            Logger.Debug("Incoming connection from {identity}", info.Identity);
            connection.Accept();
        }

        private void OnPeerConnected(Connection connection, ConnectionInfo info)
        {
            // OnPeerMessageMessage is sometimes called before OnPeerConnected, so we have to check if the player is already connected
            TryConnectPeer(connection, info.Identity);
        }

        private void OnPeerDisconnected(Connection connection, ConnectionInfo info)
        {
            Logger.Debug("Client from {identity} disconnected", info.Identity);

            if (playerConnections.TryGetValue(info.Identity, out var tuple))
            {
                tuple.messages.Dispose();
                playerConnections.Remove(info.Identity);
            }

            if (ConnectedPlayers.TryGetValue(info.Identity, out var player))
            {
                player.Destroy();
                ConnectedPlayers.Remove(info.Identity);
            }

            if (authTickets.TryGetValue(info.Identity, out var authTicket))
            {
                authTicket.Dispose();
                authTickets.Remove(info.Identity);
            }

            connectionManagers.Remove(info.Identity);
        }

        private void OnPeerMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            // Message is sometimes received before OnPeerConnected, so we have to check if the player is connected
            TryConnectPeer(connection, identity);
        }

        private bool TryConnectPeer(Connection connection, NetIdentity identity)
        {
            lock (connectLock)
            {
                if (!playerConnections.ContainsKey(identity))
                {
                    var messages = new Framed<GameMessagesCodec>(new GameMessagesCodec(), connection, identity, peerManager, this);
                    playerConnections.Add(identity, (connection, messages));

                    Logger.Debug("Client from {identity} connecting...", identity);

                    Task.Run(async () =>
                    {
                        try
                        {
                            AuthTicket? authTicket = await SteamUser.GetAuthSessionTicketAsync();

                            if (authTicket == null)
                            {
                                await ExecuteOnGameThread(() =>
                                {
                                    Logger.Error("Failed to get auth ticket, aborting connection to {identity}", identity);
                                    Disconnect(identity);

                                    if (ConnectedPlayers.TryGetValue(identity, out var player))
                                    {
                                        player.Destroy();
                                        ConnectedPlayers.Remove(identity);
                                    }
                                });

                                return;
                            }

                            await ExecuteOnGameThread(() =>
                            {
                                authTickets.Add(identity, authTicket);

                                messages.Send(new HandshakeRequest
                                {
                                    AuthSessionTicket = authTicket.Data
                                });
                                
                                Logger.Debug("Client from {identity} connected", identity);
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "An unhandled exception was raised when connecting to {identity}", identity);
                            throw;
                        }
                    });

                    return true;
                }

                return false;
            }
        }

        private async Task ProcessIncomingMessages()
        {
            while (true)
            {
                if (processIncomingMessagesCts.IsCancellationRequested)
                    return;

                Context? context = null;

                while (pendingMessages.Count > 0)
                {
                    var (message, messages) = pendingMessages.Dequeue();
                    ConnectedPlayers.TryGetValue(messages.Identity, out RemotePlayer? player);
                    context ??= new(messages, this, player);

                    try
                    {
                        await processor.HandleMessage(message, context);
                        Pool.Release(message);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "An unhadled exception was raised when handling message {message}", message);
                    }
                }

                await Task.Delay(1).ConfigureAwait(false);
            }
        }

        internal void AddPendingMessage(NetIdentity sender, GameMessage message)
        {
            pendingMessages.Enqueue((message, playerConnections[sender].messages));
        }
        
        public void Dispose()
        {
            processIncomingMessagesCts.Cancel();

            while (pendingGameThreadActions.Count > 0)
            {
                var (_, tcs) = pendingGameThreadActions.Dequeue();
                tcs.TrySetResult(false);
            }
            
            p2pListener.Close();

            foreach (var manager in playerConnections)
            {
                manager.Value.connection.Close();
                manager.Value.messages.Dispose();
            }
            
            playerConnections.Clear();
            
            foreach (var player in ConnectedPlayers)
            {
                player.Value.Destroy();
            }
            
            ConnectedPlayers.Clear();

            Instance = null;
        }

        public void ConnectTo(IEnumerable<NetIdentity> identities)
        {
            foreach (var identity in identities)
            {
                ConnectTo(identity);
            }
        }

        public void ConnectTo(NetIdentity identity)
        {
            if (!identity.IsSteamId)
                throw new NotSupportedException("Only steamid identities are supported right now");

            if (playerConnections.ContainsKey(identity))
                return;

            Logger.Verbose("Connecting to {identity}...", identity);

            ConnectionManager connectionManager;
            
            try
            {
                connectionManager = SteamNetworkingSockets.ConnectRelay<ConnectionManager>(identity, VirtualPort, symmetricConnect: true);
                connectionManager.SetOwner(peerManager);
            }
            catch (ArgumentException) // Thrown when connection was already established (or in the process of being established) from an incoming connection
            {
                return;
            }

            connectionManagers.Add(identity, connectionManager);
        }
        
        public void Disconnect(RemotePlayer player) => Disconnect(player.SteamId);
        public void Disconnect(NetIdentity identity)
        {
            if (!playerConnections.TryGetValue(identity, out var connectionManager))
                return;

            connectionManager.connection.Flush();
            connectionManager.connection.Close();
        }

        public void DisconnectAll()
        {
            foreach (var identity in playerConnections.Keys)
            {
                Disconnect(identity);
            }
        }

        /// <summary>
        /// Executes the action on the game thread on the next update and waits for it to be executed.
        /// </summary>
        public Task<bool> ExecuteOnGameThread(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var tcs = new TaskCompletionSource<bool>();
            pendingGameThreadActions.Enqueue((action, tcs));
            return tcs.Task;
        }

        public void Update(float delta)
        {
            p2pListener.Receive();

            foreach (var manager in connectionManagers.Values)
            {
                manager.Receive();
            }
            
            while (pendingGameThreadActions.Count > 0)
            {
                (Action action, TaskCompletionSource<bool> tcs) = pendingGameThreadActions.Dequeue();
                action();
                tcs.TrySetResult(true);
            }
        }

        internal void Broadcast(GameMessage message, SendType sendType = SendType.Reliable)
        {
            if (playerConnections.Count == 0)
                return;
            
            var memStream = new MemoryStream(sendBuffer, 0, sendBuffer.Length, true, true);
            using var writer = new BinaryWriter(memStream);
            codec.Encode(message, writer);
            var bytes = memStream.GetReadOnlySpan();

            foreach (var (_, messages) in playerConnections.Values)
            {
                messages.Send(bytes, sendType);
            }
        }
    }
}