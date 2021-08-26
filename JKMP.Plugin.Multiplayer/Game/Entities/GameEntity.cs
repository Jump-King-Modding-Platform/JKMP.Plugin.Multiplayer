using System;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Player;
using JumpKing;
using JumpKing.Player;
using Microsoft.Xna.Framework;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class GameEntity : BaseManagerEntity
    {
        private FakePlayer fakePlayer = null!;
        private LocalPlayerListener plrListener = null!;

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
            LogManager.TempLogger.Information("Jump");
            fakePlayer.SetSprite(JKContentManager.PlayerSprites.jump_up);
        }

        private void OnStartJump()
        {
            LogManager.TempLogger.Information("StartJump");
            fakePlayer.SetSprite(JKContentManager.PlayerSprites.jump_charge);
        }

        private void OnStartedFalling(bool knocked)
        {
            LogManager.TempLogger.Information("StartedFalling: {knocked}", knocked);
            if (!knocked)
                fakePlayer.SetSprite(JKContentManager.PlayerSprites.jump_fall);
        }

        private void OnKnocked()
        {
            LogManager.TempLogger.Information("Knocked");
            fakePlayer.SetSprite(JKContentManager.PlayerSprites.jump_bounce);
        }

        private void OnLand(bool splat)
        {
            LogManager.TempLogger.Information("Land: {splat}", splat);
            fakePlayer.SetSprite(splat ? JKContentManager.PlayerSprites.splat : JKContentManager.PlayerSprites.idle);
        }

        private void OnWalk(int direction)
        {
            LogManager.TempLogger.Information("Walk: {direction}", direction);
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
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            plrListener.Dispose();
        }
    }
}