using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Matchmaking.Client.Serializing;
using Microsoft.Xna.Framework;

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

        public static void Write(this BinaryWriter writer, Vector2 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        internal static void Write<T>(this BinaryWriter writer, ICollection<T> collection) where T : IBinarySerializable
        {
            writer.WriteVarInt((ulong)collection.Count);

            foreach (T item in collection)
            {
                item.Serialize(writer);
            }
        }

        internal static List<T> ReadList<T>(this BinaryReader reader) where T : IBinarySerializable, new()
        {
            ulong length = reader.ReadVarInt();
            List<T> result = new();

            for (ulong i = 0; i < length; ++i)
            {
                var instance = new T();

                try
                {
                    instance.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    throw new FormatException($"Failed to deserialize item in collection at index {i}/{length}", ex);
                }

                result.Add(instance);
            }

            return result;
        }

        public static void Write(this BinaryWriter writer, ICollection<ulong> collection)
        {
            writer.WriteVarInt((ulong)collection.Count);

            foreach (float item in collection)
            {
                writer.Write(item);
            }
        }

        public static List<ulong> ReadUInt64List(this BinaryReader reader)
        {
            ulong length = reader.ReadVarInt();
            List<ulong> result = new();

            for (ulong i = 0; i < length; ++i)
            {
                try
                {
                    result.Add(reader.ReadVarInt());
                }
                catch (Exception ex)
                {
                    throw new FormatException($"Failed to deserialize ulong in collection at index {i}/{length}", ex);
                }
            }
            
            return result;
        }
    }
}