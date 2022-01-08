using System;
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

            int numBytes;

            try
            {
                numBytes = await Stream.ReadAsync(recvBuffer, 0, recvBuffer.Length, cancellationToken);
            }
            catch (ObjectDisposedException) // Socket was closed
            {
                return default;
            }
            catch (TaskCanceledException)
            {
                return default;
            }

            if (numBytes == 0) // EOF/Disconnected
                return default;

            // tfw no span
            byte[] bytes = new byte[numBytes];
            Array.Copy(recvBuffer, bytes, numBytes);

            using var memoryStream = new MemoryStream(bytes);
            using var reader = new BinaryReader(memoryStream);

            // todo: support reading multiple messages in a packet
                
            return Codec.Decode(reader);
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