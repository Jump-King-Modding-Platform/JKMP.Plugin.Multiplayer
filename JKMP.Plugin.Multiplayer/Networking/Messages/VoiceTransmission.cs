using System;
using System.Collections.Generic;
using System.IO;
using Matchmaking.Client;

namespace JKMP.Plugin.Multiplayer.Networking.Messages
{
    internal class VoiceTransmission : GameMessage
    {
        public Queue<byte[]>? Data { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (Data == null || Data.Count == 0)
                throw new InvalidOperationException("Data is null or empty");

            writer.WriteVarInt((ulong)Data.Count);
            
            foreach (var voiceData in Data)
            {
                writer.WriteVarInt((ulong)voiceData.Length);
                writer.Write(voiceData);
            }
        }

        public override void Deserialize(BinaryReader reader)
        {
            int numPackets = (int)reader.ReadVarInt();

            Data = new Queue<byte[]>(numPackets);

            for (int i = 0; i < numPackets; ++i)
            {
                byte[] data = reader.ReadBytes((int)reader.ReadVarInt());
                Data.Enqueue(data);
            }
        }

        public override void Reset()
        {
            Data = default;
        }
    }
}