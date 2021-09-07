using System.Net.Sockets;
using Matchmaking.Client.Networking;

namespace Matchmaking.Client.Messages.Handlers
{
    internal struct Context
    {
        public readonly Framed<NetworkStream, MessagesCodec, Message> Messages;
        public readonly MatchmakingClient MatchmakingClient;

        public Context(Framed<NetworkStream,MessagesCodec,Message> messages, MatchmakingClient matchmakingClient)
        {
            Messages = messages;
            MatchmakingClient = matchmakingClient;
        }
    }
}