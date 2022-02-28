using System;
using Steamworks;
using Steamworks.Data;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public partial class P2PManager
    {
        private class P2PConnection : IDisposable
        {
            public readonly NetIdentity Identity;
            public Framed<GameMessagesCodec>? Messages;
            public Connection? Connection;
            public ConnectionManager? ConnectionManager;
            public AuthTicket? AuthTicket;
            public RemotePlayer? Player;
            
            public P2PConnection(NetIdentity identity)
            {
                Identity = identity;
            }

            public void Dispose()
            {
                Player?.Destroy();
                Messages?.Dispose();
                AuthTicket?.Dispose();

                Connection?.Flush();
                Connection?.Close();
            }
        }
    }
}