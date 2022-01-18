using System.IO;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    internal abstract class CodecSink<T>
    {
        public abstract void Encode(T data, BinaryWriter writer);
        public abstract T Decode(BinaryReader reader);
    }
}