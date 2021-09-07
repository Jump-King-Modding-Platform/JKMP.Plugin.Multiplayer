using System.Threading.Tasks;
using Matchmaking.Client.Messages.Processing;

namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class PlayerStateChangedHandler : IMessageHandler<PlayerStateChanged, Context>
    {
        public Task HandleMessage(PlayerStateChanged message, Context context)
        {
            return Task.CompletedTask;
        }
    }
}