using System;
using System.IO;
using JKMP.Plugin.Multiplayer.Game;
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
        public Content.SurfaceType SurfaceType { get; set; }
        public bool WearingShoes { get; set; } // todo: when skin syncing is implemented this will be unnecessary 

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)State);
            writer.Write(Position);
            writer.Write(WalkDirection);
            writer.Write((byte)SurfaceType);
            writer.Write(WearingShoes);
        }

        public override void Deserialize(BinaryReader reader)
        {
            State = (PlayerState)reader.ReadByte();
            Position = reader.ReadVector2();
            WalkDirection = reader.ReadSByte();
            SurfaceType = (Content.SurfaceType)reader.ReadByte();
            WearingShoes = reader.ReadBoolean();
        }

        public override void Reset()
        {
            State = default;
            Position = default;
            WalkDirection = default;
            SurfaceType = default;
            WearingShoes = default;
        }
    }
}