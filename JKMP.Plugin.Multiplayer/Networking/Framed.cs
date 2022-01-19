using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using JKMP.Core.Logging;
using Serilog;
using Steamworks;
using Steamworks.Data;

namespace JKMP.Plugin.Multiplayer.Networking
{
    /// <summary>
    /// Somewhat of a reimplementation of the tokio_util Framed/Codec interface that works with steamworks networking sockets.
    /// </summary>
    internal class Framed<TCodec, TData> : IDisposable where TCodec : CodecSink<TData>, new()
    {
        /// <summary>
        /// Gets the identity of the connection this framed is associated with.
        /// </summary>
        public NetIdentity Identity => identity;

        public bool HasQueuedMessages => queuedMessages.Count > 0;
        
        private Connection connection;
        private TaskCompletionSource<bool>? tcs = new();

        private readonly TCodec codec;
        private readonly NetIdentity identity;
        private readonly P2PManager.PeerManager peerManager;
        private readonly byte[] sendBuffer = new byte[4096];
        private readonly byte[] recvBuffer = new byte[4096];
        private readonly Queue<TData> queuedMessages = new();

        private static readonly ILogger Logger = LogManager.CreateLogger<Framed<TCodec, TData>>();

        public Framed(TCodec codec, Connection connection, NetIdentity identity, P2PManager.PeerManager peerManager)
        {
            this.codec = codec;
            this.connection = connection;
            this.identity = identity;
            this.peerManager = peerManager;

            peerManager.IncomingMessage += OnIncomingMessage;
        }

        private void OnIncomingMessage(Connection conn, NetIdentity ident, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            if (conn != connection || ident.SteamId != identity.SteamId)
                return;

            if (size > recvBuffer.Length)
            {
                Logger.Warning("Received message of size {messageSize} which is larger than the buffer size of {bufferSize}", size, recvBuffer.Length);
                return;
            }

            Marshal.Copy(data, recvBuffer, 0, size);
            using var reader = new BinaryReader(new MemoryStream(recvBuffer, 0, size));

            try
            {
                var message = codec.Decode(reader);

                queuedMessages.Enqueue(message);
                tcs?.TrySetResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public void Dispose()
        {
            peerManager.IncomingMessage -= OnIncomingMessage;
            tcs?.SetResult(false);
        }

        public async Task<TData?> Next()
        {
            if (tcs != null)
            {
                if (!await tcs.Task)
                    return default;

                tcs = null;
            }

            var data = queuedMessages.Dequeue();

            if (queuedMessages.Count == 0)
                tcs = new();

            return data;
        }

        /// <summary>
        /// Sends a message to the target steam id. Returns true if the message was successfully sent.
        /// It does not indicate whether the message was received on the other end.
        /// </summary>
        public bool Send(TData data, SendType sendType = SendType.Reliable) => Send(Encode(data), sendType);

        /// <summary>
        /// <para>
        /// Sends an array of bytes to the target steam id. Returns true if the message was successfully sent.
        /// It does not indicate whether the message was received on the other end.
        /// </para>
        /// <para>
        /// Note that the bytes are sent as-is, no prefix like message type or length is added.</para>
        /// </summary>
        public bool Send(byte[] data, SendType sendType = SendType.Reliable)
        {
            return connection.SendMessage(data, sendType, laneIndex: 0) == Result.OK;
        }

        /// <summary>
        /// Encodes the data and returns the bytes.
        /// </summary>
        public byte[] Encode(TData data)
        {
            lock (sendBuffer)
            {
                using var memoryStream = new MemoryStream(sendBuffer, true);
                var writer = new BinaryWriter(memoryStream);
                codec.Encode(data, writer);

                byte[] bytes = new byte[(int)memoryStream.Position];
                Array.Copy(sendBuffer, bytes, bytes.Length);
                return bytes;
            }
        }
    }
}