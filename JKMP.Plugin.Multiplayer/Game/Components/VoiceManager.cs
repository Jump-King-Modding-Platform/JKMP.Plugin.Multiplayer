using System;
using System.Collections.Generic;
using System.IO;
using EntityComponent;
using HarmonyLib;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Game.Input;
using JKMP.Plugin.Multiplayer.Game.Sound;
using JKMP.Plugin.Multiplayer.Native;
using JKMP.Plugin.Multiplayer.Native.Audio;
using JKMP.Plugin.Multiplayer.Networking;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using JumpKing.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Serilog;
using Steamworks;
using OpusContext = JKMP.Plugin.Multiplayer.Native.Audio.OpusContext;

namespace JKMP.Plugin.Multiplayer.Game.Components
{
    public class VoiceManager : Component
    {
        /// <summary>
        /// Gets or sets the device by name that is used for voice input.
        /// If set to null, the default device will be used.
        /// </summary>
        public static string? SelectedDeviceName { get; set; }

        /// <summary>
        /// Gets or sets the volume. The effective value is clamped between 0 and 2.5, however the value is not clamped when set.
        /// </summary>
        public static double Volume
        {
            get => volume;
            set
            {
                if (value.Equals(volume))
                    return;

                volume = value;
                VolumeChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Gets or sets the volume of other players. The value is clamped between 0 and 1.
        /// </summary>
        public static float PlayerVolume
        {
            get => playerVolume;
            set => playerVolume = MathHelper.Clamp(value, 0, 1);
        }

        private static double volume = 1.0;
        private static float playerVolume = 1f;

        public static Action<double>? VolumeChanged { get; set; }

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

                if (IsLocalPlayer && captureContext != null)
                {
                    if (value)
                    {
                        if (UpdateSelectedInputDevice())
                            captureContext.StartCapture(OnVoiceData, OnVoiceError);
                    }
                    else
                    {
                        captureContext.StopCapture();
                    }
                }
            }
        }

        /// <summary>
        /// Gets whether the local player is actually transmitting voice audio.
        /// </summary>
        public bool IsTransmitting => IsLocalPlayer && timeSinceTalked < 0.25f;

        private Transform? transform;
        private BodyComp? body;
        
        private DynamicSoundEffectInstance? sound;
        private AudioCaptureContext? captureContext;
        private readonly OpusContext opusContext;
        private static readonly Memory<short> DecodeBuffer = new short[4096];
        private static readonly Memory<byte> EncodeBuffer = new byte[1024]; // This needs a significantly smaller buffer size compared to decodeBuffer.
        private readonly Queue<byte[]> pendingOutgoingVoiceData = new();
        private float timeSinceTransmittedVoice;
        private const double TransmissionInterval = 0.05f; // Transmit voice every 50ms
        
        // Used by Apply3D internally to get information about the sound.
        // There's a monogame crash where Apply3D throws NRE when called on a DynamicSoundEffectInstance.
        // This is a workaround.
        private SoundEffect? dummySoundEffect;
        
        private bool isSpeaking;
        private float timeSinceTalked = 0.25f;

        private readonly SoundManager soundManager;

        private AudioEmitter? AudioEmitter => audioEmitterComponent?.AudioEmitter;
        private AudioEmitterComponent? audioEmitterComponent;

        private static readonly ILogger Logger = LogManager.CreateLogger<VoiceManager>();

        public VoiceManager()
        {
            soundManager = EntityManager.instance.Find<GameEntity>()?.Sound ?? throw new InvalidOperationException("GameEntity or SoundManager not found");
            opusContext = new(48000);
        }

        protected override void Init()
        {
            IsLocalPlayer = gameObject is PlayerEntity;
            transform = GetComponent<Transform>();
            body = GetComponent<BodyComp>();

            if (IsLocalPlayer)
            {
                captureContext = new AudioCaptureContext();
                captureContext.SetVolume(Volume);
                VolumeChanged += OnVolumeChanged;
            }
            else
            {
                audioEmitterComponent = GetComponent<AudioEmitterComponent>() ?? throw new InvalidOperationException("AudioEmitterComponent not found");
                
                sound = new DynamicSoundEffectInstance(48000, AudioChannels.Mono);
                
                dummySoundEffect = new SoundEffect(new byte[] { 0, 0 }, (int)SteamUser.OptimalSampleRate, AudioChannels.Mono);
                AccessTools.Field(typeof(DynamicSoundEffectInstance), "_effect").SetValue(sound, dummySoundEffect);
            }
        }

