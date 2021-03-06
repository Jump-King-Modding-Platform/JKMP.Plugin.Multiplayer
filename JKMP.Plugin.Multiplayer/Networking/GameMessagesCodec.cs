using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JKMP.Plugin.Multiplayer.Memory;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using Matchmaking.Client;

namespace JKMP.Plugin.Multiplayer.Networking
{
    internal class GameMessagesCodec : CodecSink<GameMessage>
    {
        private static readonly Dictionary<MessageType, Type> MessageTypes = new()
        {
            { MessageType.HandshakeRequest, typeof(HandshakeRequest) },
            { MessageType.HandshakeResponse, typeof(HandshakeResponse) },
            { MessageType.PlayerStateChanged, typeof(PlayerStateChanged) },
            { MessageType.LocalChatMessage, typeof(LocalChatMessage) },
            { MessageType.Disconnected, typeof(Disconnected) },
            { MessageType.VoiceTransmission, typeof(VoiceTransmission) },
        };

        private static readonly Dictionary<Type, MessageType> MessageTypesReversed = MessageTypes.ToDictionary(kv => kv.Value, kv => kv.Key);
        
        public override void Encode(GameMessage data, BinaryWriter writer)
        {
            MessageType messageType = GetMessageType(data);
            writer.WriteVarInt((ulong)messageType);
            data.Serialize(writer);
        }

        public override GameMessage Decode(BinaryReader reader)
        {
            var messageType = (MessageType)reader.ReadVarInt();
            Type? clrType = GetMessageType(messageType);

            if (clrType == null)
                throw new FormatException($"Unknown message type received");

            GameMessage message = Pool.Get<GameMessage>(clrType);
            message.Deserialize(reader);

            ulong available = (ulong)(reader.BaseStream.Position - reader.BaseStream.Length);

            if (available > 0)
                throw new FormatException($"Deserialized message did not consume the full length of the message (remaining bytes: {available}");

            return message;
        }

        private MessageType GetMessageType(GameMessage data)
        {
            return MessageTypesReversed[data.GetType()];
        }

        private Type? GetMessageType(MessageType type)
        {
            if (MessageTypes.TryGetValue(type, out var messageType))
                return messageType;

            return null;
        }
    }
}