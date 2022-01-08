using System;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JumpKing.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Game.Components
{
    public class FollowingTextRenderer : Component
    {
        public string? Text
        {
            get => textRenderer.Text;
            set => textRenderer.Text = value;
        }
        
        public Vector2 Offset { get; set; }

        public Color Color
        {
            get => textRenderer.Color;
            set => textRenderer.Color = value;
        }
        
        /// <summary>
        /// Gets or sets the time in seconds to display the label when the text is changed. If set to 0, the label will always be displayed.
        /// </summary>
        public float TimeUntilMessageFade
        {
            get => textRenderer.TimeUntilMessageFade;
            set => textRenderer.TimeUntilMessageFade = value;
        }

        /// <summary>
        /// Gets or sets the time in seconds it takes to fade out the text.
        /// </summary>
        public float MessageFadeTime
        {
            get => textRenderer.MessageFadeTime;
            set => textRenderer.MessageFadeTime = value;
        }

        public SpriteFont? Font
        {
            get => textRenderer.Font;
            set => textRenderer.Font = value;
        }

        /// <summary>
        /// Gets or sets the max pixel width of the text before it is wrapped.
        /// </summary>
        public int? MaxWidth
        {
            get => textRenderer.MaxWidth;
            set => textRenderer.MaxWidth = value;
        }

        private Transform? transform;
        private BodyComp? bodyComp;

        private readonly TextRenderer textRenderer;

        private static readonly ILogger Logger = LogManager.CreateLogger<FollowingTextRenderer>();

        public FollowingTextRenderer(SpriteFont font)
        {
            if (font == null) throw new ArgumentNullException(nameof(font));

            textRenderer = new(font);
        }

        protected override void Init()
        {
            transform = GetComponent<Transform>();
            bodyComp = GetComponent<BodyComp>();
            
            if (transform == null && bodyComp == null)
                throw new InvalidOperationException("LabelDisplay requires a Transform or BodyComp component");
        }

        protected override void LateUpdate(float delta)
        {
            textRenderer.Transform.Position = (transform?.Position ?? bodyComp!.position) + Offset;
        }

        public void ShowMessage(string? message)
        {
            textRenderer.ShowMessage(message);
        }
    }
}