using JKMP.Plugin.Multiplayer.Networking.Messages;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public class RemotePlayer
    {
        public readonly SteamId SteamId;
        public PlayerNetworkState State { get; private set; } = PlayerNetworkState.Handshaking;

        internal AuthTicket? AuthTicket;

        public RemotePlayer(SteamId steamId)
        {
            SteamId = steamId;
        }

        public void Update(float delta)
        {
            if (State != PlayerNetworkState.Connected)
                return;
        }

        public void Draw()
        {
            if (State != PlayerNetworkState.Connected)
                return;
        }
        
        public void Destroy()
        {
        }

        internal void InitializeFromHandshakeResponse(HandshakeResponse response, Friend userInfo)
        {
            State = PlayerNetworkState.Connected;
        }
    }

    public enum PlayerNetworkState
    {
        Handshaking,
        Connected
    }
}