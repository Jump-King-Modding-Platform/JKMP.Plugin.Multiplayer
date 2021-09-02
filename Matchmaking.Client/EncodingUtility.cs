using System;
using System.IO;
using System.Text;

namespace Matchmaking.Client
{
    public static class EncodingUtility
    {
        private const byte SingleByteMax = 250;
        private const byte U16Byte = 251;
        private const byte U32Byte = 252;
        private const byte U64Byte = 253;
        
        public static void WriteVarInt(this BinaryWriter writer, ulong value)
        {
            if (value <= SingleByteMax)
            {
                writer.Write((byte)value);
            }
            else if (value <= ushort.MaxValue)
            {
                writer.Write(U16Byte);
                writer.Write((ushort)value);
            }
            else if (value <= uint.MaxValue)
            {
                writer.Write(U32Byte);
                writer.Write((uint)value);
            }
            else
            {
                writer.Write(U64Byte);
                writer.Write(value);
            }
        }

        public static ulong ReadVarInt(this BinaryReader reader)
        {
            byte discriminant = reader.ReadByte();

            return discriminant switch
            {
                >= 0 and <= SingleByteMax => discriminant,
                U16Byte => reader.ReadUInt16(),
                U32Byte => reader.ReadUInt32(),
                U64Byte => reader.ReadUInt64(),
                _ => throw new ArgumentOutOfRangeException(nameof(reader), $"Invalid discriminant = {discriminant}")
            };
        }

        /// <summary>
        /// Returns the number of bytes required to represent the given value.
        /// </summary>
        public static byte GetVarIntLength(ulong value)
        {
            return value switch
            {
                >= 0 and <= SingleByteMax => 1,
                
                // sizeof(ushort/uint/ulong) + 1 for discriminator byte
                <= ushort.MaxValue => 3,
                <= uint.MaxValue => 5,
                <= ulong.MaxValue => 9
            };
        }

        public static void WriteUtf8(this BinaryWriter writer, string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var bytes = Encoding.UTF8.GetBytes(value);
            writer.WriteVarInt((ulong)bytes.Length);
            writer.Write(bytes);
        }

        public static string ReadUtf8(this BinaryReader reader)
        {
            var length = reader.ReadVarInt();

            if (length > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(reader), "Length > Int32.MaxValue");

            var bytes = reader.ReadBytes((int)length);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}