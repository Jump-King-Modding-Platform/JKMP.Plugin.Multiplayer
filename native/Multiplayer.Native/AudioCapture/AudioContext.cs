using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NativeAudioContext = JKMP.Plugin.Multiplayer.Native.AudioContext;

namespace JKMP.Plugin.Multiplayer.Native.AudioCapture
{
    public class AudioContext : IDisposable
    {
        private readonly NativeAudioContext context;

        private OnDataDelegate? onData;
        private Action<CaptureError>? onError;

        // These delegates need to be stored as long as the context is alive, otherwise they will be garbage collected.
        private readonly OnCapturedDataCallback internalOnData;
        private readonly OnCaptureErrorCallback internalOnError;

        public AudioContext()
        {
            context = NativeAudioContext.New();
            internalOnData = OnData;
            internalOnError = OnError;
        }

        public ICollection<DeviceInformation> GetOutputDevices()
        {
            List<DeviceInformation> result = new List<DeviceInformation>();
            
            context.GetOutputDevices(slice =>
            {
                foreach (Native.DeviceInformation deviceInfo in slice)
                {
                    unsafe
                    {
                        string name = Encoding.UTF8.GetString((byte*)deviceInfo.name_utf8, deviceInfo.name_len);
                        result.Add(new DeviceInformation(name));
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

        public bool SetActiveDevice(DeviceInformation device) => SetActiveDevice(device.Name);

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

        // Spans are not allowed to be used as type arguments, so instead of Func<Span<T>> we use a delegate
        public delegate void OnDataDelegate(in ReadOnlySpan<short> data);

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
                Console.WriteLine(err.Error);
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