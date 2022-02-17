using System;
using JKMP.Plugin.Multiplayer.Game.Components;
using JKMP.Plugin.Multiplayer.Native;
using JKMP.Plugin.Multiplayer.Native.Audio;
using Microsoft.Xna.Framework.Audio;
using OpusContext = JKMP.Plugin.Multiplayer.Native.Audio.OpusContext;

namespace JKMP.Plugin.Multiplayer.Game.Sound
{
    public class VoicePlayback : IDisposable
    {
        public bool IsCapturing => captureContext.IsCapturing;
        
        private readonly AudioCaptureContext captureContext;
        private readonly OpusContext opusContext;
        private readonly DynamicSoundEffectInstance sound;

        private readonly Memory<short> uncompressedBuffer = new short[1024];
        private readonly Memory<byte> compressedBuffer = new byte[1024];

        public VoicePlayback()
        {
            captureContext = new ();
            opusContext = new(48000);
            sound = new(48000, AudioChannels.Mono);

            VoiceManager.VolumeChanged += OnVolumeChanged;
        }

        public void Dispose()
        {
            captureContext.Dispose();
            opusContext.Dispose();
            sound.Dispose();
            VoiceManager.VolumeChanged -= OnVolumeChanged;
        }

        private void OnVolumeChanged(double volume)
        {
            captureContext.SetVolume(volume);
        }

        public bool StartPlayback()
        {
            if (captureContext.IsCapturing)
                return false;
            
            bool hasDevice = VoiceManager.SelectedDeviceName == null ? captureContext.SetActiveDeviceToDefault() : captureContext.SetActiveDevice(VoiceManager.SelectedDeviceName);

            if (!hasDevice)
                return false;

            captureContext.SetVolume(VoiceManager.Volume);
            return captureContext.StartCapture(OnVoiceData, OnVoiceError);
        }

        private void OnVoiceData(ReadOnlySpan<short> data)
        {
            var numBytes = opusContext.Compress(data, compressedBuffer.Span);

            if (numBytes <= 0)
                return;

            var compressedBytes = compressedBuffer.Span.Slice(0, numBytes);
            numBytes = opusContext.Decompress(compressedBytes, uncompressedBuffer.Span);

            if (numBytes <= 0)
                return;

            var decompressedShorts = uncompressedBuffer.Span.Slice(0, numBytes);

            unsafe
            {
                fixed (short* ptr = &decompressedShorts.GetPinnableReference())
                {
                    var decompressedBytes = new Span<byte>(ptr, numBytes * sizeof(short));
                    sound.SubmitBuffer(decompressedBytes.ToArray());

                    if (sound.State != SoundState.Playing)
                        sound.Play();
                }
            }
        }

        private void OnVoiceError(CaptureError error)
        {
            captureContext.StopCapture();
            sound.Stop();
        }

        public void StopPlayback()
        {
            captureContext.StopCapture();
            sound.Stop();
        }
    }
}