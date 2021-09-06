using JKMP.Plugin.Multiplayer.Networking.Messages;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public class RemotePlayer
    {
        public readonly SteamId SteamId;
        public PlayerNetworkState State { get; private set; } = PlayerNetworkState.Handshaking;

        public RemotePlayer(SteamId steamId)
        {
            SteamId = steamId;
        }

        public void Update(float delta)
        {
            
        }
        
        public void Destroy()
        {
            
        }

        internal void InitializeFromHandshakeResponse(HandshakeResponse response)
        {
            
        }

        internal void HandleMessage(GameMessage message)
        {
            
        }
    }

    public enum PlayerNetworkState
    {
        Handshaking,
        Connected
    }
}