using JKMP.Core.Logging;
using Serilog;

namespace JKMP.Plugin.Multiplayer.Game.Entities
{
    public class TitleScreenEntity : BaseManagerEntity
    {
        private static readonly ILogger Logger = LogManager.CreateLogger<TitleScreenEntity>();

        protected override void OnFirstUpdate()
        {
            
        }
    }
}