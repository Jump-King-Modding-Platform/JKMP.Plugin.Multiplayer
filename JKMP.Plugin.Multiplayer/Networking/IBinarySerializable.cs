using System.IO;

namespace JKMP.Plugin.Multiplayer.Networking
{
    internal interface IBinarySerializable
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}