using System.Collections.Generic;
using System.Linq;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Input;
using JKMP.Plugin.Multiplayer.Matchmaking;
using JKMP.Plugin.Multiplayer.Networking;
using JKMP.Plugin.Multiplayer.Steam.Events;
using JumpKing.PauseMenu;
using Matchmaking.Client.Chat;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Serilog;
using Color = Microsoft.Xna.Framework.Color;
using SolidBrush = Myra.Graphics2D.Brushes.SolidBrush;

namespace JKMP.Plugin.Multiplayer.Game.UI.Widgets
{
    public class Chat : ResourceWidget<Chat>
    {
        private readonly ScrollViewer outputScrollViewer;
        private readonly VerticalStackPanel chatOutput;
        private readonly ChatInput chatInput;
        private readonly IBrush originalRootBackground;
        private readonly object accessRowsLock = new();

        private const int MaxChatMessages = 100;

        private static readonly ILogger Logger = LogManager.CreateLogger<Chat>();

        private static readonly IBrush HiddenBackground = new SolidBrush(Color.Transparent);
        
        public Chat() : base("UI/Chat/Chat.xmmp")
        {
            outputScrollViewer = EnsureWidgetById<ScrollViewer>("OutputScrollViewer");
            chatOutput = EnsureWidgetById<VerticalStackPanel>("ChatOutputPanel");
            chatInput = EnsureWidgetById<ChatInput>("ChatInput");
            originalRootBackground = Root.Background;

            MatchmakingManager.Instance.Events.ChatMessageReceived += OnChatMessageReceived;
            P2PManager.Instance!.Events.IncomingChatMessage += OnChatMessageReceived;

            HideBackground();
        }

        private void OnChatMessageReceived(ChatMessage chatMessage)
        {
            AddMessage(chatMessage.SenderName, chatMessage.SenderId, chatMessage.Message, chatMessage.Channel);
        }

        public void AddMessage(string? senderName, ulong? senderId, string message, ChatChannel channel)
        {
            lock (accessRowsLock)
            {
                while (chatOutput.Widgets.Count >= MaxChatMessages)
                {
                    chatOutput.Widgets.RemoveAt(0);
                }

                // Append to previous message if it's the same sender and channel and posted on the same minute
                var previousMessage = (ChatMessageRow?)chatOutput.Widgets.LastOrDefault();

                if (previousMessage != null && previousMessage.CanMerge(channel, senderId))
                {
                    previousMessage.Message += "\n" + message;
                    return;
                }

                var row = new ChatMessageRow(senderName, senderId ?? 0, message, channel);
                chatOutput.AddChild(row);

                if (Root.Background != HiddenBackground)
                    row.HideBackground();
            }
        }
        
        internal void Update(float delta)
        {
            // Toggle chat focus
            if (!PauseManager.instance.IsPaused && InputManager.KeyJustPressed(Keys.Enter))
            {
                // Check if chat input is active
                if (chatInput.Opacity > 0)
                {
                    chatInput.SendAndClearInput();
                    InputManager.EnableGameInput();
                    UIManager.PopShowCursor();
                    HideBackground();
                }
                else
                {
                    chatInput.FocusInput();
                    InputManager.DisableGameInput();
                    UIManager.PushShowCursor();
                    ShowBackground();
                }
            }

            lock (accessRowsLock)
            {
                foreach (var row in GetMessageRows())
                {
                    row.Update(delta);
                }
            }
        }

        internal void HideBackground()
        {
            Root.Background = HiddenBackground;
            chatInput.Opacity = 0;
            chatInput.Enabled = false;
            outputScrollViewer.ShowVerticalScrollBar = false;
            ToggleChatRowBackgrounds(true);
        }

        internal void ShowBackground()
        {
            Root.Background = originalRootBackground;
            chatInput.Opacity = 1;
            chatInput.Enabled = true;
            outputScrollViewer.ShowVerticalScrollBar = true;
            ToggleChatRowBackgrounds(false);
        }

        private void ToggleChatRowBackgrounds(bool visible)
        {
            var rows = GetMessageRows();

            foreach (ChatMessageRow row in rows)
            {
                if (visible)
                    row.ShowBackground();
                else
                    row.HideBackground();
            }
        }

        private IEnumerable<ChatMessageRow> GetMessageRows()
        {
            return ((StackPanel)outputScrollViewer.Content).Widgets.Cast<ChatMessageRow>();
        }
    }
}