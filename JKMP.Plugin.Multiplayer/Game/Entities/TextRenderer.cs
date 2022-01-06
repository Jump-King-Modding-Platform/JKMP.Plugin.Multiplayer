using System;
using System.Text;
using EntityComponent;
using JKMP.Plugin.Multiplayer.Game.Components;
using JumpKing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class TextRenderer : Entity
    {
        /// <summary>
        /// Gets or sets the time in seconds to display the label when the text is changed. If set to 0, the label will always be displayed.
        /// </summary>
        public float TimeUntilMessageFade { get; set; }
        /// <summary>
        /// Gets or sets the time in seconds it takes to fade out the text.
        /// </summary>
        public float MessageFadeTime { get; set; }
        public SpriteFont? Font { get; set; }
        public float Opacity { get; set; } = 1;
        public float Scale { get; set; } = 1;
        public Color Color { get; set; } = Color.White;
        public Transform Transform { get; }

        public string? Text
        {
            get => text;
            set
            {
                if (value == text)
                    return;
                
                text = value;
                UpdateWrappedString();
            }
        }

        /// <summary>
        /// Gets or sets the max pixel width of the text before it is wrapped.
        /// </summary>
        public int? MaxWidth
        {
            get => maxWidth;
            set
            {
                if (value == maxWidth)
                    return;
                
                maxWidth = value;
                UpdateWrappedString();
            }
        }

        private string? wrappedString;
        private Vector2 wrappedStringSize;
        private int? maxWidth;
        private string? text;
        private float timeSinceLastMessage;

        public TextRenderer(SpriteFont font)
        {
            Font = font ?? throw new ArgumentNullException(nameof(font));
            Transform = new Transform();
            AddComponents(Transform);
        }

        protected override void Update(float delta)
        {
            if (TimeUntilMessageFade > 0)
            {
                timeSinceLastMessage = timeSinceLastMessage += delta;

                if (timeSinceLastMessage >= TimeUntilMessageFade)
                {
                    float opacity = 1f - MathHelper.Clamp((timeSinceLastMessage - TimeUntilMessageFade) / MessageFadeTime, 0, 1);
                    Opacity = opacity;
                }
                else
                {
                    Opacity = 1;
                }
            }
            else
            {
                Opacity = 1;
            }
        }

        public override void Draw()
        {
            if (Font == null || string.IsNullOrEmpty(wrappedString) || Color.A == 0)
                return;

            Color drawColor = Color * MathHelper.Clamp(Opacity, 0, 1);
            Vector2 drawPosition = Transform.Position;

            drawPosition.X -= wrappedStringSize.X / 2f;
            drawPosition.Y -= wrappedStringSize.Y; // Bottom align text
            drawPosition = Camera.TransformVector2(drawPosition);

            // Align draw position to nearest pixel so that text doesn't jitter
            drawPosition.X = (float)Math.Round(drawPosition.X);
            drawPosition.Y = (float)Math.Round(drawPosition.Y);

            Vector2 drawScale = new Vector2(Scale, Scale);

            Game1.spriteBatch.DrawString(Font, wrappedString, drawPosition, drawColor, 0, Vector2.Zero, drawScale, SpriteEffects.None, 0);
        }

        public void ShowMessage(string? message)
        {
            Text = message;
            timeSinceLastMessage = 0;
        }

        private string? WrapString(string? text)
        {
            if (MaxWidth <= 0)
                throw new InvalidOperationException("MaxWidth must be greater than 0");

            if (text == null)
                return null;

            StringBuilder builder = new();
            StringBuilder currentLine = new();
            string[] words = text.Split(null);
            
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                string newText = $"{currentLine}{word}";
                Vector2 size = Font!.MeasureString(newText) * Scale;

                if (size.X > MaxWidth)
                {
                    builder.Append(currentLine + Environment.NewLine);
                    currentLine.Clear();
                }
                
                currentLine.Append(i < words.Length - 1 ? $"{word} " : word);
            }

            builder.Append(currentLine);

            return builder.ToString();
        }

        private void UpdateWrappedString()
        {
            wrappedString = WrapString(Text);
            wrappedStringSize = wrappedString != null ? Font!.MeasureString(wrappedString) : Vector2.Zero;
        }
    }
}