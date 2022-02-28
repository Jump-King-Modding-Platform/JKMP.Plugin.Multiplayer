using System.IO;

namespace JKMP.Plugin.Multiplayer.Networking.Messages
{
    internal class Disconnected : GameMessage
    {
        public override void Serialize(BinaryWriter writer)
        {
        }

        public override void Deserialize(BinaryReader reader)
        {
        }

        public override void Reset()
        {
        }
    }
}