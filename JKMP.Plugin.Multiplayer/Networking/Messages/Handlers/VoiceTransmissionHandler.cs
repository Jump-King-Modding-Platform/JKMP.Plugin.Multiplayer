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
            if (message.Data!.Count == 0)
                return Task.CompletedTask;

            return context.P2PManager.ExecuteOnGameThread(() =>
            {
                foreach (byte[]? voicePacket in message.Data)
                {
                    context.Player?.VoiceManager?.ReceiveVoice(voicePacket.AsSpan());
                }
                
                //context.Player?.VoiceManager?.ReceiveVoice(message.Data.AsSpan());
            });
        }
    }
}