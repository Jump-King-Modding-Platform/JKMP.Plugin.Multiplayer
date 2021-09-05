using System;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Player;
using JKMP.Plugin.Multiplayer.Matchmaking;
using JumpKing;
using JumpKing.Player;
using Microsoft.Xna.Framework;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class GameEntity : BaseManagerEntity
    {
        private FakePlayer fakePlayer = null!;
        private LocalPlayerListener plrListener = null!;

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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            plrListener.Dispose();
        }
    }
}