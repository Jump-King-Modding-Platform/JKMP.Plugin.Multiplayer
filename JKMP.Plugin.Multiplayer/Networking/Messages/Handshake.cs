using System;
using System.IO;
using JKMP.Plugin.Multiplayer.Memory;
using Matchmaking.Client;

namespace JKMP.Plugin.Multiplayer.Networking.Messages
{
    internal class HandshakeRequest : GameMessage
    {
        public byte[]? AuthSessionTicket { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (AuthSessionTicket == null)
                throw new InvalidOperationException("AuthSessionTicket is null");

            writer.WriteVarInt((ulong)AuthSessionTicket.Length);
            writer.Write(AuthSessionTicket);
        }

        public override void Deserialize(BinaryReader reader)
        {
            ulong length = reader.ReadVarInt();
            AuthSessionTicket = reader.ReadBytes((int)length);
        }

        public override void Reset()
        {
            AuthSessionTicket = null;
        }
    }

    internal class HandshakeResponse : GameMessage
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        
        public PlayerStateChanged? PlayerState { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (!Success)
            {
                if (ErrorMessage == null)
                    throw new InvalidOperationException("ErrorMessage can only be null if Success is true");
            }
            else
            {
                if (PlayerState == null)
                    throw new InvalidOperationException("PlayerState can only be null if Success is false");
            }
            
            writer.Write(Success);

            if (!Success)
            {
                writer.Write(ErrorMessage!);
            }
            else
            {
                PlayerState!.Serialize(writer);
            }
        }

        public override void Deserialize(BinaryReader reader)
        {
            Success = reader.ReadBoolean();

            if (!Success)
            {
                ErrorMessage = reader.ReadString();
            }
            else
            {
                PlayerState = Pool.Get<PlayerStateChanged>();
                PlayerState.Deserialize(reader);
            }
        }

        public override void Reset()
        {
            Success = default;
            ErrorMessage = default;

            if (PlayerState != default)
            {
                Pool.Release(PlayerState);
                PlayerState = default;
            }
        }
    }
}