using System;
using System.Collections.Generic;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Game.Player;
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

        private static readonly Dictionary<PlayerState, Sprite> StateSprites = new()
        {
            { PlayerState.Idle, JKContentManager.PlayerSprites.idle },
            { PlayerState.Falling, JKContentManager.PlayerSprites.jump_fall },
            { PlayerState.StartJump, JKContentManager.PlayerSprites.jump_charge },
            { PlayerState.Jump, JKContentManager.PlayerSprites.jump_up },
            { PlayerState.Knocked, JKContentManager.PlayerSprites.jump_bounce },
            { PlayerState.Land, JKContentManager.PlayerSprites.idle },
            { PlayerState.Walk, JKContentManager.PlayerSprites.walk_one },
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
                FakePlayer.SetSprite(StateSprites[lastState.State]);
                FakePlayer.SetDirection(nextState.WalkDirection);

                float lerpDelta = (float)(elapsedTimeSinceLastState / (1d / 30d));
                lerpDelta = MathHelper.Clamp(lerpDelta, 0, 1);
                Vector2 position = Vector2.Lerp(lastState.Position, nextState.Position, lerpDelta);
                FakePlayer.SetPosition(position);
                UpdateAudioEmitter();

                elapsedTimeSinceLastState += delta;
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