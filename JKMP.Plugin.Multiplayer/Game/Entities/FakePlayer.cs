using System;
using EntityComponent;
using EntityComponent.BT;
using JumpKing;
using JumpKing.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class FakePlayer : Entity
    {
        private readonly BodyComp body;
        private Sprite sprite;
        private bool flip;

        public FakePlayer()
        {
            body = new BodyComp(Vector2.Zero, 18, 16);
            sprite = JKContentManager.PlayerSprites.idle;
        }

        public void SetPositionAndVelocity(Vector2 position, Vector2 velocity)
        {
            body.position = position;
            body.velocity = velocity;
        }

        public override void Draw()
        {
            sprite.Draw(Camera.TransformVector2(body.position + new Vector2(9, 26)), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }

        public void SetSprite(Sprite newSprite)
        {
            sprite = newSprite ?? throw new ArgumentNullException(nameof(newSprite));
        }

        public void SetDirection(int direction)
        {
            if (direction == 0)
                return;
            
            flip = direction < 0;
        }
    }
}