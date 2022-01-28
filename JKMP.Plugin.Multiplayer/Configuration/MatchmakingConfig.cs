using JKMP.Core.Configuration.Attributes;
using JKMP.Core.UI.MenuFields;
using Newtonsoft.Json;

namespace JKMP.Plugin.Multiplayer.Configuration
{
    public class MatchmakingConfig
    {
        [JsonRequired]
        public string Endpoint { get; set; } = "jkmp-backend-matchmaking.fly.dev";

        [JsonRequired]
        public ushort Port { get; set; } = 10069;

        [TextField(Visibility = TextVisibility.HiddenWhenUnfocused, MaxLength = 10)]
        public string Password { get; set; } = "";
    }
}