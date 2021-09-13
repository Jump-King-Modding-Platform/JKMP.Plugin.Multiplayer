using System;
using System.Collections.Generic;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Game.Player;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using JumpKing;
using Microsoft.Xna.Framework;

namespace JKMP.Plugin.Multiplayer.Game.Components
{
    public class RemotePlayerInterpolator : Component
    {
        private PlayerStateChanged? lastState;
        private PlayerStateChanged? nextState;
        private float elapsedTimeSinceLastState;

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

                elapsedTimeSinceLastState += delta;
            }
        }

        private void PlayStateSounds(PlayerStateChanged stateA, PlayerStateChanged stateB)
        {
            if (stateA.State != stateB.State)
            {
                switch (stateB.State)
                {
                    case PlayerState.Jump:
                        // todo: play jump sound
                        break;
                    case PlayerState.Land:
                        // todo: play land sound
                        break;
                    case PlayerState.Knocked:
                        // todo: play bounce sound
                        break;
                    case PlayerState.Splat:
                        // todo: play splat sound
                        break;
                }
            }
        }
    }
}