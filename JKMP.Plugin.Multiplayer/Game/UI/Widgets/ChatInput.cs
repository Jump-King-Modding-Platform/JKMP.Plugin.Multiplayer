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

        /// <summary>The maximum length of the input text</summary>
        private const int MaxInputLength = 100;

        public ChatInput() : base("UI/Chat/ChatInput.xmmp")
        {
            inputText = EnsureWidgetById<TextBox>("InputText");
            sendButton = EnsureWidgetById<TextButton>("SendButton");

            sendButton.Click += OnSendClicked;
            inputText.TextChangedByUser += OnTextChangedByUser;
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

        private void OnTextChangedByUser(object sender, ValueChangedEventArgs<string> e)
        {
            if (e.NewValue.Length > MaxInputLength)
            {
                var cursorPosition = inputText.CursorPosition;
                inputText.Text = e.NewValue.Substring(0, MaxInputLength);
                
                // Cursor position is reset to 0 when text is changed so we need to restore it
                inputText.CursorPosition = cursorPosition;
            }
        }

        public void FocusInput()
        {
            inputText.SetKeyboardFocus();
        }

        public bool IsInputFocused()
        {
            return inputText.IsKeyboardFocused;
        }

        public void SendAndClearInput(bool clearFocus = true)
        {
            if (clearFocus)
            {
                // Unfocus input text widget
                inputText.Desktop.FocusedKeyboardWidget = null;
            }

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