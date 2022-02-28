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
        [Flags]
        public enum DeltaFlags : ushort
        {
            None = 0,
            State = 1 << 0,
            Position = 1 << 1,
            WalkDirection = 1 << 2,
            SurfaceType = 1 << 3,
            WearingShoes = 1 << 4,
            All = ushort.MaxValue
        }
        
        public DeltaFlags Delta { get; set; }
        public PlayerState State { get; set; }
        public Vector2 Position { get; set; }
        public sbyte WalkDirection { get; set; } // -1 for left, 1 for right
        public Content.SurfaceType SurfaceType { get; set; }
        public bool WearingShoes { get; set; } // todo: when skin syncing is implemented this will be unnecessary

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write((ushort)Delta);

            if ((Delta & DeltaFlags.State) != 0)
                writer.Write((byte)State);

            if ((Delta & DeltaFlags.Position) != 0)
                writer.Write(Position);

            if ((Delta & DeltaFlags.WalkDirection) != 0)
                writer.Write(WalkDirection);

            if ((Delta & DeltaFlags.SurfaceType) != 0)
                writer.Write((byte)SurfaceType);

            if ((Delta & DeltaFlags.WearingShoes) != 0)
                writer.Write(WearingShoes);
        }

        public override void Deserialize(BinaryReader reader)
        {
            Delta = (DeltaFlags)reader.ReadUInt16();
            
            if ((Delta & DeltaFlags.State) != 0)
                State = (PlayerState)reader.ReadByte();
            
            if ((Delta & DeltaFlags.Position) != 0)
                Position = reader.ReadVector2();
            
            if ((Delta & DeltaFlags.WalkDirection) != 0)
                WalkDirection = reader.ReadSByte();
            
            if ((Delta & DeltaFlags.SurfaceType) != 0)
                SurfaceType = (Content.SurfaceType)reader.ReadByte();
            
            if ((Delta & DeltaFlags.WearingShoes) != 0)
                WearingShoes = reader.ReadBoolean();
        }

        public void CalculateDelta(PlayerStateChanged? previous)
        {
            if (previous == null)
            {
                Delta = DeltaFlags.All;
                return;
            }
            
            Delta = DeltaFlags.None;
            
            if (previous.State != State)
                Delta |= DeltaFlags.State;

            if ((previous.Position - Position).LengthSquared() > 0.01f)
                Delta |= DeltaFlags.Position;
            
            if (previous.WalkDirection != WalkDirection)
                Delta |= DeltaFlags.WalkDirection;
            
            if (previous.SurfaceType != SurfaceType)
                Delta |= DeltaFlags.SurfaceType;

            if (previous.WearingShoes != WearingShoes)
                Delta |= DeltaFlags.WearingShoes;
        }
        
        /// <summary>
        /// Sets the previous state's values to the this one if the delta flag is not set.
        /// </summary>
        public void MergeByDelta(PlayerStateChanged? previous)
        {
            if (previous == null)
                return;

            if ((Delta & DeltaFlags.State) == 0)
                State = previous.State;

            if ((Delta & DeltaFlags.Position) == 0)
                Position = previous.Position;

            if ((Delta & DeltaFlags.WalkDirection) == 0)
                WalkDirection = previous.WalkDirection;

            if ((Delta & DeltaFlags.SurfaceType) == 0)
                SurfaceType = previous.SurfaceType;

            if ((Delta & DeltaFlags.WearingShoes) == 0)
                WearingShoes = previous.WearingShoes;
        }

        public override void Reset()
        {
            Delta = default;
            State = default;
            Position = default;
            WalkDirection = default;
            SurfaceType = default;
            WearingShoes = default;
        }
    }
}