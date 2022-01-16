using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JKMP.Core.Logging;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking
{
    /// <summary>
    /// Somewhat of a reimplementation of the tokio_util Framed/Codec interface that works with steamworks p2p.
    /// </summary>
    internal class Framed<TCodec, TData> : IDisposable where TCodec : CodecSink<TData>, new()
    {
        private bool isRunning = true;

        private TaskCompletionSource<bool>? tcs = new();
        private readonly Queue<TData> queuedMessages = new();
        private readonly TCodec codec;
        private readonly byte[] sendBuffer = new byte[4096];
        
        private static readonly ILogger Logger = LogManager.CreateLogger<Framed<TCodec, TData>>();

        public Framed(TCodec codec)
        {
            this.codec = codec;
            Task.Run(StartPolling);
        }

        public void Dispose()
        {
            isRunning = false;
        }

        private async Task StartPolling()
        {
            try
            {
                while (isRunning)
                {
                    while (SteamNetworking.ReadP2PPacket() is {} packet)
                    {
                        using var memoryStream = new MemoryStream(packet.Data);
                        using var reader = new BinaryReader(memoryStream);
                        TData message;

                        try
                        {
                            message = codec.Decode(packet.SteamId, reader);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "An unhandled exception was raised when decoding message from {steamId}", packet.SteamId);
                            continue;
                        }
                        
                        queuedMessages.Enqueue(message);
                        tcs?.TrySetResult(true);
                    }

                    await Task.Delay(33).ConfigureAwait(false); // poll around 30 times per second
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unhandled exception was raised in the polling thread");
                throw;
            }
        }

        public async Task<TData?> Next()
        {
            if (!isRunning)
                return default;

            if (tcs != null)
            {
                await tcs.Task;
                tcs = null;
            }

            TData result = queuedMessages.Dequeue();

            if (queuedMessages.Count == 0)
            {
                tcs = new();
            }
            
            return result;
        }

        /// <summary>
        /// Sends a message to the target steam id. Returns true if the message was successfully sent.
        /// It does not indicate whether the message was received on the other end.
        /// </summary>
        public bool Send(SteamId steamId, TData data, P2PSend sendType = P2PSend.Reliable, int channel = 0)
        {
            return Send(steamId, Encode(data), sendType, channel);
        }

        /// <summary>
        /// <para>
        /// Sends an array of bytes to the target steam id. Returns true if the message was successfully sent.
        /// It does not indicate whether the message was received on the other end.
        /// </para>
        /// <para>
        /// Note that the bytes are sent as-is, no prefix like message type or length is added.</para>
        /// </summary>
        public bool Send(SteamId steamId, byte[] data, P2PSend sendType = P2PSend.Reliable, int channel = 0)
        {
            return SteamNetworking.SendP2PPacket(steamId, data, sendType: sendType, nChannel: channel);
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