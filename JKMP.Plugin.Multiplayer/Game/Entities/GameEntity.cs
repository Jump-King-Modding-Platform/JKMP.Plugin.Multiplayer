using System;
using System.Collections.Generic;
using System.Linq;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Components;
using JKMP.Plugin.Multiplayer.Game.Player;
using JKMP.Plugin.Multiplayer.Game.Sound;
using JKMP.Plugin.Multiplayer.Matchmaking;
using JKMP.Plugin.Multiplayer.Networking;
using JumpKing;
using JumpKing.Player;
using Microsoft.Xna.Framework;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class GameEntity : BaseManagerEntity
    {
        public LocalPlayerListener PlayerListener => plrListener;
        internal SoundManager Sound { get; private set; } = null!;
        
        private LocalPlayerListener plrListener = null!;
        // ReSharper disable once InconsistentNaming
        private P2PManager p2p = null!;

        private float timeSincePositionUpdate;
        private const float PositionUpdateInterval = 30; // Send a position update every 30 seconds

        private static readonly ILogger Logger = LogManager.CreateLogger<GameEntity>();

        protected override void OnFirstUpdate()
        {
            plrListener = new LocalPlayerListener();
            MatchmakingManager.Instance.Events.NearbyClientsReceived += OnNearbyClientsReceived;
            p2p = new();
            Sound = new();

            var localPlayer = EntityManager.instance.Find<PlayerEntity>();
            localPlayer.AddComponents(new PlayerStateTransmitter(p2p), new AudioListenerComponent());
        }

        protected override void OnDestroy()
        {
            MatchmakingManager.Instance.Events.NearbyClientsReceived -= OnNearbyClientsReceived;

            base.OnDestroy();
            plrListener.Dispose();
            p2p.Dispose();
        }

        private void OnNearbyClientsReceived(ICollection<ulong> steamIds)
        {
            p2p.ConnectTo(steamIds.Select(id => new SteamId { Value = id }).ToArray());
        }

        protected override void Update(float delta)
        {
            base.Update(delta);

            plrListener.Update(delta);

            timeSincePositionUpdate += delta;
            while (timeSincePositionUpdate >= PositionUpdateInterval)
            {
                timeSincePositionUpdate = 0;
                MatchmakingManager.Instance.SendPosition(plrListener.Position);
            }

            p2p.Update(delta);
            Sound.Update(delta);
        }
    }
}