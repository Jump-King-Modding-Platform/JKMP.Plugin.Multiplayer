using System;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Input;
using JKMP.Plugin.Multiplayer.Matchmaking;
using Matchmaking.Client.Chat;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using Myra.Utility;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Game.UI.Widgets
{
    public class ChatInput : ResourceWidget<ChatInput>
    {
        internal Chat? Chat;
        
        private readonly TextBox inputText;
        private readonly TextButton sendButton;

        private static readonly ILogger Logger = LogManager.CreateLogger<ChatInput>();

        public ChatInput() : base("UI/Chat/ChatInput.xmmp")
        {
            inputText = EnsureWidgetById<TextBox>("InputText");
            sendButton = EnsureWidgetById<TextButton>("SendButton");

            sendButton.Click += OnSendClicked;
        }

        public override void UpdateLayout()
        {
            base.UpdateLayout();
            Chat = this.FindParentOfType<Chat>() ?? throw new InvalidOperationException("Parent chat widget not found");
        }

        private void OnSendClicked(object sender, EventArgs e)
        {
            SendAndClearInput();
        }

        public void FocusInput()
        {
            inputText.SetKeyboardFocus();
        }

        public bool IsInputFocused()
        {
            return inputText.IsKeyboardFocused;
        }

        public void SendAndClearInput()
        {
            // Unfocus input text widget
            inputText.Desktop.FocusedKeyboardWidget = null;
                
            // Send message if any
            string message = inputText.Text.Trim();
            inputText.Text = string.Empty;

            if (message.Length > 0)
            {
                MatchmakingManager.Instance.SendChatMessage(message, ChatChannel.Global);
            }
        }
    }
}