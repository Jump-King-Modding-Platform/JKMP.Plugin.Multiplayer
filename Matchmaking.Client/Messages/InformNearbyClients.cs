using System;
using System.Collections.Generic;
using System.IO;

namespace Matchmaking.Client.Messages
{
    internal class InformNearbyClients : Message
    {
        public List<ulong>? ClientIds { get; set; }

        public override void Serialize(BinaryWriter writer)
        {
            throw new NotSupportedException();
        }

        public override void Deserialize(BinaryReader reader)
        {
            ClientIds = reader.ReadUInt64List();
        }
    }
}