using System.Threading.Tasks;
using EntityComponent;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
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
            var authResult = SteamUser.BeginAuthSession(message.AuthSessionTicket, context.Messages.Identity);

            Logger.Verbose("{steamId} auth result: {authResult}", context.Messages.Identity, authResult);

            if (authResult != BeginAuthResult.OK)
            {
                context.Messages.Send(new HandshakeResponse
                {
                    Success = false,
                    ErrorMessage = $"Steam auth result = {authResult}"
                });

                await context.P2PManager.ExecuteOnGameThread(() =>
                {
                    context.P2PManager.Disconnect(context.Messages.Identity);
                });
                return;
            }

            PlayerStateChanged? playerState = null;

            await context.P2PManager.ExecuteOnGameThread(() =>
            {
                var plrListener = EntityManager.instance.Find<GameEntity>().PlayerListener;
                playerState = new PlayerStateChanged
                {
                    Position = plrListener.Position,
                    State = plrListener.CurrentState,
                    WalkDirection = (sbyte)plrListener.WalkDirection
                };
            });
            
            context.Messages.Send(new HandshakeResponse
            {
                Success = true,
                PlayerState = playerState
            });
        }
    }
}