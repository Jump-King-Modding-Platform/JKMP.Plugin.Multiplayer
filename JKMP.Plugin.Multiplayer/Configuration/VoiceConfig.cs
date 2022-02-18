using JKMP.Core.Configuration.Attributes;
using JKMP.Plugin.Multiplayer.Game.Components;
using JKMP.Plugin.Multiplayer.Game.UI.MenuFields;
using JKMP.Plugin.Multiplayer.Native.Audio;
using Newtonsoft.Json;

namespace JKMP.Plugin.Multiplayer.Configuration
{
    public class VoiceConfig
    {
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

        [SliderField(Name = "Input", Description = "The volume of your microphone", MinValue = 0.5f, MaxValue = 2.5f, StepSize = 0.1f)]
        public float Volume
        {
            get => volume;
            set
            {
                volume = value;
                VoiceManager.Volume = value;
            }
        }

        [SliderField(Name = "Output", Description = "The volume of other players", MinValue = 0, MaxValue = 1, StepSize = 0.1f)]
        public float PlayerVolume
        {
            get => playerVolume;
            set
            {
                playerVolume = volume;
                VoiceManager.PlayerVolume = value;
            }
        }

        [AudioInputTestField(Name = "Test microphone")]
        [JsonIgnore]
        private bool UnusedInputTest { get; set; }
        
        private string? selectedDeviceName;
        private float volume = 1f;
        private float playerVolume = 1f;
    }
}