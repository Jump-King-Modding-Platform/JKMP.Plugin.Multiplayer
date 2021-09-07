using System.Threading.Tasks;
using JKMP.Core.Logging;
using Matchmaking.Client.Messages.Processing;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class HandshakeRequestHandler : IMessageHandler<HandshakeRequest, Context>
    {
        private static readonly ILogger Logger = LogManager.CreateLogger<HandshakeRequestHandler>();
        
        public async Task HandleMessage(HandshakeRequest message, Context context)
        {
            var authResult = SteamUser.BeginAuthSession(message.AuthSessionTicket, message.Sender);

            Logger.Verbose("{steamId} auth result: {authResult}", message.Sender, authResult);

            if (authResult != BeginAuthResult.OK)
            {
                context.Messages.Send(message.Sender, new HandshakeResponse
                {
                    Success = false,
                    ErrorMessage = $"Steam auth result = {authResult}"
                });

                context.P2PManager.Disconnect(message.Sender);
                return;
            }

            context.Messages.Send(message.Sender, new HandshakeResponse
            {
                Success = true
            });
        }
    }
}