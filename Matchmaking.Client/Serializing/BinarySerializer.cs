using System.IO;
using System.Text;

namespace Matchmaking.Client.Serializing
{
    internal static class BinarySerializer
    {
        public class Options
        {
            public Encoding Encoding { get; }
            
            public Options(Encoding encoding)
            {
                Encoding = encoding;
            }
        }

        public static byte[] Serialize<T>(IBinarySerializable<T> serializable) => Serialize(serializable, GetDefaultOptions());
        public static byte[] Serialize<T>(IBinarySerializable<T> serializable, Options options)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, options.Encoding);
            serializable.Serialize(writer);
            return stream.ToArray();
        }

        public static T Deserialize<T>(byte[] bytes) where T : IBinarySerializable<T>, new() => Deserialize<T>(bytes, GetDefaultOptions());
        public static T Deserialize<T>(byte[] bytes, Options options) where T : IBinarySerializable<T>, new()
        {
            using var reader = new BinaryReader(new MemoryStream(bytes), options.Encoding);
            var instance = new T();
            instance.Deserialize(reader);
            return instance;
        }

        private static Options GetDefaultOptions()
        {
            return new Options(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
        }
    }
}