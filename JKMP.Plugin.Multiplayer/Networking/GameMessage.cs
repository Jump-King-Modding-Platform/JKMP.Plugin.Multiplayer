using System.IO;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    internal abstract class GameMessage : IBinarySerializable
    {
        public abstract void Serialize(BinaryWriter writer);
        public abstract void Deserialize(BinaryReader reader);
    }
}