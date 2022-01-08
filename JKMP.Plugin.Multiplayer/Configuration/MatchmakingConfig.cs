using Newtonsoft.Json;

namespace JKMP.Plugin.Multiplayer.Configuration
{
    public class MatchmakingConfig
    {
        [JsonRequired]
        public string Endpoint { get; set; } = "localhost";

        [JsonRequired]
        public ushort Port { get; set; } = 16000;
    }
}