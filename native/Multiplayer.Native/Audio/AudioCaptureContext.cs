using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NativeAudioContext = JKMP.Plugin.Multiplayer.Native.AudioContext;

namespace JKMP.Plugin.Multiplayer.Native.AudioCapture
{
    public class AudioCaptureContext : IDisposable
    {
        private readonly NativeAudioContext context;

        private OnDataDelegate? onData;
        private Action<CaptureError>? onError;

        // These delegates need to be stored as long as the context is alive, otherwise they will be garbage collected.
        private readonly OnCapturedDataCallback internalOnData;
        private readonly OnCaptureErrorCallback internalOnError;

        public AudioCaptureContext()
        {
            context = NativeAudioContext.New();
            internalOnData = OnData;
            internalOnError = OnError;
        }

        public ICollection<Audio.DeviceInformation> GetOutputDevices()
        {
            List<Audio.DeviceInformation> result = new List<Audio.DeviceInformation>();
            
            context.GetOutputDevices(slice =>
            {
                foreach (Native.DeviceInformation deviceInfo in slice)
                {
                    unsafe
                    {
                        string name = Encoding.UTF8.GetString((byte*)deviceInfo.name_utf8, deviceInfo.name_len);
                        result.Add(new Audio.DeviceInformation(name, deviceInfo.default_config));
                    }
                }
            });

            return result;
        }

        public bool SetActiveDevice(string deviceName)
        {
            unsafe
            {
                var utf8Bytes = Encoding.UTF8.GetBytes(deviceName);

                fixed (byte* bytesPtr = utf8Bytes)
                {
                    var slice = new Sliceu8((IntPtr)bytesPtr, (ulong)utf8Bytes.Length);

                    try
                    {
                        context.SetActiveDevice(slice);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public bool SetActiveDevice(Audio.DeviceInformation device) => SetActiveDevice(device.Name);

        public bool SetActiveDeviceToDefault()
        {
            try
            {
                context.SetActiveDeviceToDefault();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Audio.DeviceInformation? GetActiveDeviceInfo()
        {
            Audio.DeviceInformation? result = null;

            try
            {
                context.GetActiveDevice((ref Native.DeviceInformation info) =>
                {
                    unsafe
                    {
                        string name = Encoding.UTF8.GetString((byte*)info.name_utf8, info.name_len);
                        result = new Audio.DeviceInformation(name, info.default_config);
                    }
                });

                return result;
            }
            catch (InteropException<MyFFIError> err)
            {
                return null;
            }
        }

        // Spans are not allowed to be used as type arguments, so instead of Func<Span<T>> we use a delegate
        public delegate void OnDataDelegate(in ReadOnlySpan<short> data);

        /// <summary>
        /// Starts capturing audio data. The data in the callback is always mono 16bit PCM regardless
        /// of the number of channels on the input device.
        /// </summary>
        public bool StartCapture(OnDataDelegate onData, Action<CaptureError> onError)
        {
            this.onData = onData;
            this.onError = onError;
            
            try
            {
                context.StartCapture(internalOnData, internalOnError);
                return true;
            }
            catch (InteropException<MyFFIError> err)
            {
                return false;
            }
        }

        private void OnData(Slicei16 slice)
        {
            onData?.Invoke(slice.AsReadOnlySpan());
        }

        private void OnError(CaptureError error)
        {
            context.StopCapture();
            onError?.Invoke(error);
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}