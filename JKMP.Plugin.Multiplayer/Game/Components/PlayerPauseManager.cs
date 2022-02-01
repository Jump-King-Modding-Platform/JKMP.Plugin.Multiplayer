using System;
using EntityComponent;
using JumpKing.PauseMenu;
using JumpKing.Player;

namespace JKMP.Plugin.Multiplayer.Game.Components
{
    internal class PlayerPauseManager : Component
    {
        private BodyComp body = null!;

        protected override void Init()
        {
            if (gameObject is not PlayerEntity)
                throw new InvalidOperationException("PlayerPauseManager can only be attached to a PlayerEntity");
            
            body = GetComponent<BodyComp>();
        }

        protected override void LateUpdate(float delta)
        {
            if (PauseManager.instance.IsPaused && CanPausePlayer())
            {
                body.Enabled = false;
            }
            else
            {
                body.Enabled = true;
            }
        }

        private bool CanPausePlayer()
        {
            return body.IsOnGround;
        }
    }
}