using System.Threading.Tasks;
using EntityComponent;
using HarmonyLib;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Memory;
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
                var handshakeResponse = Pool.Get<HandshakeResponse>();
                handshakeResponse.Success = false;
                handshakeResponse.ErrorMessage = $"Steam auth result = {authResult}";
                
                context.Messages.Send(handshakeResponse);
                Pool.Release(handshakeResponse);

                await context.P2PManager.ExecuteOnGameThread(() =>
                {
                    context.P2PManager.Disconnect(context.Messages.Identity);
                });
                return;
            }

            PlayerStateChanged playerState = Pool.Get<PlayerStateChanged>();

            await context.P2PManager.ExecuteOnGameThread(() =>
            {
                var plrListener = EntityManager.instance.Find<GameEntity>().PlayerListener;
                playerState.Position = plrListener.Position;
                playerState.State = plrListener.CurrentState;
                playerState.WalkDirection = (sbyte)plrListener.WalkDirection;
            });

            var response = Pool.Get<HandshakeResponse>();
            response.Success = true;
            response.PlayerState = playerState;

            context.Messages.Send(response);
            Pool.Release(response);
            // PlayerState is released by the response
        }
    }
}