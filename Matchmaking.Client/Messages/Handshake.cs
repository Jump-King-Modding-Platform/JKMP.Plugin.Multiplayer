using System;
using System.IO;

namespace Matchmaking.Client.Messages
{
    internal class HandshakeRequest : Message
    {
        public byte[]? AuthSessionTicket { get; set; }
        public string? Name { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (AuthSessionTicket == null)
                throw new InvalidOperationException("AuthSessionTicket is null");

            if (Name == null)
                throw new InvalidOperationException("Name is null");

            writer.WriteVarInt((ulong)AuthSessionTicket.Length);
            writer.Write(AuthSessionTicket);
            writer.WriteUtf8(Name);
        }

        public override void Deserialize(BinaryReader reader)
        {
            throw new NotImplementedException();
        }
    }

    internal class HandshakeResponse : Message
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public override void Deserialize(BinaryReader reader)
        {
            Success = reader.ReadBoolean();

            // Ideally we'd be able to save 1 byte here by checking if success == false, but bincode on the server side always writes a bool for Option types
            if (reader.ReadBoolean())
            {
                ErrorMessage = reader.ReadUtf8();
            }
        }
    }
}