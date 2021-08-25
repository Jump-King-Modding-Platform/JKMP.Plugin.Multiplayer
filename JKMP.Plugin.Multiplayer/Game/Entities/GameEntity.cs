using System;
using EntityComponent;
using JKMP.Core.Logging;
using JumpKing.Player;
using Microsoft.Xna.Framework;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class GameEntity : BaseManagerEntity
    {
        private FakePlayer fakePlayer;
        private PlayerEntity localPlr;
        private BodyComp localBody;

        protected override void OnFirstUpdate()
        {
            localPlr = Find<PlayerEntity>() ?? throw new NotImplementedException("Local player not found");
            localBody = localPlr.GetComponent<BodyComp>();
            fakePlayer = new();
        }

        protected override void Update(float delta)
        {
            base.Update(delta);

            fakePlayer.SetPosition(localBody.position + new Vector2(0, -50));
        }
    }
}