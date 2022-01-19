using System;
using JKMP.Core.Logging;
using Serilog;
using Steamworks;
using Steamworks.Data;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public partial class P2PManager
    {
        private class ConnectionManager : Steamworks.ConnectionManager
        {
            private PeerManager owner;

            public void SetOwner(PeerManager owner)
            {
                this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            private static readonly ILogger Logger = LogManager.CreateLogger<ConnectionManager>();

            public override void OnConnecting(ConnectionInfo info)
            {
                base.OnConnecting(info);
                owner.OnConnecting(Connection, info);
            }

            public override void OnConnected(ConnectionInfo info)
            {
                base.OnConnected(info);
                owner.OnConnected(Connection, info);
            }

            public override void OnDisconnected(ConnectionInfo info)
            {
                base.OnDisconnected(info);
                owner.OnDisconnected(Connection, info);
            }

            public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
            {
                base.OnMessage(data, size, messageNum, recvTime, channel);
                owner.OnIncomingMessage(Connection, ConnectionInfo.Identity, data, size, messageNum, recvTime, channel);
            }
        }
    }
}