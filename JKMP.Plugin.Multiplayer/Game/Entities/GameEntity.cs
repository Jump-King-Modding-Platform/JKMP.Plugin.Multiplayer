using System;
using System.Collections.Generic;
using System.Linq;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Player;
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
        private FakePlayer fakePlayer = null!;
        private LocalPlayerListener plrListener = null!;
        // ReSharper disable once InconsistentNaming
        private P2PManager p2p = null!;

        private float timeSincePositionUpdate;
        private const float PositionUpdateInterval = 30; // Send a position update every 30 seconds

        private static readonly ILogger Logger = LogManager.CreateLogger<GameEntity>();

        protected override void OnFirstUpdate()
        {
            fakePlayer = new FakePlayer();
            plrListener = new LocalPlayerListener();

            plrListener.Jump += OnJump;
            plrListener.StartJump += OnStartJump;
            plrListener.StartedFalling += OnStartedFalling;
            plrListener.Knocked += OnKnocked;
            plrListener.Land += OnLand;
            plrListener.Walk += OnWalk;

            MatchmakingManager.Instance.Events.NearbyClientsReceived += OnNearbyClientsReceived;
            p2p = new();
        }

        protected override void OnDestroy()
        {
            MatchmakingManager.Instance.Events.NearbyClientsReceived -= OnNearbyClientsReceived;

            base.OnDestroy();
            plrListener.Dispose();
            p2p.Dispose();
        }

        private void OnJump()
        {
            fakePlayer.SetSprite(JKContentManager.PlayerSprites.jump_up);
        }

        private void OnStartJump()
        {
            fakePlayer.SetSprite(JKContentManager.PlayerSprites.jump_charge);
        }

        private void OnStartedFalling(bool knocked)
        {
            if (!knocked)
                fakePlayer.SetSprite(JKContentManager.PlayerSprites.jump_fall);
        }

        private void OnKnocked()
        {
            fakePlayer.SetSprite(JKContentManager.PlayerSprites.jump_bounce);
        }

        private void OnLand(bool splat)
        {
            fakePlayer.SetSprite(splat ? JKContentManager.PlayerSprites.splat : JKContentManager.PlayerSprites.idle);
        }

        private void OnWalk(int direction)
        {
            if (direction != 0)
            {
                fakePlayer.SetSprite(JKContentManager.PlayerSprites.walk_one);
                fakePlayer.SetDirection(direction);
            }
            else
            {
                fakePlayer.SetSprite(JKContentManager.PlayerSprites.idle);
            }
        }

        private void OnNearbyClientsReceived(ICollection<ulong> steamIds)
        {
            p2p.ConnectTo(steamIds.Select(id => new SteamId { Value = id }).ToArray());
        }

        protected override void Update(float delta)
        {
            base.Update(delta);

            plrListener.Update(delta);
            fakePlayer.SetPositionAndVelocity(plrListener.Position + new Vector2(0, -50), plrListener.Velocity);

            timeSincePositionUpdate += delta;
            while (timeSincePositionUpdate >= PositionUpdateInterval)
            {
                timeSincePositionUpdate = 0;
                MatchmakingManager.Instance.SendPosition(plrListener.Position);
            }
        }
    }
}