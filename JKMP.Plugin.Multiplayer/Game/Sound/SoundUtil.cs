using JumpKing.PlayerPreferences;

namespace JKMP.Plugin.Multiplayer.Game.Sound
{
    public static class SoundUtil
    {
        public static SoundPrefs SoundPrefs => ISettingManager<SoundPrefsRuntime, SoundPrefs>.instance.GetPrefs();
    }
}