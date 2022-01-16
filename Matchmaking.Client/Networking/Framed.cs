using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Matchmaking.Client.Networking
{
    /// <summary>
    /// Somewhat of a reimplementation of the tokio_util Framed/Codec interface 
    /// </summary>
    internal class Framed<TStream, TCodec, TData> where TStream : Stream where TCodec : CodecSink<TData>
    {
        public TStream Stream { get; }
        public TCodec Codec { get; }

        private readonly byte[] recvBuffer = new byte[4096];
        private readonly byte[] sendBuffer = new byte[4096];
        
        public Framed(TStream stream, TCodec sink)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            Codec = sink ?? throw new ArgumentNullException(nameof(sink));
        }

        public async Task<TData?> Next(CancellationToken cancellationToken = default)
        {
            if (!Stream.CanRead)
                return default;

            Tuple<bool, ulong> msgLengthTuple = await ReadMessageLength(Stream);

            if (!msgLengthTuple.Item1)
                return default;

            ulong messageLength = msgLengthTuple.Item2;
            
            byte varIntLength = EncodingUtility.GetVarIntLength(messageLength);

            if (messageLength > int.MaxValue || (int)messageLength > recvBuffer.Length)
                throw new InvalidDataException($"Message too large (length = {msgLengthTuple})");

            if (!await ReadBytes(Stream, (int)messageLength, recvBuffer))
                return default;
            
            // tfw no span
            byte[] bytes = new byte[messageLength + varIntLength];
            
            // Write message length to first n bytes
            using (var writer = new BinaryWriter(new MemoryStream(bytes, true)))
            {
                writer.WriteVarInt(messageLength);
            }

            Array.Copy(recvBuffer, 0, bytes, varIntLength, (int)messageLength);

            using var memoryStream = new MemoryStream(bytes);
            using var reader = new BinaryReader(memoryStream);
                
            try
            {
                return Codec.Decode(reader);
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Decoding incoming message failed.\nBytes: [{BytesToString(bytes)}]", ex);
            }
        }

        private async Task<Tuple<bool, ulong>> ReadMessageLength(TStream stream)
        {
            Tuple<bool, ulong> Success(ulong val) => new(true, val);
            Tuple<bool, ulong> Fail() => new(false, default);

            byte[] discriminatorBytes = new byte[1];
            if (!await ReadBytes(stream, 1, discriminatorBytes))
                return Fail();

            var varIntLength = EncodingUtility.GetVarIntLength(discriminatorBytes[0]);

            switch (varIntLength)
            {
                case 1: // byte
                    return Success(discriminatorBytes[0]);
                case 3: // ushort
                {
                    byte[] bytes = new byte[2];

                    if (!await ReadBytes(stream, 2, bytes))
                        return Fail();

                    return Success(BitConverter.ToUInt16(bytes, 0));
                }
                case 5: // uint
                {
                    byte[] bytes = new byte[4];

                    if (!await ReadBytes(stream, 4, bytes))
                        return Fail();

                    return Success(BitConverter.ToUInt32(bytes, 0));
                }
                case 9: // ulong
                {
                    byte[] bytes = new byte[8];

                    if (!await ReadBytes(stream, 8, bytes))
                        return Fail();

                    return Success(BitConverter.ToUInt64(bytes, 0));
                }
                default: throw new NotImplementedException();
            }
        }

        private async Task<bool> ReadBytes(TStream stream, int length, byte[] buffer)
        {
            int numBytes = 0;

            while (numBytes < length)
            {
                int read;

                try
                {
                    read = await stream.ReadAsync(buffer, numBytes, length - numBytes);
                }
                catch (ObjectDisposedException) // Socket was closed
                {
                    return false;
                }
                catch (TaskCanceledException)
                {
                    return false;
                }

                if (read == 0)
                    return false;

                numBytes += read;
            }

            return true;
        }

        private string BytesToString(byte[] bytes)
        {
            StringBuilder builder = new();

            for (int i = 0; i < bytes.Length; ++i)
            {
                builder.Append("0x" + bytes[i].ToString("X2"));

                if (i < bytes.Length - 1)
                    builder.Append(", ");
            }

            return builder.ToString();
        }

        public async Task<bool> Send(TData data)
        {
            if (!Stream.CanWrite)
                return false;

            try
            {
                using var memoryStream = new MemoryStream(sendBuffer, true);
                var writer = new BinaryWriter(memoryStream);
                Codec.Encode(data, writer);

                byte[] newBytes = new byte[memoryStream.Position];
                Array.Copy(sendBuffer, newBytes, (int)memoryStream.Position);
                
                await Stream.WriteAsync(sendBuffer, 0, (int)memoryStream.Position);
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
    }

    internal abstract class CodecSink<T>
    {
        public abstract void Encode(T data, BinaryWriter writer);
        public abstract T Decode(BinaryReader reader);
    }
}