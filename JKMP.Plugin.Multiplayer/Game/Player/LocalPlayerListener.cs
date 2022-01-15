using System;
using EntityComponent;
using JKMP.Plugin.Multiplayer.Game.Events;
using JumpKing;
using JumpKing.Level;
using JumpKing.Player;
using Microsoft.Xna.Framework;

namespace JKMP.Plugin.Multiplayer.Game.Player
{
    public class LocalPlayerListener : IDisposable
    {
        public PlayerState CurrentState { get; private set; }
        public int WalkDirection { get; private set; } = 1;
        internal Content.SurfaceType CurrentSurfaceType { get; private set; }
        
        public Vector2 Position => localBody.position;
        public Vector2 Velocity => localBody.velocity;

        public Action? StartJump { get; set; }
        public Action? Jump { get; set; }
        public Action<bool>? StartedFalling { get; set; }
        public Action? Knocked { get; set; }
        public Action<bool>? Land { get; set; }
        public Action<int>? Walk { get; set; }
        
        private readonly BodyComp localBody;
        private readonly InputComponent inputComp;
        private bool lastIsOnGround;
        private bool lastIsKnocked;
        private int lastWalkDirection;
        private bool triggerStartJumpNextUpdate;
        
        public LocalPlayerListener()
        {
            PlayerEntity localPlr = EntityManager.instance.Find<PlayerEntity>() ?? throw new NotImplementedException("Local player not found");
            localBody = localPlr.GetComponent<BodyComp>();
            inputComp = localPlr.GetComponent<InputComponent>();

            PlayerEvents.StartJump += OnStartJump;
            PlayerEntity.OnJumpCall += OnJump;
        }

        public void Update(float delta)
        {
            if (localBody.LastVelocity.Y <= PlayerValues.GRAVITY && localBody.velocity.Y > PlayerValues.GRAVITY)
            {
                CurrentState = PlayerState.Falling;
                StartedFalling?.Invoke(localBody.IsKnocked);
            }

            if (!lastIsOnGround && localBody.IsOnGround)
            {
                CurrentState = localBody.LastVelocity.Y >= PlayerValues.MAX_FALL ? PlayerState.Splat : PlayerState.Land;
                Land?.Invoke(localBody.LastVelocity.Y >= PlayerValues.MAX_FALL);

                if (triggerStartJumpNextUpdate)
                {
                    triggerStartJumpNextUpdate = false;
                    CurrentState = PlayerState.StartJump;
                    StartJump?.Invoke();
                }
            }

            if (!lastIsKnocked && localBody.IsKnocked)
            {
                Knocked?.Invoke();
                CurrentState = PlayerState.Knocked;
            }

            int walkDirection = 0;
            
            if (localBody.IsOnGround)
            {
                if (triggerStartJumpNextUpdate)
                {
                    triggerStartJumpNextUpdate = false;
                    CurrentState = PlayerState.StartJump;
                    StartJump?.Invoke();
                }
                
                InputComponent.State pressedState = inputComp.GetState();

                if (!pressedState.jump)
                {
                    if (pressedState.left)
                    {
                        walkDirection -= 1;
                    }

                    if (pressedState.right)
                    {
                        walkDirection += 1;
                    }

                    if (walkDirection != lastWalkDirection)
                    {
                        Walk?.Invoke(walkDirection);
                        CurrentState = walkDirection != 0 ? PlayerState.Walk : PlayerState.Idle;
                    }

                    WalkDirection = (sbyte)walkDirection;
                }
            }

            lastIsOnGround = localBody.IsOnGround;
            lastIsKnocked = localBody.IsKnocked;
            lastWalkDirection = walkDirection;

            CurrentSurfaceType = GetCurrentSurfaceType();
        }
        
        private Content.SurfaceType GetCurrentSurfaceType()
        {
            var collisionInfo = LevelManager.GetCollisionInfo(localBody.GetHitbox());

            if (collisionInfo.ice)
                return Content.SurfaceType.Ice;
            if (collisionInfo.sand)
                return Content.SurfaceType.Sand;
            if (collisionInfo.snow)
                return Content.SurfaceType.Snow;
            if (collisionInfo.water)
                return Content.SurfaceType.Water;

            return Content.SurfaceType.Default;
        }

        public void Dispose()
        {
            PlayerEvents.StartJump -= OnStartJump;
            PlayerEntity.OnJumpCall -= OnJump;
        }

        private void OnStartJump()
        {
            triggerStartJumpNextUpdate = true;
        }

        private void OnJump()
        {
            CurrentState = PlayerState.Jump;
            Jump?.Invoke();

            WalkDirection = localBody.velocity.X switch
            {
                < 0 => -1,
                > 0 => 1,
                _ => 0
            };
        }
    }
}