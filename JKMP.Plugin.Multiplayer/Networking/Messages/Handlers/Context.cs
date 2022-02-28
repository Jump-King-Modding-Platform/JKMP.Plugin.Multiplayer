namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class Context
    {
        public readonly Framed<GameMessagesCodec> Messages;
        public readonly P2PManager P2PManager;
        public readonly RemotePlayer? Player;

        public Context(Framed<GameMessagesCodec> messages, P2PManager p2pManager, RemotePlayer? player)
        {
            Messages = messages;
            P2PManager = p2pManager;
            Player = player;
        }
    }
}