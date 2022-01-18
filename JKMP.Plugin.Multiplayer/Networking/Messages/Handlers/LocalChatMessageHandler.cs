using System;
using System.Threading.Tasks;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Steam;
using JKMP.Plugin.Multiplayer.Steam.Events;
using Matchmaking.Client.Chat;
using Matchmaking.Client.Messages.Processing;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Networking.Messages.Handlers
{
    internal class LocalChatMessageHandler : IMessageHandler<LocalChatMessage, Context>
    {
        private static readonly ILogger Logger = LogManager.CreateLogger<LocalChatMessageHandler>();
        
        public async Task HandleMessage(LocalChatMessage message, Context context)
        {
            string trimmedMessage = message.Message?.Trim() ?? String.Empty;

            if (trimmedMessage.Length > 100)
                trimmedMessage = trimmedMessage.Substring(0, 100);

            if (trimmedMessage.Length <= 0)
            {
                Logger.Warning("Received empty chat message from {senderId}", context.Messages.Identity);
            }

            Friend? userInfo = null;

            if (context.Messages.Identity.IsSteamId)
                userInfo = await SteamUtil.GetUserInfo(context.Messages.Identity);
            
            string senderName;

            if (userInfo == null)
            {
                Logger.Error("Failed to retrieve user info for identity {senderId}", context.Messages.Identity);
                senderName = context.Messages.Identity.ToString();
            }
            else
            {
                senderName = userInfo.Value.Name;
            }
            
            Logger.Information("[{channel}, {senderId}] {senderName}: {message}", ChatChannel.Local, context.Messages.Identity, senderName, trimmedMessage);

            await context.P2PManager.ExecuteOnGameThread(() =>
            {
                context.P2PManager.Events.OnIncomingChatMessage(new ChatMessage(ChatChannel.Local, context.Messages.Identity.SteamId, senderName, trimmedMessage));
            });
        }
    }
}