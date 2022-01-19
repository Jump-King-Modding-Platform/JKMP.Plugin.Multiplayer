using System.Threading.Tasks;
using Matchmaking.Client.Messages.Processing;

namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class DisconnectedMessageHandler : IMessageHandler<Disconnected, Context>
    {
        public Task HandleMessage(Disconnected message, Context context)
        {
            return context.P2PManager.ExecuteOnGameThread(() =>
            {
                context.P2PManager.Disconnect(context.Messages.Identity);
            });
        }
    }
}