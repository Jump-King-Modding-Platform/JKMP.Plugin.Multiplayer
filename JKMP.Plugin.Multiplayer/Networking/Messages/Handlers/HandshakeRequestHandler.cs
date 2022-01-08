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
        
        public Task HandleMessage(HandshakeRequest message, Context context)
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
                return Task.CompletedTask;
            }

            var plrListener = EntityManager.instance.Find<GameEntity>().PlayerListener;
            var playerState = new PlayerStateChanged
            {
                Position = plrListener.Position,
                State = plrListener.CurrentState,
                WalkDirection = (sbyte)plrListener.WalkDirection
            };

            context.Messages.Send(message.Sender, new HandshakeResponse
            {
                Success = true,
                PlayerState = playerState
            });

            return Task.CompletedTask;
        }
    }
}