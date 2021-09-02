using System.IO;

namespace Matchmaking.Client.Serializing
{
    internal interface IBinarySerializable<T>
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}