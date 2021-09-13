using System.IO;
using JKMP.Plugin.Multiplayer.Game.Player;
using Matchmaking.Client;
using Microsoft.Xna.Framework;

namespace JKMP.Plugin.Multiplayer.Networking.Messages
{
    internal class PlayerStateChanged : GameMessage
    {
        public PlayerState State { get; set; }
        public Vector2 Position { get; set; }
        public sbyte WalkDirection { get; set; } // -1 for left, 1 for right
        
        public override void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)State);
            writer.Write(Position);
            writer.Write(WalkDirection);
        }

        public override void Deserialize(BinaryReader reader)
        {
            State = (PlayerState)reader.ReadByte();
            Position = reader.ReadVector2();
            WalkDirection = reader.ReadSByte();
        }
    }
}