using System;
using JKMP.Core.Logging;
using JumpKing.PlayerPreferences;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace JKMP.Plugin.Multiplayer.Game.Sound
{
    public static class SoundUtil
    {
        public enum SoundType
        {
            Sfx,
            Voice,
        }
        
        /// <summary>
        /// This used to be a hack to scale sound volume based on the distance from the player
        /// but it's not needed anymore after setting SoundEffect.DistanceScale.
        /// Now it's just used to convert a Vector2 to a Vector3.
        /// </summary>
        public static Vector3 ScalePosition(Vector2 position)
        {
            return new Vector3(position.X, position.Y, 0);
        }

        public static void Apply2DPanAndVolume(
            this SoundEffectInstance instance,
            AudioListener listener,
            AudioEmitter emitter,
            SoundType soundType = SoundType.Sfx,
            float volume = 1f,
            float minFalloffDistance = 200,
            float maxFalloffDistance = 400)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            if (emitter == null) throw new ArgumentNullException(nameof(emitter));
            
            SoundPrefs soundPrefs = ISettingManager<SoundPrefsRuntime, SoundPrefs>.instance.GetPrefs();

            if (!soundPrefs.sfx_on && soundType == SoundType.Sfx)
            {
                instance.Volume = 0;
                return;
            }

            // Calculate the 2D pan vector within 300 units (pixels)
            Vector2 pan = new Vector2(
                (emitter.Position.X - listener.Position.X) / 300,
                (emitter.Position.Y - listener.Position.Y) / 300);

            // Clamp the pan vector from -1 to 1
            pan = Vector2.Clamp(pan, -Vector2.One, Vector2.One);

            instance.Pan = pan.X;

            // Scale volume between 0-1 based on the distance between the emitter and the listener
            float distance = Vector3.DistanceSquared(emitter.Position, listener.Position);
            float minFalloffSquared = minFalloffDistance * minFalloffDistance;
            float maxFalloffSquared = maxFalloffDistance * maxFalloffDistance;
            
            if (distance > minFalloffSquared)
            {
                float volumeFalloff = (distance - minFalloffSquared) / (maxFalloffSquared - minFalloffSquared);
                volumeFalloff = MathHelper.Clamp(volumeFalloff, 0, 1);

                volume *= 1 - volumeFalloff;
            }

            instance.Volume = volume * soundPrefs.master;
        }
    }
}