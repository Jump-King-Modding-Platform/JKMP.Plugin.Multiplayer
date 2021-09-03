using System.IO;

namespace Matchmaking.Client.Serializing
{
    internal interface IBinarySerializable
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}