using JKMP.Core.Configuration.Attributes;
using JKMP.Plugin.Multiplayer.Game.UI;

namespace JKMP.Plugin.Multiplayer.Configuration
{
    public class UiConfig
    {
        [SliderField(Description = "Requires restart", MinValue = 1f, MaxValue = 1.5f, StepSize = 0.1f)]
        public float Scale { get; set; } = 1f;
    }
}