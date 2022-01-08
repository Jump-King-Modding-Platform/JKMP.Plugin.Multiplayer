using System;
using System.ComponentModel;

namespace Matchmaking.Client.Chat
{
    public class ChatMessage
    {
        public ChatChannel Channel { get; set; }
        public ulong? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string Message { get; set; }

        public ChatMessage(ChatChannel channel, ulong? senderId, string? senderName, string message)
        {
            if (!Enum.IsDefined(typeof(ChatChannel), channel))
                throw new InvalidEnumArgumentException(nameof(channel), (int)channel, typeof(ChatChannel));
            
            Channel = channel;
            SenderId = senderId;
            SenderName = senderName;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}