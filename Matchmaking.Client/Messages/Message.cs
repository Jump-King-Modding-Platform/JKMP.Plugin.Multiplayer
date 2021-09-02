using System.IO;
using Matchmaking.Client.Networking;
using Matchmaking.Client.Serializing;

namespace Matchmaking.Client.Messages
{
    internal abstract class Message : IBinarySerializable<Message>
    {
        public abstract void Serialize(BinaryWriter writer);
        public abstract void Deserialize(BinaryReader reader);
    }
}