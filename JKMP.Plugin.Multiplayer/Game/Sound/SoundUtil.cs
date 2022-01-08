using Microsoft.Xna.Framework;

namespace JKMP.Plugin.Multiplayer.Game.Sound
{
    public static class SoundUtil
    {
        public static Vector3 ScalePosition(Vector2 position)
        {
            float scaleX = 0.002f;
            float scaleY = 0.02f;
            
            return new Vector3(position.X * scaleX, position.Y * scaleY, 0);
        }
    }
}