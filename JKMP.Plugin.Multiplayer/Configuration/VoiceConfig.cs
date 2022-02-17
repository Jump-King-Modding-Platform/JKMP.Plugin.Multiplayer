using JKMP.Plugin.Multiplayer.Game.Components;
using JKMP.Plugin.Multiplayer.Game.UI.MenuFields;
using JKMP.Plugin.Multiplayer.Native.Audio;
using Newtonsoft.Json;

namespace JKMP.Plugin.Multiplayer.Configuration
{
    public class VoiceConfig
    {
        private string? selectedDeviceName;

        [AudioInputSelectField(Name = "Microphone")]
        public string? SelectedDeviceName
        {
            get => selectedDeviceName;
            set
            {
                selectedDeviceName = string.IsNullOrEmpty(value) ? null : value;
                VoiceManager.SelectedDeviceName = selectedDeviceName;
            }
        }

        [AudioInputTestField(Name = "Test microphone")]
        [JsonIgnore]
        private bool UnusedInputTest { get; set; }
    }
}