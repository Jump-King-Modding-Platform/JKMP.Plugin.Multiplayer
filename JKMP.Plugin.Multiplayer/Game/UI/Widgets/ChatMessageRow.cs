using System;
using System.Collections.Generic;
using System.Globalization;
using FontStashSharp;
using JKMP.Core.Logging;
using Matchmaking.Client.Chat;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Game.UI.Widgets
{
    public class ChatMessageRow : ResourceWidget<ChatMessageRow>
    {
        private static readonly IBrush HiddenBackground = new SolidBrush(Color.Transparent);
        private static readonly ILogger Logger = LogManager.CreateLogger<ChatMessageRow>();

        private static readonly Dictionary<ChatChannel, Color> ChannelColors = new()
        {
            { ChatChannel.Global, new Color(245, 245, 245) },
            { ChatChannel.Group, new Color(57, 219, 122) },
            { ChatChannel.Local, new Color(200, 200, 200) }
        };

        private const float TimeUntilFade = 30f;
        private const float FadeOutTime = 0.5f;
        
        public ulong SenderId { get; set; }

        public string? SenderName
        {
            get => nameLabel.Text;
            set
            {
                nameLabel.Text = value;
                nameLabel.Visible = !string.IsNullOrEmpty(value);
            }
        }

        public string Message
        {
            get => messageLabel.Text;
            set
            {
                messageLabel.Text = value ?? throw new ArgumentNullException(nameof(value));
                timeSinceEdit = 0;
                hiddenBackgroundOpacity = 1;
                Opacity = 1;
            }
        }

        public ChatChannel Channel
        {
            get => channel;
            set
            {
                channel = value;
                channelLabel.Text = value.ToString();

                var textColor = ChannelColors[value];
                nameLabel.TextColor = textColor;
                channelLabel.TextColor = textColor;
                timeLabel.TextColor = textColor;
            }
        }

        private readonly Label nameLabel;
        private readonly Label timeLabel;
        private readonly Label channelLabel;
        private readonly Label messageLabel;
        private readonly IBrush originalRootBackground;
        private readonly DateTime creationDate;
        private ChatChannel channel;
        private float timeSinceEdit;
        private float hiddenBackgroundOpacity;

        public ChatMessageRow(string? senderName, ulong senderId, string message, ChatChannel channel) : base("UI/Chat/ChatMessage.xmmp")
        {
            nameLabel = EnsureWidgetById<Label>("Name");
            timeLabel = EnsureWidgetById<Label>("Time");
            channelLabel = EnsureWidgetById<Label>("Channel");
            messageLabel = EnsureWidgetById<Label>("Message");
            originalRootBackground = Root.Background;

            SenderName = senderName;
            SenderId = senderId;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Channel = channel;
            creationDate = DateTime.Now;

            timeLabel.Text = creationDate.ToString("t", CultureInfo.InstalledUICulture);
        }

        public void ShowBackground()
        {
            Root.Background = originalRootBackground;
        }

        public void HideBackground()
        {
            Root.Background = HiddenBackground;
        }

        public bool CanMerge(ChatChannel channel, ulong? senderId)
        {
            if (channel != Channel)
                return false;

            if (senderId != SenderId)
                return false;

            if (DateTime.Now.Minute != creationDate.Minute)
                return false;

            return true;
        }

        internal void Update(float delta)
        {
            timeSinceEdit += delta;
            
            if (timeSinceEdit >= TimeUntilFade)
            {
                hiddenBackgroundOpacity = MathHelper.Clamp(hiddenBackgroundOpacity - (delta / FadeOutTime), 0, 1);
            }

            if (Root.Background == originalRootBackground)
            {
                Opacity = hiddenBackgroundOpacity;
            }
            else
            {
                Opacity = 1;
            }
        }
    }
}