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
        private readonly Steamworks.SocketManager p2pListener;
        private readonly ConcurrentDictionary<NetIdentity, Connection> playerConnections = new();

        public P2PManager()
        {
            processor = new();
            Events = new();

            peerManager.Connecting += OnPeerConnecting;
            peerManager.Connected += OnPeerConnected;
            peerManager.Disconnected += OnPeerDisconnected;
            peerManager.IncomingMessage += OnPeerMessage;
            p2pListener = SteamNetworkingSockets.CreateRelaySocket(VirtualPort, peerManager, symmetricConnect: true);
            peerManager.Listener = p2pListener;

            Instance = this;
        }

        private void OnPeerConnecting(Connection connection, ConnectionInfo info)
        {
            Logger.Debug("Incoming connection from {identity}", info.Identity);
            connection.Accept();
        }

        private void OnPeerConnected(Connection connection, ConnectionInfo info)
        {
            Logger.Debug("Client from {identity} connected", info.Identity);
            playerConnections.TryAdd(info.Identity, connection);

            connection.SendMessage("Hello gamer");
        }

        private void OnPeerDisconnected(Connection connection, ConnectionInfo info)
        {
            Logger.Debug("Client from {identity} disconnected", info.Identity);
            Disconnect(info.Identity);
        }

        private void OnPeerMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            Logger.Debug("Incoming message from {identity} of {numBytes} bytes", identity, size);
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

            Steamworks.ConnectionManager connectionManager;
            
            try
            {
                var manager = peerManager.CreateClientConnectionManager(identity);
                connectionManager = SteamNetworkingSockets.ConnectRelay(identity, VirtualPort, manager, symmetricConnect: true);
                manager.Connection = connectionManager.Connection;
            }
            catch (ArgumentException) // Thrown when connection was already established from an incoming connection
            {
                return;
            }
            
            playerConnections.TryAdd(identity, connectionManager.Connection);
            peerManager.AddClientConnection(identity, connectionManager.Connection);
        }
        
        public void Disconnect(RemotePlayer player) => Disconnect(player.SteamId);
        public void Disconnect(NetIdentity identity)
        {
            
        }

        public void DisconnectAll()
        {
            
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
        }

        internal void Broadcast(GameMessage message, P2PSend sendType = P2PSend.Reliable)
        {
            
        }
    }
}