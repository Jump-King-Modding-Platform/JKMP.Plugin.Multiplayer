using System;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public partial class P2PManager
    {
        private class PeerManager : ISocketManager
        {
            #region Events
            
            public delegate void ConnectingHandler(Connection connection, ConnectionInfo info);
            public delegate void ConnectedHandler(Connection connection, ConnectionInfo info);
            public delegate void DisconnectedHandler(Connection connection, ConnectionInfo info);
            public delegate void IncomingMessageHandler(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel);

            public event ConnectingHandler? Connecting;
            public event ConnectedHandler? Connected;
            public event DisconnectedHandler? Disconnected;
            public event IncomingMessageHandler? IncomingMessage;

            internal void OnConnecting(Connection connection, ConnectionInfo info) => Connecting?.Invoke(connection, info);
            internal void OnConnected(Connection connection, ConnectionInfo info) => Connected?.Invoke(connection, info);
            internal void OnDisconnected(Connection connection, ConnectionInfo info) => Disconnected?.Invoke(connection, info);
            internal void OnIncomingMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel) =>
                IncomingMessage?.Invoke(connection, identity, data, size, messageNum, recvTime, channel);
            
            #endregion
            
            void ISocketManager.OnConnecting(Connection connection, ConnectionInfo info)
            {
                if (!info.Identity.IsSteamId)
                {
                    Logger.Warning("Ignoring incoming connection from non-steam peer: {identity}", info.Identity);
                    connection.Close();
                    return;
                }
                
                OnConnecting(connection, info);
            }

            void ISocketManager.OnConnected(Connection connection, ConnectionInfo info)
            {
                OnConnected(connection, info);
            }

            void ISocketManager.OnDisconnected(Connection connection, ConnectionInfo info)
            {
                OnDisconnected(connection, info);
            }

            void ISocketManager.OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
            {
                OnIncomingMessage(connection, identity, data, size, messageNum, recvTime, channel);
            }
        }
    }
}