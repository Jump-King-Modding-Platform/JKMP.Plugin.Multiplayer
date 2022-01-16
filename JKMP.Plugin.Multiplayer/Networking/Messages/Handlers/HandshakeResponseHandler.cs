using System;
using System.Threading.Tasks;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Steam;
using Matchmaking.Client.Messages.Processing;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class HandshakeResponseHandler : IMessageHandler<HandshakeResponse, Context>
    {
        private static readonly ILogger Logger = LogManager.CreateLogger<HandshakeResponseHandler>();
        
        public async Task HandleMessage(HandshakeResponse message, Context context)
        {
            Logger.Verbose("Received handshake response from {steamId}: {success} (error?: {errorMessage})", message.Sender, message.Success, message.ErrorMessage);

            if (message.Success)
            {
                SteamFriends.SetPlayedWith(message.Sender);

                var userInfo = await SteamUtil.GetUserInfo(message.Sender);

                if (!userInfo.HasValue)
                    throw new NotImplementedException("User info is null, this shouldn't happen");
                
                await context.P2PManager.ExecuteOnGameThread(() =>
                {
                    RemotePlayer player = context.P2PManager.ConnectedPlayers[message.Sender];
                    player.InitializeFromHandshakeResponse(message, userInfo.Value);
                });
            }
        }
    }
}