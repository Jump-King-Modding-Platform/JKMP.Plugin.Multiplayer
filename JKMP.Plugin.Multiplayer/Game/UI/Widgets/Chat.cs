using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Input;
using JKMP.Plugin.Multiplayer.Matchmaking;
using Matchmaking.Client.Chat;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Game.UI.Widgets
{
    public class Chat : ResourceWidget<Chat>
    {
        private readonly ScrollViewer outputScrollViewer;
        private readonly VerticalStackPanel chatOutput;
        private readonly ChatInput chatInput;

        private const int MaxChatMessages = 100;

        private static readonly ILogger Logger = LogManager.CreateLogger<Chat>();
        
        public Chat() : base("UI/Chat/Chat.xmmp")
        {
            outputScrollViewer = EnsureWidgetById<ScrollViewer>("OutputScrollViewer");
            chatOutput = EnsureWidgetById<VerticalStackPanel>("ChatOutputPanel");
            chatInput = EnsureWidgetById<ChatInput>("ChatInput");

            MatchmakingManager.Instance.Events.ChatMessageReceived += OnChatMessageReceived;
        }

        private void OnChatMessageReceived(ChatMessage chatMessage)
        {
            AddMessage(chatMessage.SenderName, chatMessage.Message, chatMessage.Channel);
        }

        public void AddMessage(string? senderName, string message, ChatChannel channel)
        {
            while (chatOutput.Widgets.Count >= MaxChatMessages)
            {
                chatOutput.Widgets.RemoveAt(0);
            }
            
            chatOutput.AddChild(new ChatMessageRow(senderName, message, channel));
        }

        internal void Update(float delta)
        {
            // Toggle chat focus
            if (InputManager.KeyJustPressed(Keys.Enter))
            {
                if (chatInput.IsInputFocused())
                {
                    chatInput.SendAndClearInput();
                    InputManager.EnableGameInput();
                }
                else
                {
                    chatInput.FocusInput();
                    InputManager.DisableGameInput();
                }
            }
        }
    }
}