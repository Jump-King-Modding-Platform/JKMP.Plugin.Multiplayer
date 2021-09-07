using System.Threading.Tasks;
using Matchmaking.Client.Messages.Processing;

namespace Matchmaking.Client.Messages.Handlers
{
    internal class InformNearbyClientsHandler : IMessageHandler<InformNearbyClients, Context>
    {
        public Task HandleMessage(InformNearbyClients message, Context context)
        {
            context.MatchmakingClient.Events.OnNearbyClientsReceived(message.ClientIds!);
            return Task.CompletedTask;
        }
    }
}