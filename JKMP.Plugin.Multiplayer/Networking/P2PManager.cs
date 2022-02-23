using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public P2PEvents Events { get; private set; }
        
        private readonly GameMessageProcessor processor;

        private static readonly ILogger Logger = LogManager.CreateLogger<P2PManager>();

        private readonly Queue<(Action action, TaskCompletionSource<bool> tcs)> pendingGameThreadActions = new();

        private readonly PeerManager peerManager;
        private readonly SocketManager p2pListener;
        private readonly GameMessagesCodec codec;
        private readonly Dictionary<NetIdentity, P2PConnection> connections = new();
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
            
            if (connections.TryGetValue(info.Identity, out var connectionManager))
            {
                connectionManager.Dispose();
                connections.Remove(info.Identity);
            }
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
                if (connections.TryGetValue(identity, out var p2pConnection))
                    return false;
                
                Logger.Debug("Client from {identity} connecting...", identity);
                var messages = new Framed<GameMessagesCodec>(new GameMessagesCodec(), connection, identity, peerManager, this);
                p2pConnection = new(identity)
                {
                    Messages = messages,
                    Connection = connection
                };

                connections.Add(identity, p2pConnection);
                SendHandshakeRequest(p2pConnection);

                return true;
            }
        }

        private void SendHandshakeRequest(P2PConnection connection)
        {
            NetIdentity identity = connection.Identity;
            Framed<GameMessagesCodec> messages = connection.Messages ?? throw new InvalidOperationException("P2P connection not properly initialized");
            
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

                            connection.Dispose();
                            connections.Remove(identity);
                        });

                        return;
                    }

                    await ExecuteOnGameThread(() =>
                    {
                        connection.AuthTicket = authTicket;

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
                    RemotePlayer? player = GetPlayer(messages.Identity);
                    
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

        public RemotePlayer? GetPlayer(NetIdentity identity)
        {
            return connections.TryGetValue(identity, out var connection) ? connection.Player : null;
        }

        public bool TryGetPlayer(NetIdentity identity, out RemotePlayer? player)
        {
            if (connections.TryGetValue(identity, out var connection) && connection.Player != null)
            {
                player = connection.Player;
                return true;
            }

            player = null;
            return false;
        }

        internal void AddPendingMessage(NetIdentity sender, GameMessage message)
        {
            if (connections.TryGetValue(sender, out var conn))
                pendingMessages.Enqueue((message, conn.Messages!));
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

            foreach (var conn in connections)
            {
                conn.Value.Dispose();
            }
            
            connections.Clear();

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

            var conn = new P2PConnection(identity)
            {
                ConnectionManager = connectionManager,
                Connection = connectionManager.Connection,
                Messages = new Framed<GameMessagesCodec>(new GameMessagesCodec(), connectionManager.Connection, identity, peerManager, this)
            };
            
            connections.Add(identity, conn);
            SendHandshakeRequest(conn);
        }
        
        public void Disconnect(RemotePlayer player) => Disconnect(player.SteamId);
        public void Disconnect(NetIdentity identity)
        {
            if (!connections.TryGetValue(identity, out var conn))
                return;

            conn.Connection?.Flush();
            conn.Connection?.Close();
        }

        public void DisconnectAll()
        {
            foreach (var identity in connections.Keys.ToList())
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

            foreach (var conn in connections.Values)
            {
                conn.ConnectionManager?.Receive();
            }
            
            while (pendingGameThreadActions.Count > 0)
            {
                (Action action, TaskCompletionSource<bool> tcs) = pendingGameThreadActions.Dequeue();
                action();
                tcs.TrySetResult(true);
            }
        }

        internal void Broadcast(GameMessage message, SendType sendType = SendType.Reliable, ushort lane = 2)
        {
            if (connections.Count == 0)
                return;
            
            var memStream = new MemoryStream(sendBuffer, 0, sendBuffer.Length, true, true);
            using var writer = new BinaryWriter(memStream);
            codec.Encode(message, writer);
            var bytes = memStream.GetReadOnlySpan();

            foreach (var conn in connections.Values)
            {
                conn.Messages?.Send(bytes, sendType, lane);
            }
        }

        public void AddPlayer(NetIdentity identity, RemotePlayer player)
        {
            if (connections.TryGetValue(identity, out var connection))
            {
                if (connection.Player != null)
                    throw new InvalidOperationException("Player already created for this identity: " + identity);

                connection.Player = player;
            }
        }
    }
}