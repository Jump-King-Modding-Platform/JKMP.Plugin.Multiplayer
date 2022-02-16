using JKMP.Plugin.Multiplayer.Game.UI.MenuFields;
using JKMP.Plugin.Multiplayer.Native.Audio;
using Newtonsoft.Json;

namespace JKMP.Plugin.Multiplayer.Configuration
{
    public class VoiceConfig
    {
        [AudioInputSelectField(Name = "Microphone")]
        public DeviceInformation? SelectedDevice { get; set; }
        
        [AudioInputTestField(Name = "Test microphone")]
        [JsonIgnore]
        public bool UnusedInputTest { get; set; }
    }
}