using System.Threading.Tasks;
using JKMP.Core.Logging;
using Matchmaking.Client.Messages.Processing;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class PlayerStateChangedHandler : IMessageHandler<PlayerStateChanged, Context>
    {
        private static readonly ILogger Logger = LogManager.CreateLogger<PlayerStateChangedHandler>();
        
        public Task HandleMessage(PlayerStateChanged message, Context context)
        {
            context.P2PManager.ExecuteOnGameThread(() =>
            {
                using var guard = context.P2PManager.ConnectedPlayersMtx.Lock();

                if (!guard.Value.TryGetValue(message.Sender, out var player))
                    return;

                player.UpdateFromState(message);
            });
            
            return Task.CompletedTask;
        }
    }
}