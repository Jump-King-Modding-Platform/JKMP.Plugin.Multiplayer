using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using JumpKing;
using Newtonsoft.Json;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public class RemotePlayer
    {
        public readonly SteamId SteamId;
        public PlayerNetworkState State { get; private set; } = PlayerNetworkState.Handshaking;

        internal AuthTicket? AuthTicket;

        private FakePlayer? fakePlayer;

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
            fakePlayer?.Destroy();
            fakePlayer = null;
        }

        internal void InitializeFromHandshakeResponse(HandshakeResponse response, Friend userInfo)
        {
            State = PlayerNetworkState.Connected;
            
            fakePlayer = new();
            fakePlayer.SetSprite(JKContentManager.PlayerSprites.idle); // temporary
            fakePlayer.SetDirection(response.PlayerState!.WalkDirection);
            fakePlayer.SetPositionAndVelocity(response.PlayerState.Position, response.PlayerState.Velocity);

            LogManager.TempLogger.Verbose("Initialized from handshake");
        }
    }

    public enum PlayerNetworkState
    {
        Handshaking,
        Connected
    }
}