using System;
using Matchmaking.Client.Chat;
using Myra.Graphics2D.UI;

namespace JKMP.Plugin.Multiplayer.Game.UI.Widgets
{
    public class ChatMessageRow : ResourceWidget<ChatMessageRow>
    {
        public string? SenderName
        {
            get => nameLabel.Text;
            set
            {
                nameLabel.Text = value;
                messageSeparator.Visible = !string.IsNullOrEmpty(value);
                nameLabel.Visible = !string.IsNullOrEmpty(value);
            }
        }

        public string Message
        {
            get => messageLabel.Text;
            set => messageLabel.Text = value ?? throw new ArgumentNullException(nameof(value));
        }
        
        public ChatChannel Channel { get; set; }

        private readonly Label nameLabel;
        private readonly Label messageLabel;
        private readonly Widget messageSeparator;

        public ChatMessageRow(string? senderName, string message, ChatChannel channel) : base("UI/Chat/ChatMessage.xmmp")
        {
            nameLabel = EnsureWidgetById<Label>("Name");
            messageLabel = EnsureWidgetById<Label>("Message");
            messageSeparator = EnsureWidgetById<Widget>("MessageSeparator");

            SenderName = senderName;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Channel = channel;
        }
    }
}