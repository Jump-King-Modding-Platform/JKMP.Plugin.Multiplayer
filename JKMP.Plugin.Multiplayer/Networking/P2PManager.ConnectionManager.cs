using System;
using JKMP.Core.Logging;
using Serilog;
using Steamworks;
using Steamworks.Data;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public partial class P2PManager
    {
        private class ConnectionManager : IConnectionManager
        {
            public Connection? Connection { get; internal set; }
            public NetIdentity Identity { get; }

            private readonly PeerManager owner;

            public ConnectionManager(PeerManager owner, NetIdentity identity)
            {
                this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
                Identity = identity;
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            private static readonly ILogger Logger = LogManager.CreateLogger<ConnectionManager>();

            void IConnectionManager.OnConnecting(ConnectionInfo info)
            {
                if (Connection == null)
                    throw new InvalidOperationException("Connection has not been assigned yet.");

                owner.OnConnecting(Connection!.Value, info);
            }

            void IConnectionManager.OnConnected(ConnectionInfo info)
            {
                owner.OnConnected(Connection!.Value, info);
            }

            void IConnectionManager.OnDisconnected(ConnectionInfo info)
            {
                owner.OnDisconnected(Connection!.Value, info);
            }

            void IConnectionManager.OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
            {
                owner.OnIncomingMessage(Connection!.Value, Identity, data, size, messageNum, recvTime, channel);
            }
        }
    }
}