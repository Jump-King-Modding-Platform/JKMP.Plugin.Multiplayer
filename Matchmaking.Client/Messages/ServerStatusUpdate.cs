using System;
using System.IO;

namespace Matchmaking.Client.Messages
{
    internal class ServerStatusUpdate : Message
    {
        public uint TotalPlayers { get; set; }
        public uint GroupPlayers { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            throw new NotSupportedException();
        }

        public override void Deserialize(BinaryReader reader)
        {
            TotalPlayers = (uint)reader.ReadVarInt();
            GroupPlayers = (uint)reader.ReadVarInt();
        }
    }
}