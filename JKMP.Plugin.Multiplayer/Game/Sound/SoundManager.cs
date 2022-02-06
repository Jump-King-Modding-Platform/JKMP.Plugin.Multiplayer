using System;
using System.Collections.Generic;
using JKMP.Core.Logging;
using JumpKing.PlayerPreferences;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Game.Sound
{
    internal class SoundManager
    {
        internal AudioListener? GlobalListener { get; set; }

        private readonly List<SoundEffectInstance> playingSounds = new();

        private static readonly ILogger Logger = LogManager.CreateLogger<SoundManager>();

        public void PlaySound(SoundEffect sound, AudioEmitter emitter, float volume = 1.0f)
        {
            if (sound == null) throw new ArgumentNullException(nameof(sound));
            if (emitter == null) throw new ArgumentNullException(nameof(emitter));
            
            if (GlobalListener == null)
            {
                Logger.Warning("Tried to play a sound without an audio listener");
                return;
            }

            var instance = sound.CreateInstance();
            playingSounds.Add(instance);

            instance.Apply2DPanAndVolume(GlobalListener, emitter, SoundUtil.SoundType.Sfx, volume);
            instance.Play();
        }

        public void Update(float delta)
        {
            for (int i = playingSounds.Count - 1; i >= 0; --i)
            {
                var instance = playingSounds[i];

                if (instance.State == SoundState.Stopped)
                {
                    instance.Dispose();
                    playingSounds.RemoveAt(i);
                }
            }
        }
    }
}