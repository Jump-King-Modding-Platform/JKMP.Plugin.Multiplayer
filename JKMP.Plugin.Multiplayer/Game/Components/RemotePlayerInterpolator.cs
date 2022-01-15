using System;
using System.Collections.Generic;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Game.Player;
using JKMP.Plugin.Multiplayer.Game.Player.Animations;
using JKMP.Plugin.Multiplayer.Game.Sound;
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
        private float elapsedTimeSinceLastState;

        private AudioEmitter audioEmitter = null!;
        private Transform plrTransform = null!;
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
            audioEmitter = new AudioEmitter();
            plrTransform = GetComponent<Transform>() ?? throw new NotSupportedException("Transform component not found");
            soundManager = EntityManager.instance.Find<GameEntity>()?.Sound ?? throw new InvalidOperationException("GameEntity or SoundManager not found");
        }

        internal void UpdateState(PlayerStateChanged newState)
        {
            if (lastState != null && nextState != null)
                PlayStateSounds(lastState, nextState);
            
            lastState = nextState;
            nextState = newState ?? throw new ArgumentNullException(nameof(newState));
            elapsedTimeSinceLastState = 0;
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

                float lerpDelta = elapsedTimeSinceLastState / PlayerStateTransmitter.TransmissionInterval;
                lerpDelta = MathHelper.Clamp(lerpDelta, 0, 1);
                Vector2 position = Vector2.Lerp(lastState.Position, nextState.Position, lerpDelta);
                FakePlayer.SetPosition(position);
                UpdateAudioEmitter();

                elapsedTimeSinceLastState += delta;
            }

            if (FakePlayer.Sprite is SpriteAnimation)
            {
                var spriteAnimation = (SpriteAnimation)FakePlayer.Sprite;
                spriteAnimation.Update(delta);
            }
        }

        private void UpdateAudioEmitter()
        {
            audioEmitter.Position = SoundUtil.ScalePosition(plrTransform.Position);
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
                        soundManager.PlaySound(Content.PlayerSounds[surfaceType].Jump, audioEmitter, 0.5f);
                        break;
                    case PlayerState.Land:
                        soundManager.PlaySound(Content.PlayerSounds[surfaceType].Land, audioEmitter, 0.5f);

                        if (stateB.WearingShoes && surfaceType != Content.SurfaceType.Water)
                            soundManager.PlaySound(Content.PlayerSounds[Content.SurfaceType.Iron].Land, audioEmitter, 0.5f);

                        break;
                    case PlayerState.Knocked:
                        soundManager.PlaySound(Content.PlayerSounds[surfaceType].Bump, audioEmitter, 0.5f);
                        break;
                    case PlayerState.Splat:
                        soundManager.PlaySound(Content.PlayerSounds[surfaceType].Splat, audioEmitter, 0.5f);

                        if (stateB.WearingShoes && surfaceType != Content.SurfaceType.Water)
                            soundManager.PlaySound(Content.PlayerSounds[Content.SurfaceType.Iron].Splat, audioEmitter, 0.5f);

                        break;
                }
            }
        }
    }
}