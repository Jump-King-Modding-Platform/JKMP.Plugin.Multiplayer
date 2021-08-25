using EntityComponent;
using EntityComponent.BT;
using JumpKing;
using JumpKing.Player;
using Microsoft.Xna.Framework;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class FakePlayer : Entity
    {
        private BodyComp body;
        private Sprite sprite;
        private BehaviorTreeComp bt;

        public FakePlayer()
        {
            body = new BodyComp(Vector2.Zero, 18, 16);
            sprite = JKContentManager.PlayerSprites.idle;
        }

        public void SetPosition(Vector2 position)
        {
            body.position = position;
        }

        public override void Draw()
        {
            sprite.Draw(Camera.TransformVector2(body.position + new Vector2(9, 26)));
        }
    }
}