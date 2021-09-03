using System;
using System.IO;

namespace Matchmaking.Client.Messages
{
    internal class SetMatchmakingPassword : Message
    {
        public string? Password { get; set; }
        
        public override void Serialize(BinaryWriter writer)
        {
            if (Password == null)
                throw new InvalidOperationException("Password is null");

            writer.WriteUtf8(Password);
        }

        public override void Deserialize(BinaryReader reader)
        {
            throw new NotSupportedException();
        }
    }
}