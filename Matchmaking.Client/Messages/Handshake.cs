using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Matchmaking.Client.Messages
{
    internal class HandshakeRequest : Message
    {
        public byte[]? AuthSessionTicket { get; set; }
        public string? Name { get; set; }
        public string? MatchmakingPassword { get; set; }
        public Vector2? Position { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (AuthSessionTicket == null)
                throw new InvalidOperationException("AuthSessionTicket is null");

            if (Name == null)
                throw new InvalidOperationException("Name is null");

            if (MatchmakingPassword == null)
                throw new InvalidOperationException("MatchmakingPassword is null");

            if (Position == null)
                throw new InvalidOperationException("Position is null");

            writer.WriteVarInt((ulong)AuthSessionTicket.Length);
            writer.Write(AuthSessionTicket);
            writer.WriteUtf8(Name);
            writer.Write(MatchmakingPassword);
            writer.Write(Position.Value);
        }

        public override void Deserialize(BinaryReader reader)
        {
            throw new NotSupportedException();
        }
    }

    internal class HandshakeResponse : Message
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            throw new NotSupportedException();
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