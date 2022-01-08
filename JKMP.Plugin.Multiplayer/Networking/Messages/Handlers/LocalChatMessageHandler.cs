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
                Logger.Warning("Received empty chat message from {senderId}", message.Sender);
            }

            var userInfo = await SteamUtil.GetUserInfo(message.Sender);
            string senderName;

            if (userInfo == null)
            {
                Logger.Error("Failed to retrieve user info for steam id {senderId}", message.Sender);
                senderName = message.Sender.ToString();
            }
            else
            {
                senderName = userInfo.Value.Name;
            }
            
            Logger.Information("[{channel}, {senderId}] {senderName}: {message}", ChatChannel.Local, message.Sender, senderName, trimmedMessage);

            context.P2PManager.Events.OnIncomingChatMessage(new ChatMessage(ChatChannel.Local, message.Sender.Value, senderName, trimmedMessage));
        }
    }
}