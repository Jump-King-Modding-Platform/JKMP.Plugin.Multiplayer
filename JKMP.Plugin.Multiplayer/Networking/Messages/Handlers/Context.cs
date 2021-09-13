namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class Context
    {
        public readonly Framed<GameMessagesCodec, GameMessage> Messages;
        public readonly P2PManager P2PManager;

        public Context(Framed<GameMessagesCodec, GameMessage> messages, P2PManager p2pManager)
        {
            Messages = messages;
            P2PManager = p2pManager;
        }
    }
}