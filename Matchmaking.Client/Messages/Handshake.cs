using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Matchmaking.Client.Messages
{
    internal class HandshakeRequest : Message
    {
        public byte[]? AuthSessionTicket { get; set; }
        /// <summary>
        /// <p>The matchmaking password to use. If set, the player will only matchmake with players that have the same password set.</p>
        /// <p>Can be null.</p>
        /// </summary>
        public string? MatchmakingPassword { get; set; }
        public string? LevelName { get; set; }
        public Vector2? Position { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (AuthSessionTicket == null)
                throw new InvalidOperationException("AuthSessionTicket is null");

            if (LevelName == null)
                throw new InvalidOperationException("LevelName is null");

            if (Position == null)
                throw new InvalidOperationException("Position is null");

            writer.WriteVarInt((ulong)AuthSessionTicket.Length);
            writer.Write(AuthSessionTicket);

            writer.Write(MatchmakingPassword != null);
            if (MatchmakingPassword != null)
                writer.Write(MatchmakingPassword);

            writer.Write(LevelName);
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