        protected override void OnOwnerDestroy()
        {
            IsSpeaking = false;
            sound?.Dispose();
            dummySoundEffect?.Dispose();
            captureContext?.Dispose();
            opusContext.Dispose();

            if (IsLocalPlayer)
            {
                VolumeChanged -= OnVolumeChanged;
            }
        }
        
        private void OnVolumeChanged(double volume)
        {
            if (IsLocalPlayer)
                captureContext?.SetVolume(volume);
        }

        protected override void Update(float delta)
        {
            IsSpeaking = InputManager.GameInputEnabled && InputManager.KeyDown(Keys.V);
            timeSinceTalked += delta;

            if (IsLocalPlayer)
            {
                TransmitVoice(delta);
            }

            if (!IsLocalPlayer)
            {
                if (timeSinceTalked >= 0.25f)
                    sound!.Stop();

                IsSpeaking = sound!.State == SoundState.Playing;

                if (IsSpeaking)
                {
                    if (soundManager.GlobalListener != null)
                    {
                        sound.Apply3D(soundManager.GlobalListener, AudioEmitter);
                    }
                }
                else
                {
                    sound.Stop();
                }
            }
        }

        private void OnVoiceData(ReadOnlySpan<short> data, float maxVolume)
        {
            timeSinceTalked = 0;
            
            lock (pendingOutgoingVoiceData)
            {
                int numBytes = opusContext.Compress(data, EncodeBuffer.Span);

                if (numBytes <= 0)
                    return;

                Span<byte> compressedData = EncodeBuffer.Span.Slice(0, numBytes);
                pendingOutgoingVoiceData.Enqueue(compressedData.ToArray());
            }
        }

        private void OnVoiceError(CaptureError captureError)
        {
            Logger.Warning("Voice capture error: {captureError}", captureError);
        }

        private void TransmitVoice(float delta)
        {
            if (!IsLocalPlayer)
                throw new InvalidOperationException("Cannot transmit voice on a non-local player.");

            timeSinceTransmittedVoice += delta;

            if (timeSinceTransmittedVoice >= TransmissionInterval)
            {
                timeSinceTransmittedVoice = 0;

                lock (pendingOutgoingVoiceData)
                {
                    if (pendingOutgoingVoiceData.Count > 0)
                    {
                        P2PManager.Instance?.Broadcast(new VoiceTransmission
                        {
                            Data = pendingOutgoingVoiceData
                        });

                        pendingOutgoingVoiceData.Clear();
                    }
                }
            }
        }

        private bool UpdateSelectedInputDevice()
        {
            if (captureContext == null)
                return false;

            bool hasDevice = SelectedDeviceName == null ? captureContext.SetActiveDeviceToDefault() : captureContext.SetActiveDevice(SelectedDeviceName);

            if (hasDevice)
            {
                return true;
            }

            captureContext.SetVolume(Volume);

            Logger.Warning("Selected capture device {deviceName} not found (null means default).", SelectedDeviceName);
            return false;
        }

        public void ReceiveVoice(Span<byte> data)
        {
            if (IsLocalPlayer)
                throw new InvalidOperationException("Cannot receive voice on the local player.");

            if (data.Length == 0)
                return;

            int numElements = opusContext.Decompress(data, DecodeBuffer.Span);

            if (numElements <= 0)
                return;

            unsafe
            {
                fixed (short* decodeButterPtr = &DecodeBuffer.Span.GetPinnableReference())
                {
                    var pcmData = new Span<byte>(decodeButterPtr, numElements * sizeof(short));

                    sound!.SubmitBuffer(pcmData.ToArray());

                    if (sound.State != SoundState.Playing)
                    {
                        sound.Volume = PlayerVolume;
                        sound.Play();
                    }
                }
            }

            timeSinceTalked = 0;
        }
    }
}