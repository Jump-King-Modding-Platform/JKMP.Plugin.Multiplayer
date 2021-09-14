using EntityComponent;
using Microsoft.Xna.Framework;

namespace JKMP.Plugin.Multiplayer.Game.Components
{
    /// <summary>
    /// A simple transform component that only holds a 2d position.
    /// </summary>
    public class Transform : Component
    {
        public Vector2 Position { get; set; }
    }
}