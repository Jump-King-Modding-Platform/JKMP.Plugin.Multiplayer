using System;
using EntityComponent;
using JKMP.Plugin.Multiplayer.Game.Components;
using JKMP.Plugin.Multiplayer.Networking;
using JumpKing;
using JumpKing.Player;
using Microsoft.Xna.Framework;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class LocalPlayer : Entity
    {
        private readonly VoiceManager voice;
        private readonly PlayerEntity player;
        private readonly BodyComp playerBody;

        /// <summary>
        /// Gets the player position.
        /// </summary>
        private Vector2 Position => playerBody.position;

        public LocalPlayer(P2PManager p2p)
        {
            player = EntityManager.instance.Find<PlayerEntity>() ?? throw new InvalidOperationException("Player entity not found");
            playerBody = player.GetComponent<BodyComp>();

            player.AddComponents(
                new PlayerStateTransmitter(p2p),
                new AudioListenerComponent(),
                new PlayerPauseManager(),
                voice = new VoiceManager()
            );
        }

        public override void Draw()
        {
            if (voice.IsTransmitting)
            {
                // Draw voice icon on top of the player
                var position = Position + new Vector2(0, -15);
                position = Camera.TransformVector2(position);
                
                Game1.spriteBatch.Draw(Content.UI.VoiceIcon, position, Color.LightGreen);
            }
        }
    }
}