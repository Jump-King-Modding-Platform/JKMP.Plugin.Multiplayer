using System;
using System.IO;
using Matchmaking.Client.Chat;

namespace Matchmaking.Client.Messages
{
    internal class IncomingChatMessage : Message
    {
        public ChatChannel Channel { get; set; }
        public string? Message { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (Message == null)
                throw new InvalidOperationException("Message is null");

            writer.WriteVarInt((ulong)Channel);
            writer.WriteUtf8(Message);
        }

        public override void Deserialize(BinaryReader reader)
        {
            throw new NotSupportedException();
        }
    }

    internal class OutgoingChatMessage : Message
    {
        public ChatChannel Channel { get; set; }
        public ulong? SenderSteamId { get; set; }
        public string? SenderName { get; set; }
        public string? Message { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            throw new NotSupportedException();
        }

        public override void Deserialize(BinaryReader reader)
        {
            Channel = (ChatChannel)reader.ReadVarInt();

            if (reader.ReadBoolean())
                SenderSteamId = reader.ReadVarInt();

            if (reader.ReadBoolean())
                SenderName = reader.ReadUtf8();
                    
            Message = reader.ReadUtf8();
        }
    }
}