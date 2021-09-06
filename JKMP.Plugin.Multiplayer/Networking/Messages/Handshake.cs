using System;
using System.IO;
using Matchmaking.Client;

namespace JKMP.Plugin.Multiplayer.Networking.Messages
{
    internal class HandshakeRequest : GameMessage
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
            ulong length = reader.ReadVarInt();
            AuthSessionTicket = reader.ReadBytes((int)length);
        }
    }

    internal class HandshakeResponse : GameMessage
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (!Success && ErrorMessage == null)
                throw new InvalidOperationException("ErrorMessage can only be null if Success is false");
            
            writer.Write(Success);

            if (!Success)
            {
                writer.Write(ErrorMessage!);
            }
        }

        public override void Deserialize(BinaryReader reader)
        {
            Success = reader.ReadBoolean();

            if (!Success)
            {
                ErrorMessage = reader.ReadString();
            }
        }
    }
}