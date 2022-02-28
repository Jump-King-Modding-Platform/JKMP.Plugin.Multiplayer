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
        private bool lanesConfigured;

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
                    Logger.Error(ex, "Error decoding message from {identity}", ident);
                    conn.Close();
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
        /// By default it sends the message as a reliable message on lane 2.
        /// </summary>
        public bool Send(GameMessage data, SendType sendType = SendType.Reliable, ushort lane = 2) => Send(Encode(data), sendType, lane);

        /// <summary>
        /// <para>
        /// Sends an array of bytes to the target steam id. Returns true if the message was successfully sent.
        /// It does not indicate whether the message was received on the other end.
        /// By default it sends the message as a reliable message on lane 2.
        /// </para>
        /// <para>
        /// Note that the bytes are sent as-is, no prefix like message type or length is added.</para>
        /// </summary>
        public unsafe bool Send(ReadOnlySpan<byte> data, SendType sendType = SendType.Reliable, ushort lane = 2)
        {
            if (!lanesConfigured)
            {
                // Configure 3 lanes.
                // The first lane is for unreliable messages sent on a regular basis, such as player state updates. (uses medium bandwidth)
                // The second lane is unreliable for voice data. (uses most bandwidth, has 2x bandwidth budget over the first lane)
                // The third lane is for everything else that is supposed to be reliable (most prioritized, uses least bandwidth)
                connection.ConfigureConnectionLanes(
                    new[] { 2, 2, 1 },
                    new ushort[] { 1, 2, 1 }
                );

                lanesConfigured = true;
            }

            fixed (byte* dataPtr = &data.GetPinnableReference())
            {
                var sendResult = connection.SendMessage((IntPtr)dataPtr, data.Length, sendType, lane);

                if (sendResult != Result.OK)
                    Logger.Warning("Send result: {result}", sendResult);

                return sendResult == Result.OK;
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