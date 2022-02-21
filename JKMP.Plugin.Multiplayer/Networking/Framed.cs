using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Memory;
using Serilog;
using Steamworks;
using Steamworks.Data;

namespace JKMP.Plugin.Multiplayer.Networking
{
    /// <summary>
    /// Handles the sending and receiving of <see cref="GameMessage"/>s to and from a peer.
    /// </summary>
    internal class Framed<TCodec> : IDisposable where TCodec : CodecSink<GameMessage>, new()
    {
        /// <summary>
        /// Gets the identity of the connection this framed is associated with.
        /// </summary>
        public NetIdentity Identity => identity;
        
        private Connection connection;

        private readonly TCodec codec;
        private readonly NetIdentity identity;
        private readonly P2PManager.PeerManager peerManager;
        private readonly P2PManager p2pManager;
        private readonly byte[] sendBuffer = new byte[8192];

        private static readonly ILogger Logger = LogManager.CreateLogger<Framed<TCodec>>();

        public Framed(TCodec codec, Connection connection, NetIdentity identity, P2PManager.PeerManager peerManager, P2PManager p2pManager)
        {
            this.codec = codec;
            this.connection = connection;
            this.identity = identity;
            this.peerManager = peerManager;
            this.p2pManager = p2pManager;

            peerManager.IncomingMessage += OnIncomingMessage;
        }

        private void OnIncomingMessage(Connection conn, NetIdentity ident, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            if (conn != connection || ident.SteamId != identity.SteamId)
                return;
            
            unsafe
            {
                using UnmanagedMemoryStream stream = new((byte*)data, size);
                using BinaryReader reader = new(stream);

                try
                {
                    var message = codec.Decode(reader);
                    p2pManager.AddPendingMessage(ident, message);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error decoding message");
                    throw;
                }
            }
        }

        public void Dispose()
        {
            peerManager.IncomingMessage -= OnIncomingMessage;
        }

        /// <summary>
        /// Sends a message to the target steam id. Returns true if the message was successfully sent.
        /// It does not indicate whether the message was received on the other end.
        /// </summary>
        public bool Send(GameMessage data, SendType sendType = SendType.Reliable) => Send(Encode(data), sendType);

        /// <summary>
        /// <para>
        /// Sends an array of bytes to the target steam id. Returns true if the message was successfully sent.
        /// It does not indicate whether the message was received on the other end.
        /// </para>
        /// <para>
        /// Note that the bytes are sent as-is, no prefix like message type or length is added.</para>
        /// </summary>
        public unsafe bool Send(ReadOnlySpan<byte> data, SendType sendType = SendType.Reliable)
        {
            fixed (byte* dataPtr = &data.GetPinnableReference())
            {
                return connection.SendMessage((IntPtr)dataPtr, data.Length, sendType, laneIndex: 0) == Result.OK;
            }
        }

        /// <summary>
        /// Encodes the data and returns a span to the bytes.
        /// Note that the span is only safe to use until the next call to Encode or <see cref="Send(TData,Steamworks.Data.SendType)"/> so don't store it or use it after the next call to Encode.
        /// </summary>
        public ReadOnlySpan<byte> Encode(GameMessage data)
        {
            lock (sendBuffer)
            {
                using var memoryStream = new MemoryStream(sendBuffer, 0, sendBuffer.Length, true, true);
                var writer = new BinaryWriter(memoryStream);
                codec.Encode(data, writer);

                var bytes = memoryStream.GetReadOnlySpan();
                return bytes;
            }
        }
    }
}