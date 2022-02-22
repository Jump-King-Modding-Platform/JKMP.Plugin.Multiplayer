using System;
using System.IO;

namespace JKMP.Plugin.Multiplayer.Networking.Messages
{
    internal class LocalChatMessage : GameMessage
    {
        public string? Message { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (Message == null)
                throw new InvalidOperationException("Message is null");

            writer.Write(Message);
        }

        public override void Deserialize(BinaryReader reader)
        {
            Message = reader.ReadString();
        }

        public override void Reset()
        {
            Message = default;
        }
    }
}