using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Matchmaking.Client.Messages
{
    internal class PositionUpdate : Message
    {
        public Vector2? Position { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (Position == null)
                throw new InvalidOperationException("Position is null");

            writer.Write(Position.Value);
        }

        public override void Deserialize(BinaryReader reader)
        {
            Position = reader.ReadVector2();
        }
    }
}