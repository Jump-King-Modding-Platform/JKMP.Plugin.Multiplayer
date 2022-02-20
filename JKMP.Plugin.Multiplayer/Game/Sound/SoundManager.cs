using System;
using System.Collections.Generic;
using JKMP.Core.Logging;
using JumpKing.PlayerPreferences;
using Microsoft.Xna.Framework.Audio;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Game.Sound
{
    internal class SoundManager
    {
        public const int MaxPlayingSounds = 64;

        public SoundPrefs SoundPrefs => SoundUtil.SoundPrefs;
        
        public AudioListener? GlobalListener { get; set; }

        public int NumPlayingSounds => playingSounds.Count;
        public int AvailableSoundCount => Math.Max(0, NumPlayingSounds - MaxPlayingSounds);

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

            if (!SoundPrefs.sfx_on)
                return;

            var instance = sound.CreateInstance();
            playingSounds.Add(instance);

            instance.Apply3D(GlobalListener, emitter);
            instance.Volume = volume * SoundPrefs.master;
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

        public void StopOldestSound()
        {
            if (playingSounds.Count == 0)
                return;
            
            var oldest = playingSounds[0];
            oldest.Stop();
            oldest.Dispose();
            playingSounds.RemoveAt(0);
        }
    }
}