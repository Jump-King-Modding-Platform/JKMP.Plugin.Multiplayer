using EntityComponent;
using JumpKing.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace JKMP.Plugin.Multiplayer.Game.Components
{
    public class AudioEmitterComponent : Component
    {
        public AudioEmitter AudioEmitter { get; }

        private Vector2 Position => body?.position ?? transform!.Position;

        private Vector2 lastPosition;
        private Transform? transform;
        private BodyComp? body;

        public AudioEmitterComponent()
        {
            AudioEmitter = new();
        }

        protected override void Init()
        {
            transform = GetComponent<Transform>();
            body = GetComponent<BodyComp>();
        }

        protected override void LateUpdate(float delta)
        {
            AudioEmitter.Position = new Vector3(Position, 0);

            if (body != null)
                AudioEmitter.Velocity = new Vector3(body.velocity, 0);
            else
            {
                AudioEmitter.Velocity = new Vector3(transform!.Position - lastPosition, 0);
                lastPosition = Position;
            }
        }
    }
}