using System;
using System.IO;
using Matchmaking.Client;

namespace JKMP.Plugin.Multiplayer.Networking.Messages
{
    internal class VoiceTransmission : GameMessage
    {
        public byte[]? Data { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (Data == null || Data.Length == 0)
                throw new InvalidOperationException("Data is null or empty");

            writer.WriteVarInt((ulong)Data.Length);
            writer.Write(Data);
        }

        public override void Deserialize(BinaryReader reader)
        {
            Data = new byte[reader.ReadVarInt()];
            reader.Read(Data, 0, Data.Length);
        }
    }
}