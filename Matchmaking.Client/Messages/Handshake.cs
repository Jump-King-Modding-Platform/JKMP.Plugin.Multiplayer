using System;
using System.IO;

namespace Matchmaking.Client.Messages
{
    internal class HandshakeRequest : Message
    {
        public byte[]? AuthSessionTicket { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (AuthSessionTicket == null)
                throw new InvalidOperationException("AuthSessionTicket is null");

            writer.WriteVarInt((ulong)AuthSessionTicket.Length);
            writer.Write(AuthSessionTicket);
        }

        public override void Deserialize(BinaryReader reader)
        {
            throw new NotImplementedException();
        }
    }

    internal class HandshakeResponse : Message
    {
        public bool Success { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public override void Deserialize(BinaryReader reader)
        {
            Success = reader.ReadBoolean();
        }
    }
}