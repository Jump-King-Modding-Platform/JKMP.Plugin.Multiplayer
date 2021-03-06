using System;
using System.Collections.Generic;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Game.Player;
using JKMP.Plugin.Multiplayer.Game.Player.Animations;
using JKMP.Plugin.Multiplayer.Game.Sound;
using JKMP.Plugin.Multiplayer.Memory;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using JumpKing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace JKMP.Plugin.Multiplayer.Game.Components
{
    public class RemotePlayerInterpolator : Component
    {
        private PlayerStateChanged? lastState;
        private PlayerStateChanged? nextState;

        private AudioEmitter AudioEmitter => audioEmitterComponent.AudioEmitter;
        private AudioEmitterComponent audioEmitterComponent = null!;
        private SoundManager soundManager = null!;

        private FakePlayer FakePlayer => (FakePlayer)gameObject;

        private static readonly SpriteAnimation StretchAnimation = new(CreateStretchAnimationFrames());

        private static IEnumerable<SpriteFrame> CreateStretchAnimationFrames()
        {
            for (int i = 0; i < 7; ++i)
            {
                yield return new SpriteFrame(JKContentManager.PlayerSprites.stretch_one, 0.5f);
                yield return new SpriteFrame(JKContentManager.PlayerSprites.stretch_smear, 0.25f);
                yield return new SpriteFrame(JKContentManager.PlayerSprites.stretch_two, 0.5f);
                yield return new SpriteFrame(JKContentManager.PlayerSprites.stretch_smear, 0.25f);
            }
        }

        private static readonly SpriteAnimation IdleAnimation = new(
            new SpriteFrame(JKContentManager.PlayerSprites.idle, 7f, 15f),
            new SpriteFrame(JKContentManager.PlayerSprites.look_up, 5f, 7f),
            new SpriteFrame(JKContentManager.PlayerSprites.idle, 7f, 15f),
            new SpriteFrame(StretchAnimation, 10.5f)
        );

        private static readonly SpriteAnimation WalkAnimation = new(
            new SpriteFrame(JKContentManager.PlayerSprites.walk_one, 0.225f),
            new SpriteFrame(JKContentManager.PlayerSprites.walk_smear, 0.05f),
            new SpriteFrame(JKContentManager.PlayerSprites.walk_two, 0.225f),
            new SpriteFrame(JKContentManager.PlayerSprites.walk_smear, 0.05f)
        );

        private static readonly Dictionary<PlayerState, Sprite> StateSprites = new()
        {
            { PlayerState.Idle, IdleAnimation },
            { PlayerState.Falling, JKContentManager.PlayerSprites.jump_fall },
            { PlayerState.StartJump, JKContentManager.PlayerSprites.jump_charge },
            { PlayerState.Jump, JKContentManager.PlayerSprites.jump_up },
            { PlayerState.Knocked, JKContentManager.PlayerSprites.jump_bounce },
            { PlayerState.Land, IdleAnimation },
            { PlayerState.Walk, WalkAnimation },
            { PlayerState.Splat, JKContentManager.PlayerSprites.splat },
        };

        protected override void Init()
        {
            soundManager = EntityManager.instance.Find<GameEntity>()?.Sound ?? throw new InvalidOperationException("GameEntity or SoundManager not found");
            audioEmitterComponent = GetComponent<AudioEmitterComponent>() ?? throw new NotSupportedException("AudioEmitterComponent component not found");
        }

        internal void UpdateState(PlayerStateChanged newState)
        {
            if (lastState?.State != null && nextState?.State != null)
                PlayStateSounds(lastState, nextState);

            newState.MergeByDelta(nextState);

            if (lastState != null)
                Pool.Release(lastState);

            lastState = nextState;
            nextState = CloneState(newState);
        }

        protected override void Update(float delta)
        {
            if (lastState != null && nextState != null)
            {
                // Reset animation if we're changing states
                if (FakePlayer.Sprite is SpriteAnimation spriteAnimation && lastState.State != nextState.State)
                {
                    spriteAnimation.Reset();
                }
                
                FakePlayer.SetSprite(StateSprites[lastState.State]);
                FakePlayer.SetDirection(nextState.WalkDirection);

                Vector2 position = Vector2.Lerp(FakePlayer.Transform.Position, nextState.Position, 30f * delta);
                FakePlayer.SetPosition(position);
            }

            if (FakePlayer.Sprite is SpriteAnimation animation)
            {
                animation.Update(delta);
            }
        }

        private void PlayStateSounds(PlayerStateChanged stateA, PlayerStateChanged stateB)
        {
            if (stateA.State != stateB.State)
            {
                var surfaceType = stateB.SurfaceType;

                if (!Content.PlayerSounds.ContainsKey(surfaceType))
                    surfaceType = Content.SurfaceType.Default;

                switch (stateB.State)
                {
                    case PlayerState.Jump:
                        soundManager.PlaySound(Content.PlayerSounds[surfaceType].Jump, AudioEmitter, 0.5f);
                        break;
                    case PlayerState.Knocked:
                        soundManager.PlaySound(Content.PlayerSounds[surfaceType].Bump, AudioEmitter, 0.5f);
                        break;
                    case PlayerState.Splat:
                        soundManager.PlaySound(Content.PlayerSounds[surfaceType].Splat, AudioEmitter, 0.5f);

                        if (stateB.WearingShoes && surfaceType != Content.SurfaceType.Water)
                            soundManager.PlaySound(Content.PlayerSounds[Content.SurfaceType.Iron].Splat, AudioEmitter, 0.5f);

                        break;
                }

                if ((stateA.State == PlayerState.Falling || stateA.State == PlayerState.Knocked) &&
                    stateB.State != PlayerState.Falling &&
                    stateB.State != PlayerState.Knocked &&
                    stateB.State != PlayerState.Splat
                   )
                {
                    soundManager.PlaySound(Content.PlayerSounds[surfaceType].Land, AudioEmitter, 0.5f);

                    if (stateB.WearingShoes && surfaceType != Content.SurfaceType.Water)
                        soundManager.PlaySound(Content.PlayerSounds[Content.SurfaceType.Iron].Land, AudioEmitter, 0.5f);
                }
            }
        }

        private PlayerStateChanged CloneState(PlayerStateChanged original)
        {
            var clone = Pool.Get<PlayerStateChanged>();
            clone.Delta = original.Delta;
            clone.Position = original.Position;
            clone.State = original.State;
            clone.SurfaceType = original.SurfaceType;
            clone.WalkDirection = original.WalkDirection;

            return clone;
        }
    }
}