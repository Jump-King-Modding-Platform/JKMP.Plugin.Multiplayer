using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using JKMP.Core.Logging;
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

        public ConcurrentDictionary<NetIdentity, RemotePlayer> ConnectedPlayers { get; } = new();

        public P2PEvents Events { get; private set; }
        
        private readonly GameMessageProcessor processor;

        private static readonly ILogger Logger = LogManager.CreateLogger<P2PManager>();

        private readonly Queue<(Action action, TaskCompletionSource<bool> tcs)> pendingGameThreadActions = new();

        private readonly PeerManager peerManager = new();
        private readonly SocketManager p2pListener;
        private readonly ConcurrentDictionary<NetIdentity, Connection> playerConnections = new();
        private readonly ConcurrentDictionary<NetIdentity, Steamworks.ConnectionManager> connectionManagers = new();

        public P2PManager()
        {
            processor = new();
            Events = new();

            peerManager.Connecting += OnPeerConnecting;
            peerManager.Connected += OnPeerConnected;
            peerManager.Disconnected += OnPeerDisconnected;
            peerManager.IncomingMessage += OnPeerMessage;
            p2pListener = SteamNetworkingSockets.CreateRelaySocket(VirtualPort, peerManager, symmetricConnect: true);

            Instance = this;
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

            playerConnections.TryRemove(info.Identity, out _);
            connectionManagers.TryRemove(info.Identity, out _);
        }

        private void OnPeerMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            // Message is sometimes received before OnPeerConnected, so we have to check if the player is already connected
            TryConnectPeer(connection, identity);
            
            Logger.Debug("Incoming message from {identity} of {numBytes} bytes", identity, size);
        }

        private bool TryConnectPeer(Connection connection, NetIdentity identity)
        {
            if (playerConnections.TryAdd(identity, connection))
            {
                Logger.Debug("Client from {identity} connected", identity);
                playerConnections.TryAdd(identity, connection);

                connection.SendMessage("Hello gamer");
                
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            p2pListener.Close();

            foreach (var manager in playerConnections)
            {
                manager.Value.Close();
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

            connectionManagers.TryAdd(identity, connectionManager);
        }
        
        public void Disconnect(RemotePlayer player) => Disconnect(player.SteamId);
        public void Disconnect(NetIdentity identity)
        {
            playerConnections[identity].Flush();
            playerConnections[identity].Close();
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
        public Task ExecuteOnGameThread(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var tcs = new TaskCompletionSource<bool>(false);
            pendingGameThreadActions.Enqueue((action, tcs));
            return tcs.Task;
        }

        public void Update(float delta)
        {
            while (pendingGameThreadActions.Count > 0)
            {
                (Action action, TaskCompletionSource<bool> tcs) = pendingGameThreadActions.Dequeue();
                action();
                tcs.SetResult(true);
            }

            p2pListener.Receive();

            foreach (var manager in connectionManagers.Values)
            {
                manager.Receive();
            }
        }

        internal void Broadcast(GameMessage message, P2PSend sendType = P2PSend.Reliable)
        {
            
        }
    }
}