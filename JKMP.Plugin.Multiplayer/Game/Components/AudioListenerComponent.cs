using System;
using System.Collections.Generic;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Game.Sound;
using JumpKing;
using JumpKing.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Game.Components
{
    public class AudioListenerComponent : Component
    {
        public AudioListener Listener { get; }

        private Transform? transform;
        private BodyComp? bodyComp;

        private Vector2 Position => transform?.Position ?? bodyComp!.position;

        private static readonly ILogger Logger = LogManager.CreateLogger<AudioListenerComponent>();

        public AudioListenerComponent()
        {
            Listener = new()
            {
                Forward = new Vector3(0, 0, -1),
                Up = Vector3.Up
            };
        }
        
        protected override void Init()
        {
            transform = GetComponent<Transform>();
            bodyComp = GetComponent<BodyComp>();

            if (transform == null && bodyComp == null)
                throw new NotSupportedException("Could not find a Transform or a BodyComp component on the owning entity");
        }

        protected override void OnEnable()
        {
            var soundManager = EntityManager.instance.Find<GameEntity>().Sound;

            if (soundManager.GlobalListener != null)
            {
                Logger.Warning("There is more than one audio listener component in the world");
                return;
            }
            
            soundManager.GlobalListener = Listener;
        }

        protected override void OnDisable()
        {
            var gameEntity = EntityManager.instance.Find<GameEntity>();

            if (gameEntity == null) // Variable is null when game state is changing to main menu or game is shutting down
                return;
            
            var soundManager = gameEntity.Sound;

            if (soundManager.GlobalListener == Listener)
                soundManager.GlobalListener = null;
        }

        protected override void OnOwnerDestroy()
        {
            OnDisable();
        }

        protected override void LateUpdate(float delta)
        {
            Listener.Position = new Vector3(Position.X, Position.Y, -50);

            if (bodyComp != null)
                Listener.Velocity = new Vector3(bodyComp.velocity.X, bodyComp.velocity.Y, 0);
        }
    }
}