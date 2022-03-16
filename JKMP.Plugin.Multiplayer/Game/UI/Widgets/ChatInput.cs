using System;
using JKMP.Core.Input;
using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.Entities;
using JKMP.Plugin.Multiplayer.Matchmaking;
using JKMP.Plugin.Multiplayer.Networking;
using JKMP.Plugin.Multiplayer.Networking.Messages;
using Matchmaking.Client.Chat;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using Myra.Utility;
using Serilog;
using Steamworks;

namespace JKMP.Plugin.Multiplayer.Game.UI.Widgets
{
    public class ChatInput : ResourceWidget<ChatInput>
    {
        public ChatChannel Channel
        {
            get => channel;
            set
            {
                channel = value;
                channelLabel.Text = value.ToString();
            }
        }
        
        internal Chat? Chat;

        private ChatChannel channel;

        private readonly Label channelLabel;
        private readonly TextBox inputText;
        private readonly TextButton sendButton;

        private static readonly ILogger Logger = LogManager.CreateLogger<ChatInput>();

        /// <summary>The maximum length of the input text</summary>
        private const int MaxInputLength = 100;

        public ChatInput() : base("UI/Chat/ChatInput.xmmp")
        {
            channelLabel = EnsureWidgetById<Label>("Channel");
            inputText = EnsureWidgetById<TextBox>("InputText");
            sendButton = EnsureWidgetById<TextButton>("SendButton");

            sendButton.Click += OnSendClicked;
            inputText.KeyDown += OnInputKeyDown;

            Channel = ChatChannel.Global;
        }

        public override void UpdateLayout()
        {
            base.UpdateLayout();

            Chat ??= this.FindParentOfType<Chat>() ?? throw new InvalidOperationException("Parent chat widget not found");
        }

        private void OnSendClicked(object sender, EventArgs e)
        {
            SendAndClearInput();
            InputManager.EnableGameInput();
            UIManager.PopShowCursor();
            Chat!.HideBackground();
        }

        private void OnInputKeyDown(object sender, GenericEventArgs<Keys> args)
        {
            if (InputKeys.NextChatChannel.IsPressed || InputKeys.PrevChatChannel.IsPressed)
            {
                int direction = 1;

                if (InputKeys.PrevChatChannel.IsPressed)
                {
                    direction = -1;
                }

                ChatChannel newChannel = Channel;

                if (newChannel + direction < 0)
                {
                    newChannel = ChatChannel.Count - 1;
                }
                else if (newChannel + direction >= ChatChannel.Count)
                {
                    newChannel = 0;
                }
                else
                {
                    newChannel += direction;
                }

                Channel = newChannel;
            }
            else if (InputKeys.SelectGlobalChat.IsPressed)
            {
                Channel = ChatChannel.Global;
            }
            else if (InputKeys.SelectGroupChat.IsPressed)
            {
                Channel = ChatChannel.Group;
            }
            else if (InputKeys.SelectLocalChat.IsPressed)
            {
                Channel = ChatChannel.Local;
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
                switch (Channel)
                {
                    case ChatChannel.Global:
                    case ChatChannel.Group:
                    {
                        MatchmakingManager.Instance.SendChatMessage(message, Channel);
                        break;
                    }
                    case ChatChannel.Local:
                    {
                        P2PManager.Instance?.Broadcast(new LocalChatMessage
                        {
                            Message = message
                        });
                        Chat!.AddMessage(SteamClient.Name, SteamClient.SteamId.Value, message, channel);
                        break;
                    }
                    default:
                    {
                        Logger.Warning("Did not send message to channel {channel} due to missing implementation", Channel);
                        break;
                    }
                }
            }
        }

        public void ClearInput(bool clearFocus = true)
        {
            if (clearFocus)
            {
                // Unfocus input text widget
                inputText.Desktop.FocusedKeyboardWidget = null;
            }

            inputText.Text = string.Empty;
        }
    }
}