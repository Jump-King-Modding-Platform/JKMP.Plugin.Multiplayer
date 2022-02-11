namespace JKMP.Plugin.Multiplayer.Native.Audio
{
    public class DeviceInformation
    {
        public string Name { get; }
        public DeviceConfig Config { get; }

        public DeviceInformation(string name, DeviceConfig config)
        {
            Name = name;
            Config = config;
        }

        public DeviceInformation(string name, Native.DeviceConfig config) : this(name, new DeviceConfig(config.sample_rate, config.channels))
        {
        }
    }
    
    public struct DeviceConfig
    {
        public uint SampleRate { get; }
        public ushort Channels { get; }

        public DeviceConfig(uint sampleRate, ushort channels)
        {
            SampleRate = sampleRate;
            Channels = channels;
        }
    }
}