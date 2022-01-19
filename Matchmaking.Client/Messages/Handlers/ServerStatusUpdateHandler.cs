using System.Threading.Tasks;
using Matchmaking.Client.EventData;
using Matchmaking.Client.Messages.Processing;
using Serilog;

namespace Matchmaking.Client.Messages.Handlers
{
    internal class ServerStatusUpdateHandler : IMessageHandler<ServerStatusUpdate, Context>
    {
        private static readonly ILogger Logger = Log.ForContext<ServerStatusUpdateHandler>();
        
        public Task HandleMessage(ServerStatusUpdate message, Context context)
        {
            Logger.Verbose("Received server status. Total players: {totalPlayers}, group players: {groupPlayers}", message.TotalPlayers, message.GroupPlayers);

            context.MatchmakingClient.ExecuteOnMainThread(() =>
            {
                context.MatchmakingClient.Events.OnServerStatusUpdateReceived(new ServerStatus(message.TotalPlayers, message.GroupPlayers));
            });
            
            return Task.CompletedTask;
        }
    }
}