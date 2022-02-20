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
        public Sprite Sprite => sprite;

        public Transform Transform => transform;
        
        private Sprite sprite;
        private bool flip;
        
        private readonly Transform transform;
        private readonly FollowingTextRenderer nameDisplay;
        private readonly FollowingTextRenderer messageDisplay;
        private readonly VoiceManager voice;

        public FakePlayer()
        {
            voice = new VoiceManager();
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

            AddComponents(transform, nameDisplay, messageDisplay, voice, new AudioEmitterComponent(), new RemotePlayerInterpolator());
        }

        public void SetPosition(Vector2 position)
        {
            transform.Position = position;
        }

        public override void Draw()
        {
            sprite.Draw(Camera.TransformVector2(transform.Position + new Vector2(9, 26)), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

            // Draw voice icon left of the name if speaking
            if (voice.IsSpeaking)
            {
                var textSize = Content.Fonts.LocalChatFont.MeasureString(nameDisplay.Text);
                var drawPos = Camera.TransformVector2(transform.Position + new Vector2((-textSize.X / 2f) - 7, -15));

                Game1.spriteBatch.Draw(Content.UI.VoiceIcon, drawPos, nameDisplay.Color);
            }
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