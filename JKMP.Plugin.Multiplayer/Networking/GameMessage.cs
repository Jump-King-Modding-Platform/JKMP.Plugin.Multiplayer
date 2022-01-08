using System.IO;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    internal abstract class GameMessage : IBinarySerializable
    {
        /// <summary>
        /// Gets the steam id of the player that sent the message.
        /// </summary>
        public SteamId Sender { get; internal set; }

        public abstract void Serialize(BinaryWriter writer);
        public abstract void Deserialize(BinaryReader reader);
    }
}