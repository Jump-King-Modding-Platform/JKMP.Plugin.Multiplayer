using Myra.Graphics2D.UI;
using Resources;

namespace JKMP.Plugin.Multiplayer.Game.UI.Widgets
{
    public class Chat : Panel
    {
        private readonly ScrollViewer outputScrollViewer;
        private readonly VerticalStackPanel chatOutput;
        
        public Chat()
        {
            VerticalStackPanel panel = AddChild(ResourceManager.GetResourceWidget<VerticalStackPanel, object>("UI/Chat.xmmp", stylesheet: null, handler: null, UIManager.TypeResolver));
            outputScrollViewer = (ScrollViewer)panel.EnsureWidgetById("OutputScrollViewer");
            chatOutput = (VerticalStackPanel)panel.EnsureWidgetById("ChatOutputPanel");
        }
    }
}