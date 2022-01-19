using System.Threading.Tasks;
using JKMP.Core.Logging;
using Matchmaking.Client.Messages.Processing;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class PlayerStateChangedHandler : IMessageHandler<PlayerStateChanged, Context>
    {
        private static readonly ILogger Logger = LogManager.CreateLogger<PlayerStateChangedHandler>();
        
        public async Task HandleMessage(PlayerStateChanged message, Context context)
        {
            await context.P2PManager.ExecuteOnGameThread(() =>
            {
                if (!context.P2PManager.ConnectedPlayers.TryGetValue(context.Messages.Identity, out var player))
                    return;

                if (player.State != PlayerNetworkState.Connected)
                    return;

                player.UpdateFromState(message);
            });
        }
    }
}