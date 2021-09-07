namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class Context
    {
        public RemotePlayer Player;
        public Framed<GameMessagesCodec, GameMessage> Messages;
        public P2PManager P2PManager;

        public Context(RemotePlayer player, Framed<GameMessagesCodec, GameMessage> messages, P2PManager p2pManager)
        {
            Player = player;
            Messages = messages;
            P2PManager = p2pManager;
        }
    }
}