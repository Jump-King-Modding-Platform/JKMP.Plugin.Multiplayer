using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using NativeAudioContext = JKMP.Plugin.Multiplayer.Native.AudioContext;

namespace JKMP.Plugin.Multiplayer.Native.AudioCapture
{
    public class AudioContext : IDisposable
    {
        private readonly NativeAudioContext context;

        public AudioContext()
        {
            context = NativeAudioContext.New();
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

        public void Dispose()
        {
            context.Dispose();
        }
    }
}