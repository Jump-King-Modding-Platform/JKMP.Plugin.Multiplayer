using System;
using System.IO;
using EntityComponent;
using HarmonyLib;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Input;
using JKMP.Plugin.Multiplayer.Game.Sound;
using JKMP.Plugin.Multiplayer.Networking;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using JumpKing.Player;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Steamworks;
using Steamworks.Data;

namespace JKMP.Plugin.Multiplayer.Game.Components
{
    public class VoiceManager : Component
    {
        /// <summary>
        /// Gets whether this voice manager is managing the local player.
        /// </summary>
        public bool IsLocalPlayer { get; private set; }

        /// <summary>
        /// True if the player is currently speaking.
        /// If set to true on the local player then we will start transmitting audio to all other players.
        /// </summary>
        public bool IsSpeaking
        {
            get => isSpeaking;
            private set
            {
                if (isSpeaking == value)
                    return;

                isSpeaking = value;

                if (IsLocalPlayer)
                {
                    SteamUser.VoiceRecord = value;
                }
            }
        }

        private Transform? transform;
        private BodyComp? body;
        private MemoryStream? transmitStream;
        private DynamicSoundEffectInstance? sound;
        private AudioEmitter? audioEmitter;
        private AudioListener? localAudioListener;
        
        // Used by Apply3D internally to get information about the sound.
        // There's a monogame crash where Apply3D throws NRE when called on a DynamicSoundEffectInstance.
        // This is a workaround.
        private SoundEffect? dummySoundEffect;
        
        private readonly byte[]? transmitBuffer = new byte[(int)SteamUser.OptimalSampleRate * 5];

        private bool isSpeaking;

        protected override void Init()
        {
            IsLocalPlayer = gameObject is PlayerEntity;
            transform = GetComponent<Transform>();
            body = GetComponent<BodyComp>();

            if (IsLocalPlayer)
            {
                transmitStream = new MemoryStream(transmitBuffer, true);
            }
            else
            {
                sound = new DynamicSoundEffectInstance((int)SteamUser.OptimalSampleRate, AudioChannels.Mono);
                dummySoundEffect = new SoundEffect(new byte[] { 0, 0 }, (int)SteamUser.OptimalSampleRate, AudioChannels.Mono);
                AccessTools.Field(typeof(DynamicSoundEffectInstance), "_effect").SetValue(sound, dummySoundEffect);
                sound.Volume = 1f; // todo: make this configurable
                audioEmitter = new AudioEmitter();
                localAudioListener = EntityManager.instance.Find<PlayerEntity>().GetComponent<AudioListenerComponent>().Listener;
            }
        }

        protected override void OnOwnerDestroy()
        {
            IsSpeaking = false;
            sound?.Dispose();
            dummySoundEffect?.Dispose();
        }

        protected override void Update(float delta)
        {
            IsSpeaking = InputManager.GameInputEnabled && InputManager.KeyDown(Keys.V);

            if (IsLocalPlayer)
            {
                TransmitVoice();
            }

            if (!IsLocalPlayer)
            {
                IsSpeaking = sound!.State == SoundState.Playing;

                if (IsSpeaking)
                {
                    audioEmitter!.Position = SoundUtil.ScalePosition(transform!.Position);

                    if (localAudioListener != null)
                    {
                        sound.Apply3D(localAudioListener, audioEmitter);
                    }
                }
            }
        }

        private void TransmitVoice()
        {
            if (!IsLocalPlayer)
                throw new InvalidOperationException("Cannot transmit voice on a non-local player.");
            
            if (SteamUser.HasVoiceData)
            {
                transmitStream!.Seek(0, SeekOrigin.Begin);
                int bytesWritten = SteamUser.ReadVoiceData(transmitStream);

                if (bytesWritten > 0)
                {
                    Span<byte> bytes = new Span<byte>(transmitBuffer, 0, bytesWritten);
                    P2PManager.Instance?.Broadcast(new VoiceTransmission
                    {
                        Data = bytes.ToArray()
                    });
                }
            }
        }

        public void ReceiveVoice(Span<byte> data)
        {
            if (IsLocalPlayer)
                throw new InvalidOperationException("Cannot receive voice on the local player.");

            // todo: catch exceptions in case the voice data is corrupted or otherwise invalid
            sound!.SubmitBuffer(data.ToArray());

            if (sound.State != SoundState.Playing)
            {
                sound.Play();
            }
        }
    }
}