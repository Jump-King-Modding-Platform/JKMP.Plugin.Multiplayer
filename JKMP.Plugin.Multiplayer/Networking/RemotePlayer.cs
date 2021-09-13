using System;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Components;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public class RemotePlayer
    {
        public readonly SteamId SteamId;
        public PlayerNetworkState State { get; private set; } = PlayerNetworkState.Handshaking;

        internal AuthTicket? AuthTicket;

        private FakePlayer? fakePlayer;
        private RemotePlayerInterpolator? interpolator;

        public RemotePlayer(SteamId steamId)
        {
            SteamId = steamId;
        }
        
        public void Destroy()
        {
            
        }

        internal void InitializeFromHandshakeResponse(HandshakeResponse response, Friend userInfo)
        {
            State = PlayerNetworkState.Connected;

            interpolator = new();
            fakePlayer = new();
            fakePlayer.AddComponents(interpolator);
            UpdateFromState(response.PlayerState!);

            LogManager.TempLogger.Verbose("Initialized from handshake");
        }

        internal void UpdateFromState(PlayerStateChanged message)
        {
            if (interpolator == null || State != PlayerNetworkState.Connected)
                throw new InvalidOperationException("Tried to update state before receiving handshake response");

            interpolator?.UpdateState(message);
        }
    }

    public enum PlayerNetworkState
    {
        Handshaking,
        Connected
    }
}