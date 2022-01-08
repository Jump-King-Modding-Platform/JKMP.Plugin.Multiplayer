using System.Threading.Tasks;
using Matchmaking.Client.Messages.Processing;

namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class DisconnectedMessageHandler : IMessageHandler<Disconnected, Context>
    {
        public Task HandleMessage(Disconnected message, Context context)
        {
            context.P2PManager.Disconnect(message.Sender);
            return Task.CompletedTask;
        }
    }
}