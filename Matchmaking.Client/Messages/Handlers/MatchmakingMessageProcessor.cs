using Matchmaking.Client.Messages.Processing;

namespace Matchmaking.Client.Messages.Handlers
{
    internal class MatchmakingMessageProcessor : MessageProcessor<Message, Context>
    {
        public MatchmakingMessageProcessor()
        {
            RegisterHandler(new InformNearbyClientsHandler());
            RegisterHandler(new OutgoingChatMessageHandler());
        }
    }
}