using System;
using EntityComponent;
using EntityComponent.BT;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Components;
using JumpKing;
using JumpKing.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class FakePlayer : Entity
    {
        private Sprite sprite;
        private bool flip;
        
        private readonly Transform transform;

        public FakePlayer()
        {
            sprite = JKContentManager.PlayerSprites.idle;
            transform = new();
            AddComponents(transform);
        }

        public void SetPosition(Vector2 position)
        {
            transform.Position = position;
        }

        public override void Draw()
        {
            sprite.Draw(Camera.TransformVector2(transform.Position + new Vector2(9, 26)), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
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