using System.IO;
using JKMP.Plugin.Multiplayer.Memory;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    internal abstract class GameMessage : IBinarySerializable, IPoolable
    {
        public abstract void Serialize(BinaryWriter writer);
        public abstract void Deserialize(BinaryReader reader);
        public abstract void Reset();
        public virtual void OnSpawned()
        {
        }
    }
}