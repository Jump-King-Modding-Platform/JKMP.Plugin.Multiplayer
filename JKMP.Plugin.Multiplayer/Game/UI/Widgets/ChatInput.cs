using Myra.Graphics2D.UI;
using Resources;

namespace JKMP.Plugin.Multiplayer.Game.UI.Widgets
{
    public class ChatInput : Panel
    {
        private readonly TextBox inputText;
        private readonly TextButton sendButton;

        public ChatInput()
        {
            HorizontalStackPanel panel = AddChild(ResourceManager.GetResourceWidget<HorizontalStackPanel, object>("UI/ChatInput.xmmp", UIManager.TypeResolver));
            inputText = (TextBox)panel.EnsureWidgetById("InputText");
            sendButton = (TextButton)panel.EnsureWidgetById("SendButton");
        }
    }
}