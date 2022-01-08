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
        private readonly FollowingTextRenderer nameDisplay;
        private readonly FollowingTextRenderer messageDisplay;

        public FakePlayer()
        {
            sprite = JKContentManager.PlayerSprites.idle;
            transform = new();
            nameDisplay = new(Content.Fonts.LocalChatFont)
            {
                Offset = new Vector2(9, 0),
                MaxWidth = 200
            };
            messageDisplay = new(Content.Fonts.LocalChatFont)
            {
                Offset = new Vector2(9, -12),
                TimeUntilMessageFade = 10f,
                MessageFadeTime = 0.5f,
                MaxWidth = 200
            };
            AddComponents(transform, nameDisplay, messageDisplay);
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

        /// <summary>
        /// Shows the given message above the player's head. If null then the message is hidden.
        /// </summary>
        public void Say(string? message)
        {
            messageDisplay.ShowMessage(message);
        }
        
        /// <summary>
        /// Sets the name displayed above the character's head.
        /// </summary>
        public void SetName(string name)
        {
            nameDisplay.Text = name;
        }

        /// <summary>
        /// Sets the color of the name displayed above the character's head.
        /// </summary>
        public void SetNameColor(Color color)
        {
            nameDisplay.Color = color;
        }
    }
}