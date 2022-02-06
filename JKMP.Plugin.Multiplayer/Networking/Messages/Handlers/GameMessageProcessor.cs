using Matchmaking.Client.Messages.Processing;

namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class GameMessageProcessor : MessageProcessor<GameMessage, Context>
    {
        public GameMessageProcessor()
        {
            RegisterHandler(new HandshakeRequestHandler());
            RegisterHandler(new HandshakeResponseHandler());
            RegisterHandler(new PlayerStateChangedHandler());
            RegisterHandler(new LocalChatMessageHandler());
            RegisterHandler(new DisconnectedMessageHandler());
            RegisterHandler(new VoiceTransmissionHandler());
        }
    }
}