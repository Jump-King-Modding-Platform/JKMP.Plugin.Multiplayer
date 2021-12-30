using Matchmaking.Client.Chat;

namespace JKMP.Plugin.Multiplayer.Networking
{
    public class P2PEvents
    {
        #region IncomingChatMessage

        public delegate void IncomingChatMessageEventHandler(ChatMessage message);

        public event IncomingChatMessageEventHandler? IncomingChatMessage;
        
        internal void OnIncomingChatMessage(ChatMessage message)
        {
            IncomingChatMessage?.Invoke(message);
        }

        #endregion
    }
}