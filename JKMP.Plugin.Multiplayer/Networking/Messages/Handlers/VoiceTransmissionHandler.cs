using System;
using System.IO;
using System.Threading.Tasks;
using Matchmaking.Client.Messages.Processing;
using StbImageSharp;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class VoiceTransmissionHandler : IMessageHandler<VoiceTransmission, Context>
    {
        private readonly byte[] recvBuffer = new byte[SteamUser.OptimalSampleRate * 5];
        
        public Task HandleMessage(VoiceTransmission message, Context context)
        {
            int numBytes;
            
            unsafe
            {
                fixed (byte* dataBytes = message.Data)
                {
                    fixed (byte* recvBytes = recvBuffer)
                    {
                        numBytes = SteamUser.DecompressVoice((IntPtr)dataBytes, message.Data!.Length, (IntPtr)recvBytes, recvBuffer.Length);
                    }
                }
            }

            if (numBytes <= 0)
                return Task.CompletedTask;

            return context.P2PManager.ExecuteOnGameThread(() =>
            {
                context.Player?.VoiceManager?.ReceiveVoice(new Span<byte>(recvBuffer, 0, numBytes));
            });
        }
    }
}