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
            { MessageType.HandshakeResponse, typeof(HandshakeResponse) },
            { MessageType.PositionUpdate, typeof(PositionUpdate) },
            { MessageType.SetMatchmakingPassword, typeof(SetMatchmakingPassword) },
            { MessageType.InformNearbyClients, typeof(InformNearbyClients) },
            { MessageType.IncomingChatMessage, typeof(IncomingChatMessage) },
            { MessageType.OutgoingChatMessage, typeof(OutgoingChatMessage) }
        };

        private static readonly Dictionary<Type, MessageType> MessageTypesReversed = MessageTypes.ToDictionary(kv => kv.Value, kv => kv.Key);

        public override void Encode(Message data, BinaryWriter writer)
        {
            byte[] bytes = BinarySerializer.Serialize(data);

            ulong messageType = (ulong)MessageTypesReversed[data.GetType()];
            writer.WriteVarInt((ulong)(bytes.Length + EncodingUtility.GetVarIntLength(messageType)));
            writer.WriteVarInt(messageType);
            writer.Write(bytes);
        }

        public override Message Decode(BinaryReader reader)
        {
            ulong length = reader.ReadVarInt();
            ulong available = (ulong)(reader.BaseStream.Length - reader.BaseStream.Position);
            
            if (length > available)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"Length ({length} > Available ({available})");
            }
            
            MessageType messageType = (MessageType)reader.ReadVarInt();

            if (!MessageTypes.TryGetValue(messageType, out var clrType))
            {
                throw new NotSupportedException($"Received message is not supported: {messageType}");
            }

            Message message = (Message)Activator.CreateInstance(clrType);
            message.Deserialize(reader);

            available = (ulong)(reader.BaseStream.Length - reader.BaseStream.Position);
            
            if (available > 0)
                throw new FormatException($"Deserialized message did not consume the full length of the message (remaining bytes: {available})");

            return message;
        }
    }
}