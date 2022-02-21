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
            Logger.Verbose("Received handshake response from {identity}: {success} (error?: {errorMessage})", context.Messages.Identity, message.Success, message.ErrorMessage);

            if (message.Success)
            {
                if (context.Messages.Identity.IsSteamId)
                    SteamFriends.SetPlayedWith(context.Messages.Identity);

                var userInfo = await SteamUtil.GetUserInfo(context.Messages.Identity);

                if (!userInfo.HasValue)
                    throw new NotImplementedException("User info is null, this shouldn't happen");
                
                await context.P2PManager.ExecuteOnGameThread(() =>
                {
                    var player = new RemotePlayer(context.Messages.Identity);
                    player.InitializeFromHandshakeResponse(message, userInfo.Value);
                    context.P2PManager.ConnectedPlayers.Add(context.Messages.Identity, player);
                });
            }
        }
    }
}