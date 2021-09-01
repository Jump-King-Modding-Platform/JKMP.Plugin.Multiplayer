using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Matchmaking.Client.Messages;
using Matchmaking.Client.Serializing;

namespace Matchmaking.Client.Networking
{
    internal class MessagesCodec : CodecSink<Message>
    {
        private static readonly Dictionary<MessageType, Type> MessageTypes = new()
        {
            { MessageType.HandshakeRequest, typeof(HandshakeRequest) },
            { MessageType.HandshakeResponse, typeof(HandshakeResponse) }
        };

        private static readonly Dictionary<Type, MessageType> MessageTypesReversed = MessageTypes.ToDictionary(kv => kv.Value, kv => kv.Key);

        public override async Task Encode(Message data, BinaryWriter writer)
        {
            byte[] bytes = BinarySerializer.Serialize(data);

            writer.Write((uint)(bytes.Length + 4));
            writer.Write((uint)MessageTypesReversed[data.GetType()]);
            writer.Write(bytes);
        }

        public override async Task<Message> Decode(BinaryReader reader)
        {
            uint length = reader.ReadUInt32();
            MessageType messageType = (MessageType)reader.ReadUInt32();

            if (!MessageTypes.TryGetValue(messageType, out var clrType))
            {
                throw new NotSupportedException($"Received message is not supported: {messageType}");
            }

            Message message = (Message)Activator.CreateInstance(clrType);
            message.Deserialize(reader);

            return message;
        }
    }
}