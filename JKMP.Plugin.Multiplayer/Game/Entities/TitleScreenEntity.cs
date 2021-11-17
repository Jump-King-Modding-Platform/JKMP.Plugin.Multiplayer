using JKMP.Core.Logging;
using JKMP.Plugin.Multiplayer.Game.UI;
using JKMP.Plugin.Multiplayer.Game.UI.Widgets;
using Myra.Graphics2D.UI;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class TitleScreenEntity : BaseManagerEntity
    {
        private static readonly ILogger Logger = LogManager.CreateLogger<TitleScreenEntity>();

        protected override void OnFirstUpdate()
        {
            UIManager.PushShowCursor();

            var chatWidget = new Chat();
            UIManager.AddWidget(chatWidget);
        }

        protected override void OnDestroy()
        {
            UIManager.PopShowCursor();
        }
    }
}