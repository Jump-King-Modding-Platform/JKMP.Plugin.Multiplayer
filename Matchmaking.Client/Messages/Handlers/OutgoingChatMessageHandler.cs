using System.Threading.Tasks;
using Matchmaking.Client.Chat;
using Matchmaking.Client.Messages.Processing;
using Serilog;

namespace Matchmaking.Client.Messages.Handlers
{
    internal class OutgoingChatMessageHandler : IMessageHandler<OutgoingChatMessage, Context>
    {
        private static readonly ILogger Logger = Log.ForContext<OutgoingChatMessageHandler>();
        
        public Task HandleMessage(OutgoingChatMessage message, Context context)
        {
            Logger.Information("[{channel}, {senderId}] {senderName}: {message}", message.Channel, message.SenderSteamId, message.SenderName, message.Message);
            context.MatchmakingClient.Events.OnChatMessageReceived(new ChatMessage(message.Channel, message.SenderSteamId, message.SenderName, message.Message!));

            return Task.CompletedTask;
        }
    }
}