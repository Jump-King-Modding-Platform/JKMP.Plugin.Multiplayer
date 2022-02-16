using System;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Components;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using Matchmaking.Client.Chat;
using Microsoft.Xna.Framework;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public class RemotePlayer
    {
        public readonly SteamId SteamId;
        public PlayerNetworkState State { get; private set; } = PlayerNetworkState.Handshaking;

        public VoiceManager? VoiceManager { get; private set; }
        
        public bool EntityIsAlive => fakePlayer?.IsAlive ?? false;

        private FakePlayer? fakePlayer;
        private RemotePlayerInterpolator? interpolator;

        private static readonly ILogger Logger = LogManager.CreateLogger<RemotePlayer>();

        public RemotePlayer(SteamId steamId)
        {
            SteamId = steamId;
            P2PManager.Instance!.Events.IncomingChatMessage += OnIncomingChatMessage;
        }

        public void Destroy()
        {
            fakePlayer?.Destroy();
            fakePlayer = null;
            P2PManager.Instance!.Events.IncomingChatMessage -= OnIncomingChatMessage;
        }

        /// <summary>
        /// Shows the given message above the player's head.
        /// </summary>
        public void Say(string message)
        {
            fakePlayer?.Say(message);
        }

        internal void InitializeFromHandshakeResponse(HandshakeResponse response, Friend userInfo)
        {
            State = PlayerNetworkState.Connected;
            
            fakePlayer = new();
            fakePlayer.SetName(userInfo.Name);
            VoiceManager = fakePlayer.GetComponent<VoiceManager>();
            interpolator = fakePlayer.GetComponent<RemotePlayerInterpolator>();

            if (userInfo.IsFriend)
            {
                fakePlayer.SetNameColor(Color.LightGreen);
            }
            
            UpdateFromState(response.PlayerState!);
            fakePlayer.SetPosition(response.PlayerState!.Position); // Prevent lerping from (0,0) to the player's position when the player is spawned.

            Logger.Verbose("Initialized from handshake");
        }

        internal void UpdateFromState(PlayerStateChanged message)
        {
            if (interpolator == null || State != PlayerNetworkState.Connected)
                throw new InvalidOperationException("Tried to update state before receiving handshake response");

            interpolator?.UpdateState(message);
        }

        private void OnIncomingChatMessage(ChatMessage message)
        {
            if (message.SenderId != SteamId)
                return;

            if (message.Channel != ChatChannel.Local)
                return;

            Say(message.Message);
        }
    }

    public enum PlayerNetworkState
    {
        Handshaking,
        Connected
    }
}