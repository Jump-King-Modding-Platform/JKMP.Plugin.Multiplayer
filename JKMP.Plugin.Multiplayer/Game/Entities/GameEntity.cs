using System;
using System.Collections.Generic;
using System.Linq;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Components;
using JKMP.Plugin.Multiplayer.Game.Input;
using JKMP.Plugin.Multiplayer.Game.Player;
using JKMP.Plugin.Multiplayer.Game.Sound;
using JKMP.Plugin.Multiplayer.Game.UI;
using JKMP.Plugin.Multiplayer.Game.UI.Widgets;
using JKMP.Plugin.Multiplayer.Matchmaking;
using JKMP.Plugin.Multiplayer.Networking;
using JumpKing;
using JumpKing.Player;
using Microsoft.Xna.Framework;
using Myra;
using Serilog;
using Steamworks;
using Steamworks.Data;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class GameEntity : BaseManagerEntity
    {
        public P2PManager P2P { get; private set; } = null!;
        public LocalPlayerListener PlayerListener => plrListener;
        internal SoundManager Sound { get; private set; } = null!;
        
        private LocalPlayerListener plrListener = null!;
        private Chat chatWidget = null!;
        private StatusPanel statusPanel = null!;

        private float timeSincePositionUpdate;
        private const float PositionUpdateInterval = 30; // Send a position update every 30 seconds

        private static readonly ILogger Logger = LogManager.CreateLogger<GameEntity>();
        private PlayerEntity localPlayer = null!;

        protected override void OnFirstUpdate()
        {
            plrListener = new LocalPlayerListener();
            P2P = new();
            Sound = new();
            chatWidget = UIManager.AddWidget(new Chat());
            localPlayer = EntityManager.instance.Find<PlayerEntity>();
            localPlayer.AddComponents(new PlayerStateTransmitter(P2P), new AudioListenerComponent(), new PlayerPauseManager());
            statusPanel = UIManager.AddWidget(new StatusPanel());
            
            MatchmakingManager.Instance.Events.NearbyClientsReceived += OnNearbyClientsReceived;
        }

        protected override void OnDestroy()
        {
            MatchmakingManager.Instance.Events.NearbyClientsReceived -= OnNearbyClientsReceived;

            base.OnDestroy();
            plrListener.Dispose();
            P2P.Dispose();
            UIManager.RemoveWidget(chatWidget);
            UIManager.RemoveWidget(statusPanel);
        }

        private void OnNearbyClientsReceived(ICollection<ulong> steamIds)
        {
            P2P.ConnectTo(steamIds.Select(id => (NetIdentity)(SteamId)id));
        }

        protected override void Update(float delta)
        {
            base.Update(delta);

            plrListener.Update(delta);

            timeSincePositionUpdate += delta;
            if (timeSincePositionUpdate >= PositionUpdateInterval)
            {
                timeSincePositionUpdate = 0;
                MatchmakingManager.Instance.SendPosition(plrListener.Position);
            }

            P2P.Update(delta);
            MatchmakingManager.Update(delta);
            Sound.Update(delta);
            chatWidget.Update(delta);
            statusPanel.Update(delta);
        }
    }
}