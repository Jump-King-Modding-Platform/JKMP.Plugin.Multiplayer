using System;

namespace JKMP.Plugin.Multiplayer.Native.Audio
{
    public class DeviceInformation : IEquatable<DeviceInformation>
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

        public bool Equals(DeviceInformation other)
        {
            return Name == other.Name && Config.Equals(other.Config);
        }

        public override bool Equals(object? obj)
        {
            return obj is DeviceInformation other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode() * 397) ^ Config.GetHashCode();
            }
        }
    }
    
    public class DeviceConfig : IEquatable<DeviceConfig>
    {
        public uint SampleRate { get; }
        public ushort Channels { get; }

        public DeviceConfig(uint sampleRate, ushort channels)
        {
            SampleRate = sampleRate;
            Channels = channels;
        }

        public bool Equals(DeviceConfig other)
        {
            return SampleRate == other.SampleRate && Channels == other.Channels;
        }

        public override bool Equals(object? obj)
        {
            return obj is DeviceConfig other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)SampleRate * 397) ^ Channels.GetHashCode();
            }
        }
    }
}